using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SlickView
{
    public class SlickViewElementsEnumerator : IEnumerator<SlickViewElement>
    {
        public static bool OSX = Application.platform == RuntimePlatform.OSXEditor;

        public class InternalSlickViewState
        {
            public bool           beganHorizontal;
            public int            endRow;
            public int            invisibleRows;
            public int            rectHeight;
            public SlickViewState state;
        }

        private readonly int[] colWidths;

        private readonly InternalSlickViewState ilvState;
        private readonly int                    xTo;
        private readonly int                    yFrom;
        private readonly int                    yTo;
        private          SlickViewElement       element;

        private Rect firstRect;

        private bool quiting;
        private Rect rect;
        private int  xPos = -1;
        private int  yPos = -1;

        public SlickViewElementsEnumerator(InternalSlickViewState ilvState, int[] colWidths, int yFrom, int yTo, string dragTitle, Rect firstRect)
        {
            this.colWidths = colWidths;
            xTo            = colWidths.Length - 1;
            this.yFrom     = yFrom;
            this.yTo       = yTo;
            this.firstRect = firstRect;
            rect           = firstRect;
            quiting        = ilvState.state.totalRows == 0;

            this.ilvState = ilvState;

            Reset();
        }

        public bool MoveNext()
        {
            if (xPos > -1)
                if (HasMouseDown(ilvState, rect))
                {
                    var previousRow = ilvState.state.row;
                    ilvState.state.selectionChanged = true;
                    ilvState.state.row              = yPos;
                    ilvState.state.column           = xPos;
                    ilvState.state.scrollPos        = SlickViewScrollToRow(ilvState, yPos); // this is about clicking on a row that is partially visible
                }

            xPos++;

            if (xPos > xTo)
            {
                xPos = 0;
                yPos++;

                rect.x     = firstRect.x;
                rect.width = colWidths[0];

                if (yPos > yTo)
                    quiting = true;
                else // move vertically
                    rect.y += rect.height;
            }
            else // move horizontally
            {
                if (xPos >= 1)
                    rect.x += colWidths[xPos - 1];

                rect.width = colWidths[xPos];
            }

            element.row      = yPos;
            element.position = rect;

            if (element.row >= ilvState.state.totalRows)
                quiting = true;


            if (quiting)
            {
                if (SlickViewKeyboard(ilvState, colWidths.Length))
                    ilvState.state.selectionChanged = true;

                if (Event.current.GetTypeForControl(ilvState.state.ID) == EventType.MouseUp) GUIUtility.hotControl = 0;


                if (ilvState.beganHorizontal)
                {
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndHorizontal();
                    ilvState.beganHorizontal = false;
                }
            }

            return !quiting;
        }

        public static bool HasMouseDown(InternalSlickViewState ilvState, Rect r)
        {
            return HasMouseDown(ilvState, r, 0);
        }

        public static bool HasMouseDown(InternalSlickViewState ilvState, Rect r, int button)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == button)
                if (r.Contains(Event.current.mousePosition))
                {
                    GUIUtility.hotControl      = ilvState.state.ID;
                    GUIUtility.keyboardControl = ilvState.state.ID;
                    Event.current.Use();
                    return true;
                }

            return false;
        }

        public static bool SlickViewKeyboard(InternalSlickViewState ilvState, int totalCols)
        {
            var totalRows = ilvState.state.totalRows;

            if (Event.current.type != EventType.KeyDown || totalRows == 0)
                return false;

            if (GUIUtility.keyboardControl != ilvState.state.ID ||
                Event.current.GetTypeForControl(ilvState.state.ID) != EventType.KeyDown)
                return false;

            return SendKey(ilvState, Event.current.keyCode, totalCols);
        }

        public static bool SendKey(InternalSlickViewState ilvState, KeyCode keyCode, int totalCols)
        {
            var state = ilvState.state;

            var previousRow = state.row;

            //ilvState.state.row, ref ilvState.state.column, ref ilvState.state.scrollPos
            switch (keyCode)
            {
                case KeyCode.UpArrow:
                {
                    if (state.row > 0)
                        state.row--;
                    // If nothing is selected, upArrow selects the 1st element
                    else if (state.row == -1)
                        state.row = 0;
                    break;
                }
                case KeyCode.DownArrow:
                {
                    if (state.row < state.totalRows - 1)
                        state.row++;
                    break;
                }
                case KeyCode.Home:
                {
                    state.row = 0;
                    break;
                }
                case KeyCode.End:
                {
                    state.row = state.totalRows - 1;
                    break;
                }
                case KeyCode.LeftArrow:
                    if (state.column > 0)
                        state.column--;
                    break;
                case KeyCode.RightArrow:
                    if (state.column < totalCols - 1)
                        state.column++;
                    break;

                case KeyCode.PageUp:
                {
                    if (!DoLVPageUpDown(ilvState, ref state.row, ref state.scrollPos, true))
                    {
                        Event.current.Use();
                        return false;
                    }

                    break;
                }
                case KeyCode.PageDown:
                {
                    if (!DoLVPageUpDown(ilvState, ref state.row, ref state.scrollPos, false))
                    {
                        Event.current.Use();
                        return false;
                    }

                    break;
                }
                case KeyCode.A: // must evade the return false and be handled by MultiSelection if needed
                    break;
                default:
                    return false;
            }

            state.scrollPos = SlickViewScrollToRow(ilvState, state.scrollPos, state.row);
            Event.current.Use();
            return true;
        }

        private static bool DoLVPageUpDown(InternalSlickViewState ilvState, ref int selectedRow, ref Vector2 scrollPos, bool up)
        {
            var visibleRows = ilvState.endRow - ilvState.invisibleRows;

            if (up)
            {
                if (OSX)
                {
                    scrollPos.y -= ilvState.state.rowHeight * visibleRows;

                    if (scrollPos.y < 0)
                        scrollPos.y = 0;
                }
                else
                {
                    selectedRow -= visibleRows;

                    if (selectedRow < 0)
                        selectedRow = 0;

                    return true;
                }
            }
            else
            {
                if (OSX)
                {
                    scrollPos.y += ilvState.state.rowHeight * visibleRows;
                    //FIXME: does this need an upper bound check?
                }
                else
                {
                    selectedRow += visibleRows;

                    if (selectedRow >= ilvState.state.totalRows)
                        selectedRow = ilvState.state.totalRows - 1;

                    return true;
                }
            }

            return false;
        }

        public static Vector2 SlickViewScrollToRow(InternalSlickViewState ilvState, int row)
        {
            return SlickViewScrollToRow(ilvState, ilvState.state.scrollPos, row);
        }

        public static Vector2 SlickViewScrollToRow(InternalSlickViewState ilvState, Vector2 currPos, int row)
        {
            if (ilvState.invisibleRows < row && ilvState.endRow > row)
                return currPos;

            if (row <= ilvState.invisibleRows)
                currPos.y = ilvState.state.rowHeight * row;
            else
                currPos.y = ilvState.state.rowHeight * (row + 1) - ilvState.rectHeight;

            if (currPos.y < 0)
                currPos.y = 0;
            else if (currPos.y > ilvState.state.totalRows * ilvState.state.rowHeight - ilvState.rectHeight)
                currPos.y = ilvState.state.totalRows * ilvState.state.rowHeight - ilvState.rectHeight;

            return currPos;
        }

        public void Reset()
        {
            xPos = -1;
            yPos = yFrom;
        }

        SlickViewElement IEnumerator<SlickViewElement>.Current => element;

        object IEnumerator.Current => element;

        public void Dispose()
        {
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }
    }
}
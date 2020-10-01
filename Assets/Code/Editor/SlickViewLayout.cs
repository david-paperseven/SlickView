using System;
using UnityEditor.Experimental.TerrainAPI;
using UnityEditor.UIElements;
using UnityEngine;

namespace SlickView
{
    public class SlickViewLayout
    {
        private SlickViewState          _slickViewState;
        private int                     _LVHeight;
        private Action<Rect, int, bool> _draw;
        public  SlickViewState          SlickViewState => _slickViewState;
        private GUIStyle                _style;
        private int                     _selectedRow;

        public SlickViewLayout(int totalRows, int rowHeight, Action<Rect, int, bool> draw)
        {
            _draw           = draw;
            _slickViewState = new SlickViewState(totalRows, rowHeight);
        }


        public bool Draw(Rect rect, int totalRows, GUIStyle style)
        {
            Event e = Event.current;

            bool selectionChanged = false;
            _slickViewState.visRect   = rect;
            _slickViewState.totalRows = totalRows;

            foreach (SlickViewElement el in SlickView.ListView(_slickViewState, style))
            {
                if (e.type == EventType.MouseDown && e.button == 0 && el.position.Contains(e.mousePosition))
                {
                    _selectedRow     = _slickViewState.row;
                    selectionChanged = true;
                }
                else if (e.type == EventType.Repaint)
                {
                    _draw(el.position, el.row, _selectedRow == el.row);
                }
            }

            return selectionChanged;
        }
    }
}
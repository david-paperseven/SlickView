using System;
using UnityEditor;
using UnityEngine;

namespace SlickView
{
    public class SlickView
    {
        public static           SlickViewElementsEnumerator.InternalSlickViewState ilvState     = new SlickViewElementsEnumerator.InternalSlickViewState();
        private static readonly int                                                slickViewHash = "SlickView".GetHashCode();

        public static SlickViewElementsEnumerator ListView(SlickViewState state, GUIStyle style, params GUILayoutOption[] options)
        {
            return ListView(state, null, string.Empty, style, options);
        }

        public static SlickViewElementsEnumerator ListView(SlickViewState state, int[] colWidths, string dragTitle, GUIStyle style,
                                                           params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(style);
            state.scrollPos          = EditorGUILayout.BeginScrollView(state.scrollPos, options);
            ilvState.beganHorizontal = true;
            return DoListView(GUILayoutUtility.GetRect(1, state.totalRows * state.rowHeight + 3), state, colWidths, string.Empty);
        }

        public static SlickViewElementsEnumerator DoListView(Rect pos, SlickViewState state, int[] colWidths, string dragTitle)
        {
            var id = GUIUtility.GetControlID(slickViewHash, FocusType.Passive);
            state.ID = id;

            state.selectionChanged = false;
            Rect vRect;
            vRect = pos.y < 0
                ? new Rect(0, 0, state.visRect.width, state.visRect.height)
                : new Rect(0, state.scrollPos.y, state.visRect.width, state.visRect.height); // check if this is custom scroll

            if (vRect.width <= 0)
                vRect.width = 1;
            if (vRect.height <= 0)
                vRect.height = 1;

            var invisibleRows = (int) ((-pos.y + vRect.yMin) / state.rowHeight);
            var endRow        = invisibleRows + (int) Math.Ceiling(((vRect.yMin - pos.y) % state.rowHeight + vRect.height) / state.rowHeight) - 1;

           ilvState.invisibleRows = invisibleRows;
            ilvState.endRow        = endRow;
            ilvState.rectHeight    = (int) vRect.height;
            ilvState.state         = state;

            if (invisibleRows < 0)
                invisibleRows = 0;

            if (endRow >= state.totalRows)
                endRow = state.totalRows - 1;

            return new SlickViewElementsEnumerator(ilvState, invisibleRows, endRow, dragTitle, new Rect(0, invisibleRows * state.rowHeight, pos.width, state.rowHeight));
        }
    }
}
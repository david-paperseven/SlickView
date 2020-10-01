using System;
using UnityEngine;

namespace SlickView
{
    [Serializable]
    public class SlickViewState
    {
        public int     row;
        public bool[]  selectedItems;
        public int     initialRow = -1;
        public int     column;
        public Vector2 scrollPos;
        public int     totalRows;
        public int     rowHeight;
        public Rect    visRect;

        public int  ID;
        public bool selectionChanged;

        public SlickViewState(int totalRows, int rowHeight)
        {
            row            = -1;
            column         = 0;
            scrollPos      = Vector2.zero;
            this.totalRows = totalRows;
            this.rowHeight = rowHeight;

            selectionChanged = false;
        }
    }

    public struct SlickViewElement
    {
        public int  row;
        public Rect position;
    }
}
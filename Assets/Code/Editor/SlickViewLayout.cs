using System;
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


        /// <summary>
        /// Draw
        /// Called during the OnGUI of the client code
        /// for SlickView to render its view
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="totalRows"></param>
        /// <param name="style"></param>
        public void Draw(Rect rect, int totalRows, GUIStyle style)
        {
            Event e = Event.current;

            _slickViewState.visRect   = rect;
            _slickViewState.totalRows = totalRows;

            // iterate through all the rows that are _visible_ in the view 
            // and call the draw delegate
            foreach (SlickViewElement el in SlickView.ListView(_slickViewState, style))
            {
                if (e.type == EventType.MouseDown && e.button == 0 && el.position.Contains(e.mousePosition))
                {
                    _selectedRow = _slickViewState.row;
                }
                else if (e.type == EventType.Repaint)
                {
                    _draw(el.position, el.row, _selectedRow == el.row);
                }
            }
        }
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SlickView
{
    public class SlickViewWindow : EditorWindow
    {
        private SlickViewLayout _slickViewLayout;

        private static SlickViewWindow _instance;
        public static  GUIStyle        Box;
        public static  GUIStyle        EvenBackground;
        public static  GUIStyle        OddBackground;
        public static  GUIStyle        LogStyle;
        private        bool            _initialisedGUIStyles;
        Texture2D[]                    _textures;
        private Vector2                _scrollPos;
        private bool                   _slickview = true;
        private Rect[]                 _columnRects;
        GUIContent                     _tempContent = new GUIContent();

        public struct ExampleListElement
        {
            public ExampleListElement(int index, Texture2D texture2D)
            {
                elementNumber = index.ToString();
                name          = texture2D.name;
                width         = texture2D.width;
                height        = texture2D.height;
                texture       = texture2D;
                columnWidth   = new[] {60, 240, 60, 60, 50};
            }

            public string    elementNumber;
            public string    name;
            public int       width;
            public int       height;
            public Texture2D texture;
            public int[]     columnWidth;
        }

        private List<ExampleListElement> _listElements = new List<ExampleListElement>(1000);

        [MenuItem("SlickView/Open Demo Window", false, 0)]
        public static void CreateDeviceWindow()
        {
            _instance = GetWindow<SlickViewWindow>("SlickView");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void AfterAssembliesLoaded()
        {
            _instance = GetWindow<SlickViewWindow>("SlickView");
            _instance.InitVars();
        }

        void Init()
        {
            if (!_initialisedGUIStyles)
            {
                Box                   = "CN Box";
                EvenBackground        = "CN EntryBackEven";
                OddBackground         = "CN EntryBackodd";
                LogStyle              = "CN EntryInfoSmall";
                _initialisedGUIStyles = true;
            }
        }

        void InitVars()
        {
            _listElements.Clear();
            _slickViewLayout = new SlickViewLayout(32, 24, DrawElement);
            _textures        = Resources.FindObjectsOfTypeAll<Texture2D>();
            for (int i = 0; i < _listElements.Capacity; i++)
            {
                _listElements.Add(new ExampleListElement(i, _textures[i % _textures.Length]));
            }

            _initialisedGUIStyles = false;
        }

        private void OnEnable()
        {
            position              = new Rect(200, 200, 300, 400);
            wantsLessLayoutEvents = true;
            titleContent          = new GUIContent("SlickView");
            InitVars();
        }

        private void OnGUI()
        {
            Init();

            _slickViewLayout.Draw(position, _listElements.Capacity, Box);
        }


        void DrawElement(Rect rect, int row, bool selected)
        {
            ExampleListElement el = _listElements[row];
            GUIStyle           s  = (row & 1) == 0 ? OddBackground : EvenBackground;
            s.Draw(rect, false, false, selected, false);
            int column = 0;
            DrawLabel(ref rect, el.elementNumber, el.columnWidth[column++], false);
            DrawLabel(ref rect, el.name, el.columnWidth[column++], false);
            DrawInt(ref rect, el.width, el.columnWidth[column++]);
            DrawInt(ref rect, el.height, el.columnWidth[column++]);
            DrawTexture(ref rect, el.texture, el.columnWidth[column]);
        }

        void DrawInt(ref Rect r, int value, int width)
        {
            r.width = width;
            EditorGUI.IntField(r, width);
            r.x += width;
        }

        void DrawLabel(ref Rect r, string value, int width, bool selected)
        {
            r.width           = width;
            _tempContent.text = value;
            LogStyle.Draw(r, _tempContent, GUIUtility.GetControlID(0), selected);
            r.x += width;
        }

        void DrawTexture(ref Rect r, Texture2D value, int width)
        {
            r.width = width;
            EditorGUI.ObjectField(r, value, typeof(Texture2D), false);
            r.x += width;
        }
    }
}
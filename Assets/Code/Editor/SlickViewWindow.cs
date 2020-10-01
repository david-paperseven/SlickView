using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SlickView
{
    public class SlickViewWindow : EditorWindow
    {
        private SlickViewLayout _slickViewLayout;

        private static SlickViewWindow _instance;
        private static GUIStyle        Box;
        private static GUIStyle        EvenBackground;
        private static GUIStyle        OddBackground;
        private static GUIStyle        LogStyle;
        private        bool            _initialisedGUIStyles;
        Texture2D[]                    _textures;
        private Vector2                _scrollPos;
        private bool                   _slickview = true;
        private Rect[]                 _columnRects;
        GUIContent                     _tempContent = new GUIContent();

        /// <summary>
        /// An example list view element for the demo window
        /// </summary>
        public struct ExampleListElement
        {
            public ExampleListElement(int index, Texture2D texture2D)
            {
                elementNumber = index.ToString();
                name          = texture2D.name;
                width         = texture2D.width;
                height        = texture2D.height;
                format        = texture2D.format;
                texture       = texture2D;
                columnWidth   = new[] {60, 240, 60, 60, 100, 21};
            }

            public string        elementNumber;
            public string        name;
            public int           width;
            public int           height;
            public TextureFormat format;
            public Texture2D     texture;
            public int[]         columnWidth;
        }

        private List<ExampleListElement> _listElements = new List<ExampleListElement>(10000);

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

        /// <summary>
        /// InitStyles
        /// This has to be called from OnGUI rather than a initialisation step
        /// </summary>
        void InitStyles()
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

        /// <summary>
        /// InitVars
        /// Set up all the elements for the demo window
        /// This happens whenever the window is opened or the assemblies have been loaded
        /// </summary>
        void InitVars()
        {
            _listElements.Clear();

            // create the slick view class and give it a starting size and delegate for drawing each element
            _slickViewLayout = new SlickViewLayout(32, 21, DrawElement);

            // the demo window finds all the textures in the project and displays them in order
            // over and over until it reaches the specified capacity
            _textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            for (int i = 0; i < _listElements.Capacity; i++)
            {
                _listElements.Add(new ExampleListElement(i, _textures[i % _textures.Length]));
            }

            _initialisedGUIStyles = false;
        }

        private void OnEnable()
        {
            position = new Rect(200, 200, 300, 400);
            //   wantsLessLayoutEvents = true;
            titleContent = new GUIContent("SlickView");
            InitVars();
        }

        private void OnGUI()
        {
            InitStyles();

            _slickViewLayout.Draw(position, _listElements.Capacity, Box);

            // uncomment this to compare performance with SlickView
            // ExistingScrollView();
        }

        /// <summary>
        /// ExistingScrollView
        /// Example implementation using the existing ScrollView
        /// </summary>
        void OldScrollView()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var el in _listElements)
            {
                EditorGUILayout.TextField(el.name);
                EditorGUILayout.IntField(el.width);
            }

            EditorGUILayout.EndScrollView();
        }


        /// <summary>
        /// DrawElement
        /// Called for each row that is visible
        /// </summary>
        /// <param name="rect"></param>
        /// Area in which to render the elements
        /// <param name="row"></param>
        /// Row number in the underlying list
        /// <param name="selected"></param>
        /// Is this row currently selected by the user
        void DrawElement(Rect rect, int row, bool selected)
        {
            ExampleListElement el = _listElements[row];
            GUIStyle           s  = (row & 1) == 0 ? OddBackground : EvenBackground;
            s.Draw(rect, false, false, selected, false);
            int column = 0;

            // draw some info using the row number as an index into the underlying element array
            DrawLabel(ref rect, el.elementNumber, el.columnWidth[column++], selected);
            DrawLabel(ref rect, el.name, el.columnWidth[column++], selected);
            DrawInt(ref rect, el.width, el.columnWidth[column++]);
            DrawInt(ref rect, el.height, el.columnWidth[column++]);
            DrawEnum(ref rect, el.format, el.columnWidth[column++]);
            DrawTexture(ref rect, el.texture, el.columnWidth[column]);
        }

        /// Some helper functions for displaying different types 
        void DrawInt(ref Rect r, int value, int width)
        {
            r.width = width;
            EditorGUI.IntField(r, value);
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
            EditorGUI.DrawPreviewTexture(r, value);
            r.x += width;
        }

        void DrawEnum(ref Rect r, Enum value, int width)
        {
            r.width = width;
            EditorGUI.EnumPopup(r, value);
            r.x += width;
        }
    }
}
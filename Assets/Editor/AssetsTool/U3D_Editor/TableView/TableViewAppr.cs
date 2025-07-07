using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace XGameEditor.GlacierEditor.TableView
{
    public class TableViewAppr
    {
        public float LineHeight
        {
            get { return _lineHeight; }
            set { _lineHeight = value; }
        }
        float _lineHeight = 25;

        public string GetSortMark(bool descending)
        {
            return descending ? " ▼" : " ▲";
        }

        private static Dictionary<Color, Texture2D> s_colorTextures = new Dictionary<Color, Texture2D>();
        public static Texture2D GetColorTexture(Color c)
        {
            Texture2D tex = null;
            s_colorTextures.TryGetValue(c, out tex);
            if (tex == null)
            {
                tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, c);
                tex.Apply();

                s_colorTextures[c] = tex;
            }
            return tex;
        }

        public GUIStyle GetTitleStyle(bool selected)
        {
            if (_styleTitle == null || _titleOrdinary == null || _titleSelected == null)
            {
                _styleTitle = new GUIStyle(EditorStyles.whiteBoldLabel);
                _styleTitle.alignment = TextAnchor.MiddleCenter;
                _titleOrdinary = GetColorTexture(new Color32(38, 158, 111, 255));
                _titleSelected = GetColorTexture(new Color32(19, 80, 60, 255));
            }

            _styleTitle.normal.background = selected ? _titleSelected : _titleOrdinary;
            _styleTitle.normal.textColor = selected ? Color.yellow : Color.white;
            return _styleTitle;
        }
        private GUIStyle _styleTitle;
        private Texture2D _titleOrdinary;
        private Texture2D _titleSelected;

        public GUIStyle Style_Line
        {
            get
            {
                if (_styleLine == null)
                {
                    _styleLine = new GUIStyle(EditorStyles.whiteLabel);
                    _styleLine.normal.background = GetColorTexture(new Color(0.5f, 0.5f, 0.5f, 0.1f));
                    _styleLine.normal.textColor = Color.white;
                }
                return _styleLine;
            }
        }
        private GUIStyle _styleLine;

        public GUIStyle Style_LineAlt
        {
            get
            {
                if (_styleLineAlt == null)
                {
                    _styleLineAlt = new GUIStyle(EditorStyles.whiteLabel);
                    _styleLineAlt.normal.background = GetColorTexture(new Color(0.5f, 0.5f, 0.5f, 0.2f));
                    _styleLineAlt.normal.textColor = Color.white;
                }
                return _styleLineAlt;
            }
        }
        private GUIStyle _styleLineAlt;

        public GUIStyle Style_Selected
        {
            get
            {
                if (_styleSelected == null)
                {
                    _styleSelected = new GUIStyle(EditorStyles.whiteLabel);
                    _styleSelected.normal.background = GetColorTexture(new Color32(62, 95, 150, 255));
                    _styleSelected.normal.textColor = Color.white;
                }
                return _styleSelected;
            }
        }
        private GUIStyle _styleSelected;

        public GUIStyle Style_SelectedCell
        {
            get
            {
                if (_styleSelectedCell == null)
                {
                    _styleSelectedCell = new GUIStyle(EditorStyles.whiteBoldLabel);
                    _styleSelectedCell.normal.background = GetColorTexture(new Color32(62, 95, 150, 128));
                    _styleSelectedCell.normal.textColor = Color.yellow;
                }
                return _styleSelectedCell;
            }
        }
        private GUIStyle _styleSelectedCell;
    }

}


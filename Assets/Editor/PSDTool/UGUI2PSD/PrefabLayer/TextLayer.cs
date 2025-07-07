using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI2PSD
{
    public class TextLayer : PrefabLayerBase
    {
        public override EPrefabLayerType LayerType => EPrefabLayerType.Text;

        public string Text
        {
            get;
            private set;
        }

        public double FontSize
        {
            get;
            private set;
        }

        public string FontName
        {
            get;
            private set;
        }

        public bool FauxBold
        {
            get;
            private set;
        }

        public bool FauxItalic
        {
            get;
            private set;
        }

        public bool Underline
        {
            get;
            private set;
        }

        public Color FillColor
        {
            get;
            private set;
        }

        public double OutlineWidth
        {
            get;
            private set;
        }

        public Color StrokeColor
        {
            get;
            private set;
        }

        private Text textComp;

        public TextLayer(Transform tran) : base(tran)
        {
            textComp = tran.GetComponent<Text>();
            ResolveLayerInfo();
        }

        public override void ResolveLayerInfo()
        {
            Text = textComp.text.TrimEnd((char)10) + "\r"; //去掉结尾的换行符
            Debug.Log(Text);
            FontSize = textComp.fontSize;
            FontName = textComp.font.name;
            FauxBold = textComp.fontStyle == FontStyle.Bold || textComp.fontStyle == FontStyle.BoldAndItalic;
            FauxItalic = textComp.fontStyle == FontStyle.Italic || textComp.fontStyle == FontStyle.BoldAndItalic;
            Underline = false;
            FillColor = textComp.color;

            Outline outline = textComp.GetComponent<Outline>();
            if (outline)
            {
                OutlineWidth = outline.effectDistance.x;
                StrokeColor = outline.effectColor;
            }
            else
            {
                OutlineWidth = 1.0d;
                StrokeColor = Color.white;
            }
        }
    }
}

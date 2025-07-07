﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WXB
{
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/SymbolLabel")]
    public class SymbolLabel : SymbolText
    {
        protected override void Awake()
        {
            base.Awake();
            m_textDirty = false;
        }

        public override void SetTextDirty()
        {
        }

        public override void SetLayoutDirty()
        {
            base.SetLayoutDirty();
            SetMaterialDirty();
            SetRenderDirty();
            m_layoutDirty = true;
            m_textDirty = false;
        }

        public override string text
        {
            set
            {
                throw new NotSupportedException("text");
            }
        }

        [SerializeField]
        int m_MaxElement = 30; // 最大元素个数

        public int MaxElement
        {
            get { return m_MaxElement; }
            set
            {
                m_MaxElement = value;
            }
        }

        /// <summary>
        /// 追加内容，追加内容的时候，如果节点数量超过了最大值，则将最开始的节点移除
        /// </summary>
        /// <param name="text"></param>
        public void Append(string text)
        {
            LinkedListNode<NodeBase> itor = mNodeList.Last;
            int endl = 0;
            if (itor != null)
            {
                endl = (int)(itor.Value.userdata);
                itor = mNodeList.First;
                while (itor != null)
                {
                    if (endl - ((int)itor.Value.userdata) >= MaxElement)
                    {
                        //节点回收
                        var node = itor.Value;
                        UHyperTextFactory.ReleaseNode(node);

                        mNodeList.RemoveFirst();
                        itor = mNodeList.First;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            ++endl;
            s_nodebases.Clear();

            InitParserConfig();
            Parser.parser(this, text, ref s_ParserConfig, s_nodebases, getExternalNode);
            s_nodebases.ForEach((NodeBase nb) =>
            {
                nb.userdata = endl;
                mNodeList.AddLast(nb);
            });

            s_nodebases.back().setNewLine(true);
            s_nodebases.Clear();

            SetRenderDirty();
            SetLayoutDirty();
        }
    }
}

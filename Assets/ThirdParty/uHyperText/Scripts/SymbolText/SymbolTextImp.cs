using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WXB
{
    public partial class SymbolText
    {
        public override float preferredWidth
        {
            get
            {
                if (font == null)
                    return 0f;

                UpdateByDirty();
                return getNodeWidth();
            }
        }

        public override float preferredHeight
        {
            get
            {
                if (font == null)
                    return 0f;

                UpdateByDirty();
                return getNodeHeight();
            }
        }

        public override void SetAllDirty()
        {
            base.SetAllDirty();
            SetTextDirty();
        }

        public void UpdateByDirty()
        {
            if (m_textDirty)
            {
                //构建节点
                UpdateByTextDirty();
                m_textDirty = false;

                //构建行
                UpdateTextHeight();
                m_layoutDirty = false;

                UpdateRenderElements();
                m_renderNodeDirty = false;

                //需要同步背景
                isWillSyncBackground = true;
            }

            if (m_layoutDirty)
            {
                UpdateTextHeight();
                m_layoutDirty = false;

                UpdateRenderElements();
                m_renderNodeDirty = false;

                isWillSyncBackground = true;
            }

            if (m_renderNodeDirty)
            {
                UpdateRenderElements();
                m_renderNodeDirty = false;

                isWillSyncBackground = true;
            }
        }

        public override void Rebuild(CanvasUpdate update)
        {
            if (canvasRenderer.cull)
                return;

            if (pixelsPerUnit >= 10f)
                return;

            if (font == null)
            {
                return;
            }

            switch (update)
            {
                case CanvasUpdate.PreRender:
                    {
                        //先进行预处理
                        UpdateByDirty();

                        base.Rebuild(update);
                    }
                    break;
            }
        }

        public virtual void SetRenderNodeDirty()
        {
            m_renderNodeDirty = true;
            SetVerticesDirty();

            //if (isActiveAndEnabled)
            //    CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void SetTextDirty()
        {
            m_textDirty = true;
            m_renderNodeDirty = true;

            SetMaterialDirty();
            FreeDraws();

            if (isActiveAndEnabled)
                CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        protected override void UpdateMaterial()
        {
            if (!IsActive())
                return;

            if (m_UsedDraws.Count == 0)
            {
                return;
            }

            var components = ListPool<Component>.Get();
            GetComponents(typeof(IMaterialModifier), components);

            for (int i = 0; i < m_UsedDraws.Count; ++i)
            {
                IDraw draw = m_UsedDraws[i];
                if (draw.srcMat == null)
                    draw.srcMat = material;

                Material currentMat = draw.srcMat;
                for (var m = 0; m < components.Count; m++)
                    currentMat = (components[m] as IMaterialModifier).GetModifiedMaterial(currentMat);

                draw.UpdateMaterial(currentMat);
            }

            ListPool<Component>.Release(components);
        }

        Around d_Around = new Around();

        public Around around { get { return d_Around; } }

        public ElementSegment elementSegment
        {
            get
            {
                if (string.IsNullOrEmpty(m_ElementSegment))
                    return null;

                return ESFactory.Get(m_ElementSegment);
            }
        }

        //当前解析得到的所有节点
        protected static List<NodeBase> s_nodebases = new List<NodeBase>();

        // 得到外部结点
        public System.Func<TagAttributes, IExternalNode> getExternalNode { get; set; }

        // 根据新文本，解析结点
        public void UpdateByTextDirty()
        {
            Clear();

            s_nodebases.Clear();

            InitParserConfig();
            Parser.parser(this, text, ref s_ParserConfig, s_nodebases, getExternalNode);

            s_nodebases.ForEach((NodeBase nb) => {
                mNodeList.AddLast(nb);
            });

            s_nodebases.Clear();
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            SetMaterialDirty();
        }

        Vector2 last_size = new Vector2(-1000f, -1000f);

        protected override void OnRectTransformDimensionsChange()
        {
            if (gameObject.activeInHierarchy)
            {
                // prevent double dirtying...
                if (CanvasUpdateRegistry.IsRebuildingLayout())
                {
                    if (last_size == rectTransform.rect.size)
                        return;

                    SetLayoutDirty();
                }
                else
                {
                    if (last_size != rectTransform.rect.size)
                        SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

        public override void SetLayoutDirty()
        {
            base.SetLayoutDirty();
            SetMaterialDirty();
            SetRenderDirty();
            m_textDirty = true;
            m_layoutDirty = true;
        }

        /// <summary>
        /// 更新高度
        /// </summary>
        public void UpdateTextHeight()
        {
            if (pixelsPerUnit <= 0f)
                return;

            renderCache.Release();

            //显示区域的最大宽度(起始就是RectTransform的宽度)
            float w = rectTransform.rect.size.x /** pixelsPerUnit*/;

            //先释放所有的行
            ReleaseAllLines();

            if (w <= 0f)
                return;

            //保存所有的Sprite的矩形区域，用来计算边界
            d_Around.Clear();
            foreach (NodeBase node in mNodeList)
            {
                if (node is RectSpriteNode)
                {
                    RectSpriteNode rsn = node as RectSpriteNode;
                    d_Around.Add(rsn.rect);
                }
            }

            //先添加一行空白行
            mLines.Add(UHyperTextFactory.CreateLine(Vector2.zero));

            //当前的顶部位置
            Vector2 currentpos = Vector2.zero;
            float scale = pixelsPerUnit;
            foreach (NodeBase node in mNodeList)
                node.FillToLine(ref currentpos, mLines, w, scale);

            for (int i = 0; i < mLines.Count; ++i)
            {
                mLines[i].y = Mathf.Max(mLines[i].y, m_MinLineHeight);
            }
        }

        // 更新渲染的文本
        public void UpdateRenderElements()
        {
            if (pixelsPerUnit <= 0f)
                return;

            FreeDraws();
            renderCache.Release();

            Rect inputRect = rectTransform.rect;
            float w = inputRect.size.x/* * pixelsPerUnit*/;
            if (w <= 0f)
                return;

            float x = 0f;
            float offsetY = 0f;
            uint yline = 0;
            LinkedListNode<NodeBase> itor = mNodeList.First;
            while (itor != null)
            {
                var n = itor.Value;
                if (n is LineNode)
                {
                    var lineNode = (n as LineNode);
                    offsetY += lineNode.offset;
                    offsetY += lineSpacing;
                }

                itor.Value.render(w, renderCache, ref x, ref yline, mLines, 0f, offsetY);
                itor = itor.Next;
            }

        }

        public void FontTextureChangedOther()
        {
            // Only invoke if we are not destroyed.
            if (!this)
            {
                return;
            }

            if (m_DisableFontTextureRebuiltCallback)
                return;

            if (!IsActive())
                return;

            // this is a bit hacky, but it is currently the
            // cleanest solution....
            // if we detect the font texture has changed and are in a rebuild loop
            // we just regenerate the verts for the new UV's
            if (CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout())
                UpdateGeometry();
            else
                SetAllDirty();
        }

    }
}

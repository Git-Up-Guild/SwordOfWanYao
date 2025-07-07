using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WXB
{
    /// <summary>
    /// 渲染数据的基类
    /// </summary>
    public abstract class BaseRenderData
    {
        //渲染矩形
        private Rect m_Rect;
        public Rect rect
        {
            set { m_Rect = value; }
            get { return m_Rect; }
        }

        //关联的节点
        public NodeBase node = null;

        // 当前所处的行
        public Line line { get; protected set; }

        //是否包含了指定的点
        public bool isContain(Vector2 pos)
        {
            var rect = new Rect(m_Rect);
            rect.y = rect.y + line.y - rect.height;

            return rect.Contains(pos);
        }

        public virtual bool isAlignByGeometry
        {
            get { return false; }
        }

        public abstract void Render(VertexHelper vh, Rect area, Vector2 offset, float pixelsPerUnit);

        //鼠标进入
        public virtual void OnMouseEnter() { }

        //鼠标离开
        public virtual void OnMouseLevel() { }

        //鼠标弹起
        public virtual void OnMouseUp(PointerEventData eventData) { }

        public int subMaterial { get; set; }

        public override string ToString()
        {
            return string.Format("rect:{0}", m_Rect);
        }

        public virtual void Release()
        {
            node = null;
            line = null;
            OnRelease();
        }

        protected abstract void OnRelease();

        public virtual void OnAlignByGeometry(ref Vector2 offset, float pixelsPerUnit)
        {
        }

        public virtual void OnLineYCheck(float pixelsPerUnit)
        {
        }

        protected LineAlignment lineAlignment
        {
            get
            {
                return (node.lineAlignment == LineAlignment.Default ? node.owner.lineAlignment : node.lineAlignment);
            }
        }

        public virtual Vector2 GetStartLeftBottom(float unitsPerPixel)
        {
            if (line == null)
            {
                return new Vector2(rect.x, rect.y + rect.height);
            }

            Vector2 leftBottomPos = new Vector2(rect.x, rect.y + rect.height);
            var la = lineAlignment;
            switch (la)
            {
                case LineAlignment.Top:
                    break;
                case LineAlignment.Center:
                    {
                        if (line.y == rect.height)
                        {
                        }
                        else
                        {
                            float offset = ((line.y - rect.height) * 0.5f);
                            leftBottomPos.y += offset;
                        }
                    }
                    break;

                case LineAlignment.Bottom:
                    leftBottomPos.y = rect.y + line.y;
                    break;
            }

            return leftBottomPos;
        }
    }
}
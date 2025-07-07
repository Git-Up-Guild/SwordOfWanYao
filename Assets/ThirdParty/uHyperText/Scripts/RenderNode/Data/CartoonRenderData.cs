using UnityEngine;
using UnityEngine.UI;

namespace WXB
{
    // 缓存渲染元素
    public class CartoonRenderData : BaseRenderData
    {
        public Cartoon cartoon;

        public void Reset(NodeBase n, Cartoon c, Rect r, Line l)
        {
            node = n;
            cartoon = c;
            rect = r;
            line = l;
        }

        protected override void OnRelease()
        {
            cartoon = null;
        }

        public override void Render(VertexHelper vh, Rect area, Vector2 offset, float pixelsPerUnit)
        {
            Color currentColor = node.d_color;
            if (currentColor.a <= 0.01f)
                return;

            Vector2 leftPos = GetStartLeftBottom(1f) + offset;
            Tools.LB2LT(ref leftPos, area.height);

            CartoonDraw cd = node.owner.GetDraw(DrawType.Cartoon, node.keyPrefix + cartoon.GetHashCode(),
                (IDraw d, object p) =>
                {
                    CartoonDraw cad = d as CartoonDraw;
                    cad.cartoon = cartoon;
                    cad.isOpenAlpha = node.d_bBlink;
                    cad.isOpenOffset = false;
                }) as CartoonDraw;

            cd.Add(leftPos, rect.width, rect.height, currentColor);
        }
    }
}

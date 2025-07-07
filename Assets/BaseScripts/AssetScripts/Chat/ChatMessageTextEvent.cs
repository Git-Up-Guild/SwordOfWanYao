using UnityEngine;
using WXB;
using XGame.LOP;

namespace XGame.AssetScript.Chat
{
    public class ChatMessageTextEvent : SymbolTextEvent
    {
        private ChatMessageItem item;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnClickAny.AddListener(OnSympleTextAnyClick);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnClickAny.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            item = null;
        }

        private void OnSympleTextAnyClick(NodeBase node)
        {
            if (item == null)
                item = GetComponentInParent<ChatMessageItem>();

            var clickType = EnChatViewClickType.None;
            var clickData1 = string.Empty;
            var clickData2 = string.Empty;

            ParseClickNode(node, ref clickType, ref clickData1, ref clickData2);

            item?.OnAnyClick(clickType, clickData1, clickData2);
        }

        private void ParseClickNode(NodeBase node, ref EnChatViewClickType clickTyp, ref string clickData1, ref string clickData2)
        {
            //点击到超链接
            if (node is HyperlinkNode)
            {
                var n = node as HyperlinkNode;
                clickTyp = EnChatViewClickType.Link;
                clickData1 = n.d_link;
                clickData2 = string.Empty;
            }
            //点击到图片
            else if (node is SpriteNode)
            {
                var n = node as SpriteNode;
                clickTyp = EnChatViewClickType.Sprite;
                clickData1 = n.id;
                clickData2 = n.spriteName;
            }
            //点击到空白
            else
            {
                clickTyp = EnChatViewClickType.Empty;
                clickData1 = string.Empty;
                clickData2 = string.Empty;
            }
        }
    }
}

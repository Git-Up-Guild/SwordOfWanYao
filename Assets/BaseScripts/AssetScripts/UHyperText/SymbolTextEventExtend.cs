/**************************************************************************    
文　　件：SymbolTextEventExtend
作　　者：郑秀程
创建时间：2021/2/11 12:53:53
描　　述：富文本点击处理
***************************************************************************/

using System;
using WXB;

namespace XGame.AssetScript.UHyperText
{
    public class SymbolTextEventExtend : SymbolTextEvent
    {
        private Action<string> m_OnClickSpriteCallbacks;
        private Action<string> m_OnClickLinkCallbacks;

        public void AddClickLinkListener(Action<string> listener)
        {
            m_OnClickLinkCallbacks += listener;
        }

        public void RemoveClickLinkListener(Action<string> listener)
        {
            m_OnClickLinkCallbacks -= listener;
        }

        public void AddClickSpriteListener(Action<string> listener)
        {
            m_OnClickSpriteCallbacks += listener;
        }

        public void RemoveClickSpriteListener(Action<string> listener)
        {
            m_OnClickSpriteCallbacks -= listener;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnClick.AddListener(OnSymbolTextNodeClick);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnClick.RemoveListener(OnSymbolTextNodeClick);
        }

        private void OnDestroy()
        {
            m_OnClickLinkCallbacks = null;
            m_OnClickSpriteCallbacks = null;
        }

        private void OnSymbolTextNodeClick(NodeBase node)
        {
            //点击到超链接
            if (node is HyperlinkNode)
            {
                var n = node as HyperlinkNode;
                m_OnClickLinkCallbacks?.Invoke(n.d_link);
            }
            //点击到图片
            else if (node is SpriteNode)
            {
                var n = node as SpriteNode;
                m_OnClickSpriteCallbacks?.Invoke(n.id);
            }
        }

    }

}

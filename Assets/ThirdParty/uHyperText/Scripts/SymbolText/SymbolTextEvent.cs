using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace WXB
{
    [RequireComponent(typeof(SymbolText))]
    public class SymbolTextEvent : MonoBehaviour, /*IPointerEnterHandler, IPointerExitHandler,*/ IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        SymbolText d_symbolText;

        //RenderCache.BaseData d_baseData;

        [System.Serializable]
        public class OnClickEvent : UnityEvent<NodeBase> {}

        [System.Serializable]
        public class OnClickAnyEvent : UnityEvent<NodeBase> { }

        // 点击了此结点
        public OnClickEvent OnClick = new OnClickEvent(); 

        // 点击到了任何东西(即使没有点击到)
        public OnClickAnyEvent OnClickAny = new OnClickAnyEvent();

        private Vector2 localPosition;

        private  BaseRenderData d_down_basedata;

        protected virtual void OnEnable()
        {
            if (d_symbolText == null)
            {
                d_symbolText = GetComponent<SymbolText>();
            }
        }

        protected virtual void OnDisable()
        {
            d_down_basedata = null;
            localPosition = Vector2.zero;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Tools.ScreenPointToWorldPointInRectangle(d_symbolText.rectTransform, eventData.position, d_symbolText.canvas.worldCamera, out localPosition))
                return;

            //localPosition *= d_symbolText.pixelsPerUnit;
            d_down_basedata = d_symbolText.renderCache.Get(localPosition);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //OnHandleClickEvent(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnHandleClickEvent(eventData);
        }

        private void OnHandleClickEvent(PointerEventData eventData)
        {
            if (!Tools.ScreenPointToWorldPointInRectangle(d_symbolText.rectTransform, eventData.position, d_symbolText.canvas.worldCamera, out localPosition))
                return;

            //localPosition *= d_symbolText.pixelsPerUnit;
            var up_node = d_symbolText.renderCache.Get(localPosition);
            if (d_down_basedata != up_node)
                return;

            if (d_down_basedata != null)
            {
                OnClick?.Invoke(d_down_basedata.node);
                OnClickAny?.Invoke(d_down_basedata.node);
            }
            else
            {
                OnClickAny?.Invoke(null);
            }
        }
    }
}

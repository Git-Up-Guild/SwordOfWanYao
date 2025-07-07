using UnityEngine;
using UnityEngine.EventSystems;
using XGame.Attr;

namespace XGame.UI
{
    public class RectTransformSizeSyncer : UIBehaviour
    {
        /// <summary>
        /// 同步方式
        /// </summary>
        public enum SyncMode
        {
            DeltaSize,
            Scale,
            Anchor,         //目标必须为子对象
        }

        [Label("同步目标"), SerializeField]
        private RectTransform m_Target;

        [Label("同步模式"), SerializeField]
        private SyncMode m_Mode = SyncMode.DeltaSize;

        [Label("百分比"), SerializeField]
        private float m_Percentage = 1f;

        private RectTransform m_My;
        private RectTransform my
        {
            get
            {
                if (m_My == null)
                    m_My = transform as RectTransform;
                return m_My;
            }
        }

        private bool m_IsDirty = false;
        public bool isDirty
        {
            get => m_IsDirty;
            set
            {
                m_IsDirty = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_My = transform as RectTransform;
        }

        protected override void Start()
        {
            base.Start();
        }

        public void SetTarget(RectTransform target)
        {
            m_Target = target;
            isDirty = true;
        }

        public void ClearTarget()
        {
            m_Target = null;
        }

        public void SetPercentage(float percentage)
        {
            m_Percentage = percentage;
            isDirty = true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            m_IsDirty = true;
            DOSync();
        }
#endif

        private void DOSync()
        {
            if (m_Target == null)
                return;

            if (isDirty)
            {
                isDirty = false;

                switch (m_Mode)
                {
                    case SyncMode.DeltaSize:
                        m_Target.localScale = Vector3.one;
                        m_Target.sizeDelta = my.sizeDelta * m_Percentage;
                        break;

                    case SyncMode.Scale:
                        {
                            var sizeTarget = m_Target.rect;
                            var sizeMy = my.rect;
                            var scaleX = (sizeMy.width * m_Percentage) / sizeTarget.width;
                            var scaleY = (sizeMy.height * m_Percentage) / sizeTarget.height;
                            m_Target.localScale = new Vector3(scaleX, scaleY, 1f);
                        }
                        break;

                    case SyncMode.Anchor:
                        {
                            m_Target.localScale = Vector3.one;

                            var sizeMy = my.rect;
                            var anchorXOffset = sizeMy.width * (1f - m_Percentage) * 0.5f;
                            var anchorYOffset = sizeMy.height * (1f - m_Percentage) * 0.5f;

                            m_Target.anchorMin = new Vector2(anchorXOffset, anchorYOffset);
                            m_Target.anchorMax = new Vector2(anchorXOffset, anchorYOffset);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        void Update()
        {
            DOSync();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            isDirty = true;
        }
    }
}
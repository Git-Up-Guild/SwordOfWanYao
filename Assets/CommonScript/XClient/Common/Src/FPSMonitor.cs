using UnityEngine;
using System.Collections.Generic;

namespace XClient.Common
{
    public class FpsMonitor : MonoBehaviour
    {
        /* ----- TODO: ----------------------------
         * Add summaries to the variables.
         * Add summaries to the functions.
         * --------------------------------------*/

        #region Variables -> Serialized Private

        [SerializeField]
        private int m_averageSamples = 150;

        #endregion

        #region Variables -> Private
        private float m_currentFps = 0f;
        private float m_avgFps = 0f;
        private float m_minFps = 0f;
        private float m_maxFps = 0f;

        private List<float> m_averageFpsSamples;
        private int m_eldestFpsSampleIndex = 0;
        private float m_accumulatedFps = 0;


        private int m_timeToResetMinMaxFps = 10;

        private float m_timeToResetMinFpsPassed = 0f;
        private float m_timeToResetMaxFpsPassed = 0f;

        private float unscaledDeltaTime = 0f;

        #endregion

        #region Properties -> Public

        public float CurrentFPS { get { return m_currentFps; } }
        public float AverageFPS { get { return m_avgFps; } }

        public float MinFPS { get { return m_minFps; } }
        public float MaxFPS { get { return m_maxFps; } }

        #endregion

        #region Methods -> Unity Callbacks

        private void Awake()
        {
            Init();
        }


        private void OnEnable()
        {
            Restore();
        }

        private void Update()
        {
            unscaledDeltaTime = Time.unscaledDeltaTime;

            m_timeToResetMinFpsPassed += unscaledDeltaTime;
            m_timeToResetMaxFpsPassed += unscaledDeltaTime;

            // Update fps and ms
            m_currentFps = 1 / unscaledDeltaTime;

            // Update avg fps
            if (m_averageFpsSamples.Count >= m_averageSamples)
            {
                m_accumulatedFps -= m_averageFpsSamples[m_eldestFpsSampleIndex];
                m_averageFpsSamples[m_eldestFpsSampleIndex] = m_currentFps;
                m_eldestFpsSampleIndex = (m_eldestFpsSampleIndex + 1) % m_averageSamples;
            }
            else
            {
                m_averageFpsSamples.Add(m_currentFps);
            }
            m_accumulatedFps += m_currentFps;
            m_avgFps = m_accumulatedFps / m_averageFpsSamples.Count;

            // Checks to reset min and max fps
            if (m_timeToResetMinMaxFps > 0
                && m_timeToResetMinFpsPassed > m_timeToResetMinMaxFps)
            {
                m_minFps = 0;
                m_timeToResetMinFpsPassed = 0;
            }

            if (m_timeToResetMinMaxFps > 0
                && m_timeToResetMaxFpsPassed > m_timeToResetMinMaxFps)
            {
                m_maxFps = 0;
                m_timeToResetMaxFpsPassed = 0;
            }

            // Update min fps

            if (m_currentFps < m_minFps || m_minFps <= 0)
            {
                m_minFps = m_currentFps;

                m_timeToResetMinFpsPassed = 0;
            }

            // Update max fps

            if (m_currentFps > m_maxFps || m_maxFps <= 0)
            {
                m_maxFps = m_currentFps;

                m_timeToResetMaxFpsPassed = 0;
            }
        }

        #endregion

        #region Methods -> Public

        public void UpdateParameters()
        {
            //m_timeToResetMinMaxFps = m_graphyManager.TimeToResetMinMaxFps;
        }

        public void SetActive(bool enable)
        {
            if (enabled != enable)
                enabled = enable;
        }

        private void Restore()
        {
            m_accumulatedFps = 0;
            m_averageFpsSamples.Clear();
            m_eldestFpsSampleIndex = 0;
            m_timeToResetMinFpsPassed = 0;
            m_timeToResetMaxFpsPassed = 0;
            m_minFps = 0;
            m_maxFps = 0;
            m_currentFps = 0;
            unscaledDeltaTime = 0;
            m_avgFps = 0;
        }

        #endregion

        #region Methods -> Private

        private void Init()
        {
            //m_graphyManager = transform.root.GetComponentInChildren<GraphyManager>();

            m_averageFpsSamples = new List<float>(m_averageSamples);

            //UpdateParameters();
        }

        #endregion
    }
}
/************************************************************************************
* Copyright (c) 2022 All Rights Reserved.
*命名空间：XGame.AssetScript.Util
*文件名： CameraProjectionSizeAdapter.cs
*创建人： 郑秀程
*创建时间：2020/7/10 18:03:43 
*描述：修正不同相机投影视口大小
*=====================================================================
*修改标记
*修改时间：
*修改人：
*描述：
************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace XGame.AssetScript.Util
{
    public class CameraOrthographicProjectionSizeAdapter : MonoBehaviour
    {
        [SerializeField]
        private Camera m_TargetCamera;

        [SerializeField]
        private Vector2Int m_ReferenceResolution;

        private float m_OrignProjectionSize;
        private Vector2Int m_ScreenSize;

        private void Awake()
        {
            if (m_TargetCamera != null)
                m_OrignProjectionSize = m_TargetCamera.orthographicSize;
            m_ScreenSize = new Vector2Int(Screen.width, Screen.height);
        }

        void Start()
        {
            Execute();
        }

        private void OnEnable()
        {
            Execute();
        }

        private void Update()
        {
            if(m_ScreenSize.x != Screen.width || m_ScreenSize.y != Screen.height)
            {
                m_ScreenSize.x = Screen.width;
                m_ScreenSize.y = Screen.height;
                Execute();
            }
        }

        public void Execute()
        {
            if (m_TargetCamera == null)
                return;

            if (!m_TargetCamera.orthographic)
                return;

            float scale = Screen.height / ((float)Screen.width / m_ReferenceResolution.x) / m_ReferenceResolution.y;
            m_TargetCamera.orthographicSize = m_OrignProjectionSize * scale;
        }
    }
}
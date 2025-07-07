using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static XClient.Network.NetObjectTransform;

namespace XClient.Network
{
    public class NetObjectTransform : NetObjectBehaviour<TransformData>
    {
        public class TransformData : MonoNetObject
        {
            public NetVarVector3 position;
            public NetVarVector3 scale;
            public NetVarVector3 rotation;

            protected override void OnSetupVars()
            {
                position = SetupVar<NetVarVector3>();
                scale = SetupVar<NetVarVector3>();
                rotation = SetupVar<NetVarVector3>();
            }
        }

        public bool enablePositionSync = true;

        public bool enableRotationSync = true;

        public bool enableScaleSync = false;

        /// <summary>
        /// 是否正在插值
        /// </summary>
        private bool m_IsPositionLerping = false;
        private float m_PositionLerpStartTick = 0f;
        private Vector3 m_SrcPosition = new Vector3();
        private Vector3 m_TargetPosition = new Vector3();
        private float m_PositonLerpTime = 0.06f;

        private Transform m_Transform;
        public new Transform transform
        {
            get
            {
                if(m_Transform == null)
                    m_Transform = GetComponent<Transform>();
                return m_Transform;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Transform = null;
        }

        protected override void OnNetObjectCreate()
        {
            NetObj.position.OnChange.AddListener(OnSyncPosition);
            NetObj.rotation.OnChange.AddListener(OnSyncRotation);
            NetObj.scale.OnChange.AddListener(OnSyncScale);
        }

        private void OnSyncScale(Vector3 o, Vector3 val)
        {
            if (enableRotationSync)
            {
                transform.localScale = val;
            }
        }
        private void OnSyncRotation(Vector3 o, Vector3 val)
        {
            if (enableRotationSync)
            {
                transform.eulerAngles = val;
            }
        }

        private void OnSyncPosition(Vector3 o, Vector3 val)
        {
            if (enablePositionSync)
            {
                m_IsPositionLerping = true;
                m_PositionLerpStartTick = Time.time;
                m_SrcPosition = transform.position;
                m_TargetPosition.Set(val.x, val.y, val.z);
                //transform.position = val;
            }
        }

        protected override void OnUpdate()
        {
            if (!NetObj.IsStarted)
                return;

            //拥有同步权限的，则主动进行数据同步
            if(NetworkManager.Instance.IsHasRight(NetObj.NetID))
            {
                //位置同步
                if(enablePositionSync)
                {
                    var curPos = transform.position;
                    if (curPos != NetObj.position.Value)
                    {
                        NetObj.position.Value = curPos;
                    }
                }

                //角度同步
                if(enableRotationSync)
                {
                    var curEuler = transform.eulerAngles;
                    if (curEuler != NetObj.rotation.Value)
                    {
                        NetObj.rotation.Value = curEuler;
                    }
                }

                //缩放同步
                if(enableScaleSync)
                {
                    var curScale = transform.localScale;
                    if (curScale != NetObj.scale.Value)
                    {
                        NetObj.scale.Value = curScale;
                    }
                }
            }

            if(m_IsPositionLerping)
            {
                var t = (Time.time - m_PositionLerpStartTick) / m_PositonLerpTime;
                transform.position = Vector3.Lerp(m_SrcPosition, m_TargetPosition, t);

                //插值完成
                if (t >= 1.0f)
                    m_IsPositionLerping = false;
            }
        }
    }
}

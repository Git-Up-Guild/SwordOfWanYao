/*******************************************************************
** 文件名:	NetEntityEffectSyncManager.cs
** 版  权:	(C) 冰川网络
** 创建人:	郑秀程
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  网络实体效果广播器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using XClient.Common;
using XClient.Network;
using XGame.Utils;

namespace XClient.Entity
{
    /// <summary>
    /// 实体效果同步管理器
    /// </summary>
    public class NetEntityEffectSyncManager : Singleton<NetEntityEffectSyncManager>
    {
        class AudioSyncData : NetCommData
        {
            public NetVarInt audioID;

            protected override void OnSetupVars()
            {
                audioID = SetupVar<NetVarInt>();
            }
        }

        /// <summary>
        /// 默认受击音效
        /// </summary>
        public int defaultHitAudioID = 3001;

        /// <summary>
        /// 默认物理攻击音效
        /// </summary>
        public int defaultPhyAttackAudioID = 3001;

        /// <summary>
        /// 默认魔法攻击音效
        /// </summary>
        public int defaultMagicAttackAudioID = 3002;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        private bool m_IsInitialized = false;

        /// <summary>
        /// 目标客户端ID
        /// </summary>
        private ulong m_TargetClientID = 0;

        public void Initialize()
        {
            if (m_IsInitialized)
                return;

            NetCommDataManager.Instance.AddHandler<AudioSyncData>(OnReceiveSync);
        }

        public void Reset()
        {
            NetCommDataManager.Instance.RemoveHandler<AudioSyncData>(OnReceiveSync);
        }

        private void OnReceiveSync(AudioSyncData data)
        {
            if (!data.IsSendToMe)
                return;

            if(data.audioID.Value > 0)
                GameGlobal.AudioCom.PlayAudio(data.audioID.Value);
        }

        /// <summary>
        /// 播放声音
        /// </summary>
        /// <param name="audioID"></param>
        public void PlayAudio(int audioID)
        {
            GameGlobal.AudioCom.PlayAudio(audioID);

            //同步到别的端
            var data = NetCommDataManager.Instance.GetSendData<AudioSyncData>();
            data.audioID.Value = audioID;
            NetCommDataManager.Instance.SendTo(m_TargetClientID, data);
        }

        /// <summary>
        /// 设置目标端ID
        /// </summary>
        public void SetTargetClientID(ulong targetID)
        {
            m_TargetClientID = targetID;
        }
    }
}

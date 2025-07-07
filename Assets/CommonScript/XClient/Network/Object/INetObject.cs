/*******************************************************************
** 文件名:	INetObject.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2024/5/21 15:35:30
** 版  本:	1.0
** 描  述:	
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;

namespace XClient.Network
{
    /// <summary>
    /// 网络对象，具有在网络间广播属性的能力
    /// </summary>
    public interface INetObject : INetable
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        bool Create();

        /// <summary>
        /// 销毁
        /// </summary>
        void Release();

        /// <summary>
        /// 设置网络ID
        /// </summary>
        /// <param name="netID"></param>
        void SetupNetID(ulong netID);

        /// <summary>
        /// 设置网络ID
        /// </summary>
        /// <param name="netID"></param>
        /// <param name="objIndex"></param>
        void SetupNetID(ulong netID, byte objIndex);

        /// <summary>
        /// 启动此对象
        /// </summary>
        void Start(bool isAddToManager = false);

        /// <summary>
        /// 停止此对象
        /// </summary>
        void Stop();

        /// <summary>
        /// 是否数据脏了
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// 设置脏标记
        /// </summary>
        void SetDirty();

        /// <summary>
        /// 清除脏标记
        /// </summary>
        void ClearDirty();

        /// <summary>
        /// 立即将修改同步到其它客户端
        /// </summary>
        void SyncImmediately();
    }
}

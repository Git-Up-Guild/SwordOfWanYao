/*******************************************************************
** 文件名:	NetVar.cs
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

using UnityEngine.Events;

namespace XClient.Network
{
    /// <summary>
    /// 网络变量类型
    /// </summary>
    public enum NetVarDataType
    {
        Bool = 1,
        Int,
        Long,
        Float,
        String,
        Vector3,
        IntArray,
        FloatArray,
    }

    /// <summary>
    /// 网络数据接口
    /// </summary>
    public interface INetVar
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 拥有者
        /// </summary>
        INetable Owner { get; }

        /// <summary>
        /// 设置拥有者
        /// </summary>
        /// <param name="owner"></param>
        void SetOwner(INetable owner);

        /// <summary>
        /// 变量类型
        /// </summary>
        NetVarDataType DataType { get; }

        /// <summary>
        /// 是否调试模式
        /// </summary>
        bool IsDebug { get; set; }

        /// <summary>
        /// 是否为脏
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// 设置藏标记
        /// </summary>
        void SetDirty();

        /// <summary>
        /// 清除脏标记
        /// </summary>
        void ClearDirty();

        /// <summary>
        /// 远端增量数值是否为脏
        /// </summary>
        bool IsRemoteValueDeltaDirty { get; }

        /// <summary>
        /// 设置远端增量数值为脏
        /// </summary>
        void SetRemoteValueDeltaDirty();

        /// <summary>
        /// 清除远端增量数值为脏
        /// </summary>
        void ClearRemoteValueDeltaDirty();

        /// <summary>
        /// 是否为公开的
        /// </summary>
        bool IsPublic { get; set; }

        /// <summary>
        /// 是否拥有权限
        /// </summary>
        bool IsHasRight { get; }

        /// <summary>
        /// 清除所有侦听函数
        /// </summary>
        void RemoveAllChangeListeners();

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="serializer"></param>
        void Read(INetVarSerializer serializer);

        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="serializer"></param>
        void Write(INetVarSerializer serializer);

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="serializer"></param>
        void ReadRemoteValueDelta(INetVarSerializer serializer);

        /// <summary>
        /// 输出增量值
        /// </summary>
        /// <param name="serializer"></param>
        void WriteRemoteValueDelta(INetVarSerializer serializer);

        /// <summary>
        /// 清除增量值
        /// </summary>
        void ClearRemoteValueDelta();

        /// <summary>
        /// 清理值
        /// </summary>
        void Clear();

        /// <summary>
        /// 拷贝值
        /// </summary>
        /// <param name="netVar"></param>
        void Set(INetVar netVar);

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="varValue"></param>
       void Write(NetVarValue varValue);

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="varValue"></param>
        void Read(NetVarValue varValue);

    }
}

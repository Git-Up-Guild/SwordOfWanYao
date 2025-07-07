/*******************************************************************
** 文件名:	NetID.cs
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

using System;

namespace XClient.Network
{
    /// <summary>
    /// 网络ID是一个64位的整型数据，由3部分组成：
    /// ClientID：代表了客户端的唯一标识，由服务器统一分配（高32位）
    /// SerialNo： 由客户端来分配，生成的流水号 （中24位）
    /// Index：索引，一个流水号最多可以生成255个索引 （低8位）
    /// </summary>
    public class NetID
    {
        /// <summary>
        /// 临时对象，用来进行一些转换
        /// </summary>
        public static NetID Temp = new NetID();

        /// <summary>
        /// 客户端ID（高32位，服务器生成）
        /// </summary>
        public uint ClientID { get; private set; }

        /// <summary>
        /// 流水号（低24位，客户端生成）
        /// </summary>
        public uint SerialNo { get; private set; }

        /// <summary>
        /// 生成索引（低8位，客户端生成）
        /// </summary>
        public byte Index { get; private set; }

        /// <summary>
        /// 组合后的唯一ID
        /// </summary>
        public ulong ID { get; private set; }

        public NetID() { }

        /// <summary>
        /// 网络ID
        /// </summary>
        /// <param name="id">主ID（高32位）</param>
        /// <param name="no">流水号（存放在低24位）</param>
        /// <param name="index">生成索引（存放在低8位）</param>
        public NetID(uint id, uint no, byte index)
        {
            Set(id, no, index);
        }

        /// <summary>
        /// 网络ID
        /// </summary>
        /// <param name="id"></param>
        public NetID(ulong id)
        {
            Set(id);
        }

        /// <summary>
        /// 网络ID
        /// </summary>
        /// <param name="id">主ID（高32位）</param>
        /// <param name="no">流水号（存放在低24位）</param>
        /// <param name="index">生成索引（存放在低8位）</param>
        public void Set(uint id, uint no, byte index)
        {
            ClientID = id;
            SerialNo = no;
            Index = index;

            ID = ((ulong)ClientID << 32) | (SerialNo << 8) | Index;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="no"></param>
        public void Set(NetID o)
        {
            ClientID = o.ClientID;
            SerialNo = o.SerialNo;
            Index = o.Index;
            ID = o.ID;
        }

        /// <summary>
        /// 设置唯一ID
        /// </summary>
        /// <param name="id"></param>
        public NetID Set(ulong id)
        {
            ClientID = (uint)(id >> 32);

            //取不到高位，说明是不是要给有效的网络ID，直接当作主ID来使用
            if (ClientID == 0)
            {
                ClientID = (uint)id;
                SerialNo = 0;
                Index = 0;
            }
            else
            {
                SerialNo = (uint)((id & 0x00000000ffffff00)>>8);
                Index = (byte)(id & 0x00000000000000ff);
            }

            ID = ((ulong)ClientID << 32) | (SerialNo << 8) | Index;

            return this;
        }

        /// <summary>
        /// 是否为拥有者
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool IsLocalClient => NetworkManager.Instance.IsLocalClient(this);

        /// <summary>
        /// 是否拥有权限进行修改
        /// </summary>
        public bool IsHasRight => NetworkManager.Instance.IsHasRight(this);

        /// <summary>
        /// 是否为主机
        /// </summary>
        /// <returns></returns>
        public bool IsHost()
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            if(obj is NetID)
            {
                var o = (obj as NetID);
                return o.ID == ID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientID, SerialNo, Index);
        }

        public static bool operator ==(NetID a, NetID b) => a.ID == b.ID;

        public static bool operator !=(NetID a, NetID b) => a.ID != b.ID;

        public override string ToString()
        {
            return $"ID: {ID}, Detail: {ClientID}-{SerialNo}-{Index}";
        }

    }
}

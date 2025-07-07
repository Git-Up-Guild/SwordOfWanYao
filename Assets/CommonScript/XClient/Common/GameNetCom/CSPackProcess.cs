using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.NetCom;

using ORM;
using gamepol;

namespace XClient.Common
{
    public class CSPackProcess : IGamePackProcess
    {
        // 游戏消息TCSMessage（收包）
        private gamepol.TCSMessage m_recvGameMsg = new gamepol.TCSMessage();

        // 网关消息TCGMessage（收包）
        private cgpol.TCGMessage m_recvGatewayMsg = new cgpol.TCGMessage();

        private gamepol.TMsgHead m_gameHead = new gamepol.TMsgHead();
        private cgpol.TMsgHead m_gatewayHead = new cgpol.TMsgHead();
        

        private CORM_packaux m_recvGameOrmPacker = new CORM_packaux();

        //用来解析网关消息
        private CORM_packaux m_recvGatewayOrmPacker = new CORM_packaux();

        private uint nRecvMsgID;//消息id(收包)
        private int m_srcEndPoint;

        // 游戏消息TCSMessage（发包）
        private gamepol.TCSMessage m_sendGameMsg = new gamepol.TCSMessage();

        // 网关消息TCGMessage（发包）
        private cgpol.TCGMessage m_sendGatewayMsg = new cgpol.TCGMessage();

        private CORM_packaux m_sendGameOrmPacker = new CORM_packaux();
        private CORM_packaux m_sendGatewayOrmPacker = new CORM_packaux();

        //用来解析网关消息头
        private CORM_packaux m_ormPacker = new CORM_packaux();

        // 发包buffer
        private byte[] m_sendBuffer = new byte[cgpol.TCGMessage.MAX_PACKEDSIZE];

        //发包对象
        private SendMessage m_sendMessage = new SendMessage();

        //上层合并对象
        private PackDataMerge m_packDataMerge = null;

        // 获取游戏协议对象
        public gamepol.TCSMessage GetGameMsg(bool bSendFlag)
        {
            return bSendFlag ? m_sendGameMsg : m_recvGameMsg;
        }

        // 获取网关协议对象
        public cgpol.TCGMessage GetGatewayMsg(bool bSendFlag)
        {
            return bSendFlag ? m_sendGatewayMsg : m_recvGatewayMsg;
        }

        public ISendMessage GetSendMessage()
        {
            return m_sendMessage;
        }

        public ISendMessage Pack(byte srcEndPoint, byte dstEndPoint, uint nMsgID)
        {
            //非网关消息，需要构建一条网关中转消息
            if (dstEndPoint != NetDefine.ENDPOINT_GATEWAY)
            {
                //构建中转消息实例
                m_sendGatewayMsg.Init(cgpol.TCGMessage.MSG_GATEWAY_SEND_DATA_C2G_REQ);
                cgpol.TMSG_GATEWAY_SEND_DATA_C2G_REQ sendDataReq = m_sendGatewayMsg.stTMSG_GATEWAY_SEND_DATA_C2G_REQ;
                sendDataReq.set_iDstType(dstEndPoint);

                //初始化打包器
                m_sendGameOrmPacker.Init(sendDataReq.set_arrBuffer(), 
                    cgpol.TMSG_GATEWAY_SEND_DATA_C2G_REQ.countof_arrBuffer,true, false);

                //将要发送的消息打包
                m_sendGameMsg.Pack(m_sendGameOrmPacker);

                //游戏消息作为中转消息的内容
                sendDataReq.set_dwBufferLen((uint)m_sendGameOrmPacker.GetDataOffset());
            }

            m_sendGatewayOrmPacker.Init(m_sendBuffer, cgpol.TCGMessage.MAX_PACKEDSIZE, true, false);
            m_sendGatewayMsg.Pack(m_sendGatewayOrmPacker);

            m_sendMessage.SetData(srcEndPoint, dstEndPoint, nMsgID, m_sendBuffer, 
                m_sendGatewayOrmPacker.GetDataOffset());
            return m_sendMessage;
        }


        public bool UnPackTest(byte[] data, int nLen, int srcEndPoint)
        {
            m_recvGameOrmPacker.Init(data, nLen, false, true);
            m_recvGameMsg.Unpack(m_recvGameOrmPacker);
            srcEndPoint = NetDefine.ENDPOINT_NORMAL;
            nRecvMsgID = m_recvGameMsg.stHead.get_iMsgID();
            this.m_srcEndPoint = srcEndPoint;
            return true;
        }

        public UNPACK_STATE UnPack(byte[] data, int nLen, int srcEndPoint)
        {
            //初始化解包器
            m_ormPacker.Init(data, nLen);

            //尝试解析成网关消息头
            m_gatewayHead.Unpack(m_ormPacker);

            //判断是否为有效的网关消息
            bool isCGMessage = cgpol.TCGMessage.ExistsMsg((int)m_gatewayHead.get_iMsgID());

            UNPACK_STATE unpackStage = UNPACK_STATE.UNPACK_STATE_FAILED;
            //bool canHandle = (m_gatewayHead.get_iMsgID() == cgpol.TCGMessage.MSG_GATEWAY_RECV_DATA_G2C_NTF);
            if (isCGMessage)//&& canHandle)
            {
                unpackStage = UNPACK_STATE.UNPACK_STATE_SUC;
                try
                {
                    //将网关消息解析出来
                    m_recvGatewayOrmPacker.Init(data, nLen,false,true);
                    m_recvGatewayMsg.Unpack(m_recvGatewayOrmPacker);
                    nRecvMsgID = m_recvGatewayMsg.stHead.get_iMsgID();

                    //网关中转消息
                   if (nRecvMsgID == cgpol.TCGMessage.MSG_GATEWAY_RECV_DATA_G2C_NTF)
                    {
                        cgpol.TMSG_GATEWAY_RECV_DATA_G2C_NTF recvNtf = m_recvGatewayMsg.stTMSG_GATEWAY_RECV_DATA_G2C_NTF;

                        byte[] arrPackedData = recvNtf.get_arrBuffer();
                        int iDataSize = (int)recvNtf.get_dwBufferLen();
                        sbyte start = recvNtf.get_bStart();
                        sbyte end = recvNtf.get_bEnd();

                        //第一个包验证是否C#解包
                        if (start > 0)
                        {
                            //游戏消息解析器
                            m_recvGameOrmPacker.Init(arrPackedData, iDataSize, false, true);

                            //先把消息头解析出来
                            m_gameHead.Unpack(m_recvGameOrmPacker);

                            //看是否为C#游戏消息
                            var msgID = m_gameHead.get_iMsgID();
                            if (!gamepol.TCSMessage.ExistsMsg((int)msgID))
                            {
                                Debug.LogError($"网络消息不存在：MsgID={msgID}");
                                return UNPACK_STATE.UNPACK_STATE_FAILED;
                            }
                        }

                        //需要合包的处理(start 为1, end 为 1的 不需要合包,直接跳过)
                        if (start < 1 || end < 1)
                        {
                            if (null == m_packDataMerge)
                            {
                                m_packDataMerge = new PackDataMerge();
                            }

                            //第一个包先清除(start = 1,end = 0)
                            if (start > 0)
                            {
                                m_packDataMerge.StartMerging();
                            }
                            else if (m_packDataMerge.IsMerging() == false)
                            {
                                //当前没有在合并的对象，应该是另外对象处理
                                return UNPACK_STATE.UNPACK_STATE_FAILED;
                            }

                            m_packDataMerge.MergeData(arrPackedData, iDataSize);
                            //start=0，end = 1
                            if (end > 0)
                            {
                                m_packDataMerge.EndMerging();
                                //获取解包数据
                                arrPackedData = m_packDataMerge.GetData();
                                iDataSize = m_packDataMerge.GetDataLen();

                            }
                            else
                            {
                                //start = 0，end = 0
                                //还没有解包完成,数据已经拷贝了，告诉外部处理完成
                                return UNPACK_STATE.UNPACK_STATE_WAIT;
                            }
                        }

                        //解析成游戏消息
                        m_recvGameOrmPacker.Init(arrPackedData, iDataSize, false, true);
                        m_recvGameMsg.Unpack(m_recvGameOrmPacker);

                        srcEndPoint = NetDefine.ENDPOINT_NORMAL;
                        nRecvMsgID = m_recvGameMsg.stHead.get_iMsgID();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("C#解包失败：nMsgID=,error=" +
                        e.ToString());
                }
                //m_ormPacker.Init(data, nLen);

            }
            this.m_srcEndPoint = srcEndPoint;
            return unpackStage;
        }

        public int GetSrcEndPoint()
        {
            return m_srcEndPoint;
        }
        public uint GetMsgID()
        {
            return nRecvMsgID;
        }

    }
}
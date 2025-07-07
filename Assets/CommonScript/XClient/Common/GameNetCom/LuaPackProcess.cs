using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.NetCom;

using ORM;
using System;
using System.IO;

namespace XClient.Common
{
    public class LuaPackProcess : IGamePackProcess
    {
        //lua消息的打包对象，这个对象是唯一的，需要在lua处理那边传入
        private COrmCSharpSerializer m_cormSerializer;

        private cgpol.TMsgHead m_msgHead = new cgpol.TMsgHead();

        //ORM消息解包器
        private CORM_packaux m_ormPacker = new CORM_packaux();

        //游戏消息
        private SendMessage m_sendMessage = new SendMessage();

        // 发包buffer
        private byte[] m_sendBuffer = new byte[cgpol.TCGMessage.MAX_PACKEDSIZE];

        // 网关消息
        private cgpol.TCGMessage m_sendGatewayMsg = new cgpol.TCGMessage();
        private CORM_packaux m_sendGatewayOrmPacker = new CORM_packaux();

        private int m_srcEndPoint;

        //上层合并对象
        private PackDataMerge m_packDataMerge = null;

        /// <summary>
        /// 传入lua消息的打包器
        /// </summary>
        /// <param name="cormSerializer"></param>
        public void SetOrmBridge(COrmCSharpSerializer cormSerializer)
        {
            this.m_cormSerializer = cormSerializer;
        }

        public COrmCSharpSerializer GetCOrmCSharpSerializer()
        {
            return m_cormSerializer;
        }

        public ISendMessage Pack(byte srcEndPoint, byte dstEndPoint, uint nMsgID)
        {
            //在打包之前，在lua中已经对消息做好了设置
            byte[] buffer = m_cormSerializer.GetWriteBufferPtr();
            int bufferLen = m_cormSerializer.GetWriteStreamPos();

            //网关消息，直接发送
            if (dstEndPoint == NetDefine.ENDPOINT_GATEWAY)
            {
                m_sendMessage.SetData(srcEndPoint, dstEndPoint, nMsgID, buffer, bufferLen);
            }
            //非网关消息，需要构建一条网关中转消息
            else
            {
                //构建中转消息实例
                m_sendGatewayMsg.Init(cgpol.TCGMessage.MSG_GATEWAY_SEND_DATA_C2G_REQ);
                cgpol.TMSG_GATEWAY_SEND_DATA_C2G_REQ sendDataReq = m_sendGatewayMsg.stTMSG_GATEWAY_SEND_DATA_C2G_REQ;
                sendDataReq.set_iDstType(dstEndPoint);

                //将消息打包，拷贝到中转协议中
                var bufferC2G = sendDataReq.set_arrBuffer();

                Array.Copy(buffer, bufferC2G, bufferLen);
               // buffer.CopyTo(bufferC2G, 0);
                sendDataReq.set_dwBufferLen((uint)bufferLen);
            }

            m_sendGatewayOrmPacker.Init(m_sendBuffer, cgpol.TCGMessage.MAX_PACKEDSIZE, true, false);
            m_sendGatewayMsg.Pack(m_sendGatewayOrmPacker);

            m_sendMessage.SetData(srcEndPoint, dstEndPoint, nMsgID, m_sendBuffer,
                m_sendGatewayOrmPacker.GetDataOffset());
          
            return m_sendMessage;
        }
        
        private CORM_packaux m_recvGatewayOrmPacker = new CORM_packaux();
        // 网关消息TCGMessage（收包）
        private cgpol.TCGMessage m_recvGatewayMsg = new cgpol.TCGMessage();
        private CORM_packaux m_recvGameOrmPacker = new CORM_packaux();
        private cgpol.TMsgHead m_gatewayHead = new cgpol.TMsgHead();
        private gamepol.TMsgHead m_gameHead = new gamepol.TMsgHead();


        public UNPACK_STATE UnPack(byte[] data, int nLen, int srcEndPoint)
        {
            //初始化解包器
            m_ormPacker.Init(data, nLen);

            //先将消息头解出来
            m_gatewayHead.Unpack(m_ormPacker);

            //解出消息ID
            bool isCGMessage = cgpol.TCGMessage.ExistsMsg((int)m_gatewayHead.get_iMsgID());
            bool canHandle = (m_gatewayHead.get_iMsgID()== cgpol.TCGMessage.MSG_GATEWAY_RECV_DATA_G2C_NTF);

            UNPACK_STATE unpackStage = UNPACK_STATE.UNPACK_STATE_FAILED;

            if (isCGMessage&& canHandle)
            {
                unpackStage = UNPACK_STATE.UNPACK_STATE_SUC;
                try
                {
                    //网关协议解包
                    m_recvGatewayOrmPacker.Init(data, nLen, false, true);
                    m_recvGatewayMsg.Unpack(m_recvGatewayOrmPacker);
                    
                    //只处理中转消息
                    //if (m_recvGatewayMsg.stHead.get_iMsgID() == cgpol.TCGMessage.MSG_GATEWAY_RECV_DATA_G2C_NTF)
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
                            if (gamepol.TCSMessage.ExistsMsg((int)m_gameHead.get_iMsgID()))
                            {
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



                        //将消息内容导入到解包器中，这样就可以在lua中访问消息的内容了
                        m_cormSerializer.ImportStream(arrPackedData, iDataSize);
                        srcEndPoint = NetDefine.ENDPOINT_NORMAL;
                        return UNPACK_STATE.UNPACK_STATE_SUC;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("C#解包失败：nMsgID=,error=" +
                        e.ToString());
                }
                //m_ormPacker.Init(data, nLen);

            }

            this.m_srcEndPoint =  NetDefine.ENDPOINT_NORMAL;
           
            return unpackStage;
        }

        /// <summary>
        /// 因为是lua解包，所以当前包是没有解完的
        /// </summary>
        /// <returns></returns>
        public uint GetMsgID()
        {
            return 0;
        }

        public int GetSrcEndPoint()
        {
            return m_srcEndPoint;
        }

        //测试接口，手动解包没有包头的包
        public bool UnPackTest(byte[] data, int nLen, int srcEndPoint)
        {
            return true;
        }
    }
}
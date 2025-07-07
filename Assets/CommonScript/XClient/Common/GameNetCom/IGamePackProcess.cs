using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XGame.NetCom;


namespace XClient.Common
{
    //添加一个游戏解包接口（到时候一个发送包，接收包，只会有一个，所以解包，发包的数据，全部保存在这里面）
    public interface IGamePackProcess : IPackProcess
    {
        uint GetMsgID();
        int GetSrcEndPoint();
        ISendMessage Pack(byte srcEndPoint, byte dstEndPoint, uint nMsgID);//打包
    }

    public class SendMessage : ISendMessage
    {
        byte[] buffer;
        int bufferLen;
        byte dstEndPoint;
        byte srcEndPoint;
        uint nMsgID;


        public void SetData(byte srcEndPoint, byte dstEndPoint, uint nMsgID, byte[] buffer, int bufferLen)
        {
            this.srcEndPoint = srcEndPoint;
            this.dstEndPoint = dstEndPoint;
            this.nMsgID = nMsgID;
            this.buffer = buffer;
            this.bufferLen = bufferLen;
        }

        public byte[] GetBuffer()
        {
            return buffer;
        }

        public int GetBufferLen()
        {
            return bufferLen;
        }

        public byte GetDstEndPoint()
        {
            return dstEndPoint;
        }

        public uint GetMsgID()
        {
            return nMsgID;
        }

        public byte GetSrcEndPoint()
        {
            return srcEndPoint;
        }
    }
}

/*******************************************************************
** 文件名:	ByteDataStream.cs
** 版  权:	(C) 幻引力网络科技有限公司
** 创建人:	许德纪
** 日  期:	2021.03.27
** 版  本:	1.0
** 描  述:	
** 应  用: 操控内存对象

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.IO;
using System.Text;

namespace Spine
{


    public class BinaryStream : Stream
    {
        //数据buff
        private byte[] m_Data = null;

        //数据长度
        private int m_nDataLen = 0;

        //当前读写位置
        private int m_nCurPos = 0;

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanWrite { get { return false; } }

        public override long Length { get { return (long)m_nDataLen; } }

        public override long Position { get { return m_nCurPos; } set { m_nCurPos = (int)value; } }

        public void Init(byte[] data, int len)
        {
            m_Data = data;
            m_nDataLen = len;
            m_nCurPos = 0;
        }

        public void Seek(int pos)
        {
            m_nCurPos = pos;
        }

     
        public void Write(byte[] buffer)
        {
            Write(buffer, buffer.Length);

        }

        public void Write(byte[] buffer,int size)
        {
            Array.Copy(buffer, 0, m_Data, m_nCurPos, size);
            m_nCurPos += size;

        }

        public  void Write(sbyte value)
        {
            m_Data[m_nCurPos++] = (byte)value;
        }
      
        public  void Write(byte value)
        {
            m_Data[m_nCurPos++] = value;
        }

        public override int ReadByte()
        {
            return m_Data[m_nCurPos++];
        }

        /*
        public   byte ReadByte()
        {
            return m_Data[m_nCurPos++];
        }
        */

        public sbyte ReadSByte()
        {
            return (sbyte)m_Data[m_nCurPos++];
        }

        public  byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            Array.Copy(m_Data, m_nCurPos, data,0,count);
            m_nCurPos += count;
            return data;
        }

        public string GetString(int nLen)
        {
            /*
            if(m_nCurPos+nLen>m_nDataLen)
            {
                UnityEngine.Debug.LogError("读取越界: m_nDataLen=" + m_nDataLen + ",nLen = " + (m_nCurPos+nLen));
                return "";
            }
            */

            int nPos = m_nCurPos;
            m_nCurPos += nLen;
            return Encoding.UTF8.GetString(m_Data, nPos, nLen);
        }

        public int GetPos()
        {
            return m_nCurPos;
        }

        public byte[] GetData()
        {
            return m_Data;
        }

        public int GetDataSize()
        {
            return m_nDataLen;
        }

        public int GetFreeSpaceSize()
        {
            return m_nDataLen - m_nCurPos;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Array.Copy(m_Data, m_nCurPos, buffer, offset, count);
            m_nCurPos += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    m_nCurPos = (int)offset;
                    break;
                case SeekOrigin.Current:
                    m_nCurPos += (int)offset;
                    break;
                case SeekOrigin.End:
                    m_nCurPos = m_nDataLen-(int)offset;
                    break;
            }

            return m_nCurPos;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

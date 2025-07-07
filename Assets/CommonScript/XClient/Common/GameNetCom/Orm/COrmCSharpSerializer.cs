using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ORM
{
    public class COrmCSharpSerializer
    {
        private CORM_packaux m_oReader = null;
        private CORM_packaux m_oWriter = null;
        private int m_nWriteBufferSize = 0;
        private int m_nImportBufferSize = 0;

        public int WriteFieldHead(int iFID, ORM.EN_WIRETYPE enWT)
        {
            uint uHead = CPackMisc.FHEAD_MAKE((uint)iFID, (uint)enWT);
            WriteVarintUINT32(uHead);
            return 0;
        }

        public int WriteUINT8(byte val)
        {
            m_oWriter.WriteUINT8(val);
            return 0;
        }

        public int WriteVarintUINT16(ushort val)
        {
            m_oWriter.WriteVarintUINT16(val);
            return 0;
        }

        public int WriteVarintUINT32(uint val)
        {
            m_oWriter.WriteVarintUINT32(val);
            return 0;
        }

        public int WriteVarintUINT64(ulong val)
        {
            m_oWriter.WriteVarintUINT64(val);
            return 0;
        }

        public int WriteINT8(sbyte val)
        {
            m_oWriter.WriteINT8(val);
            return 0;
        }

        public int WriteVarintINT16(short val)
        {
            m_oWriter.WriteVarintINT16(val);
            return 0;
        }

        public int WriteVarintINT32(int val)
        {
            m_oWriter.WriteVarintINT32(val);
            return 0;
        }

        public int WriteVarintINT64(long val)
        {
            m_oWriter.WriteVarintINT64(val);
            return 0;
        }

        public int WriteFLOAT(float val)
        {
            m_oWriter.WriteFLOAT(val);
            return 0;
        }

        public int WriteDOUBLE(double val)
        {
            m_oWriter.WriteDOUBLE(val);
            return 0;
        }

        public int WriteSTRING(string strContent)
        {
            m_oWriter.WriteSTRING(strContent);
            return 0;
        }

        //  固定数据暂时只有32位
        public int WriteFixedUINT32(uint val)
        {
            m_oWriter.WriteFixedUINT32(val);
            return 0;
        }

        public int WriteFixedINT32(int val)
        {
            m_oWriter.WriteFixedUINT32((uint)val);
            return 0;
        }

        public int GetWriteStreamPos()
        {
            return m_oWriter.GetDataOffset();
        }

        public int SetWriteStreamPos(int iPos)
        {
            m_oWriter.SetDataOffset(iPos);
            return 0;
        }

        public void ReserveWriteBufferSize(int iNewSize)
        {
            if(m_nWriteBufferSize != iNewSize)
            {
                m_nWriteBufferSize = iNewSize;
                m_oWriter = new CORM_packaux(iNewSize);
            }
        }

        public void ResetWriteBuffer()
        {
            m_oWriter.SetDataOffset(0);
        }

        public byte[] GetWriteBufferPtr()
        {
            return m_oWriter.GetData();
        }

        public int GetWriteBufferSize()
        {
            return m_nWriteBufferSize;
        }

        public void ImportStream(byte[] arrBuffer, int nBufferSize)
        {
            m_oReader.StorageData(arrBuffer, nBufferSize);
            m_oReader.Seek(0);
        }

        public void ReserveImportBufferSize(int iNewSize)
        {
            if(m_nImportBufferSize != iNewSize)
            {
                m_nImportBufferSize = iNewSize;
                m_oReader = new CORM_packaux(iNewSize);
            }
        }

        public int ReadFieldHead(out uint uHead, out uint uWireType)
        {
            uint u32Head = m_oReader.ReadVarintUINT32();
            uHead = CPackMisc.FHEAD_GET_FID(u32Head);
            uWireType = CPackMisc.FHEAD_GET_WIRETYPE(u32Head);
            return 0;
        }

        public int ReadUINT8(out byte val)
        {
            val = m_oReader.ReadUINT8();
            return 0;
        }

        public int ReadVarintUINT16(out ushort val)
        {
            val = m_oReader.ReadVarintUINT16();
            return 0;
        }

        public int ReadVarintUINT32(out uint val)
        {
            val = m_oReader.ReadVarintUINT32();
            return 0;
        }

        public int ReadVarintUINT64(out ulong val)
        {
            val = m_oReader.ReadVarintUINT64();
            return 0;
        }

        public int ReadINT8(out sbyte val)
        {
            val = m_oReader.ReadINT8();
            return 0;
        }

        public int ReadVarintINT16(out short val)
        {
            val = m_oReader.ReadVarintINT16();
            return 0;
        }

        public int ReadVarintINT32(out int val)
        {
            val = m_oReader.ReadVarintINT32();
            return 0;
        }

        public int ReadVarintINT64(out long val)
        {
            val = m_oReader.ReadVarintINT64();
            return 0;
        }

        public int ReadFLOAT(out float val)
        {
            val = m_oReader.ReadFLOAT();
            return 0;
        }

        public int ReadDOUBLE(out double val)
        {
            val = m_oReader.ReadDOUBLE();
            return 0;
        }

        public int ReadSTRING(out string val)
        {
            val = m_oReader.ReadSTRING();
            return 0;
        }

        public int ReadFixedINT32(out int val)
        {
            val = (int)m_oReader.ReadFixedUINT32();
            return 0;
        }

        public int ReadFixedUINT32(out uint val)
        {
            val = m_oReader.ReadFixedUINT32();
            return 0;
        }

        public int SkipField(uint uHead)
        {
            return m_oReader.SkipField(uHead) ? 0 : 1;
        }

        public int GetReadStreamPos()
        {
            return m_oReader.GetDataOffset();
        }

        public int SetReadStreamPos(int iPos)
        {
            m_oReader.SetDataOffset(iPos);
            return 0;
        }

        public void ResetImportBuffer()
        {
            m_oReader.SetDataOffset(0);
        }
    }
}

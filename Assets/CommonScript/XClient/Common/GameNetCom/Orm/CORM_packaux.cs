using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;
using XGame.Memory;


namespace ORM
{
    //  传输类型
    public enum EN_WIRETYPE
    {
        enWT_VARINT,      // 跟后着面的数据是以变体保存
        enWT_BINDATA,     // 跟在后面的数据是bin数据, 是一个UINT32(fixed) + bin_data   
        enWT_FIXED8,      // 后面就一个 int8 或 uint8
        enWT_STRING,      // 后面是一个 varu32 + string数据
    };

    //  数据类型ID定义
    public enum EN_DATATYPE_ID
    {
        en_DATATYPE_MIN = 0,
        en_DATATYPE_INT8 = en_DATATYPE_MIN,
        en_DATATYPE_INT16,
        en_DATATYPE_INT32,
        en_DATATYPE_INT64,
        en_DATATYPE_UINT8,
        en_DATATYPE_UINT16,
        en_DATATYPE_UINT32,
        en_DATATYPE_UINT64,
        en_DATATYPE_FLOAT,
        en_DATATYPE_DOUBLE,
        en_DATATYPE_DATA,
        en_DATATYPE_STRING,
        en_DATATYPE_MAX,
    };

    public class CPackDataException : Exception
    {
        public enum EN_EXCEPTION
        {
            SUCCESS = 0,           // 成功, 不会使用这个值. 
            BUFFER_REACH_END,        //  到达缓冲区末尾 (缓冲区写满 或 缓冲区读空)
            FLOAT_SIZE_ERROR,   //浮点数大小错误
            DST_BUFF_TOO_SMALL,    //目标缓冲区太小
            REFER_GREATER_THAN_COUNT,   //refer字段的值超过count的限制
            UNDEFINE_MSGID,               //无效的消息ID, 消息打包时, 如果传入无效消息码, 会返回这个报错
            LUA_MISS_FIELD,               // 缺少LUA字段
            LUA_FIELD_TYPE_DISMATCH,      // LUA字段类型不匹配
            VARIENT_DECODE_ERROR,          //  变体数据编码错误
            BYTES_STREAM_ERROR,             //  字节流读取错误
            UNKNOWN_WIRETYPE,                //  wiretype类型错误
            UNEXPECTED_ERROR,            //  未预期的情况, 
        };

        private EN_EXCEPTION m_enException;
        private string m_strMessage;

        public CPackDataException(EN_EXCEPTION enException, string strMsg = "")
        {
            m_enException = enException;
            m_strMessage = strMsg;
        }

        public int GetExceptionID()
        {
            return (int)m_enException;
        }

        public string GetExceptionMessage()
        {
            if (m_strMessage.Length > 0)
            {
                return m_strMessage;
            }
            return GetError((int)m_enException);
        }

        public static string GetError(int iExceptionID)
        {
            EN_EXCEPTION enExceptionID = (EN_EXCEPTION)iExceptionID;
            string strRet;
            switch (enExceptionID)
            {
                case EN_EXCEPTION.BUFFER_REACH_END:
                    {
                        strRet = "已到达缓冲区末尾";
                    }
                    break;
                case EN_EXCEPTION.FLOAT_SIZE_ERROR:
                    {
                        strRet = "浮点数大小错误";
                    }
                    break;
                case EN_EXCEPTION.DST_BUFF_TOO_SMALL:
                    {
                        strRet = "目标缓冲区太小";
                    }
                    break;
                case EN_EXCEPTION.REFER_GREATER_THAN_COUNT:
                    {
                        strRet = "refer字段的值大于count指定的值";
                    }
                    break;
                case EN_EXCEPTION.UNDEFINE_MSGID:
                    {
                        strRet = "消息码未定义";
                    }
                    break;
                case EN_EXCEPTION.LUA_MISS_FIELD:
                    {
                        strRet = "缺少LUA字段";
                    }
                    break;
                case EN_EXCEPTION.LUA_FIELD_TYPE_DISMATCH:
                    {
                        strRet = "字段类型不匹配";
                    }
                    break;
                case EN_EXCEPTION.VARIENT_DECODE_ERROR:
                    {
                        strRet = "变体数据编码错误";
                    }
                    break;
                case EN_EXCEPTION.BYTES_STREAM_ERROR:
                    {
                        strRet = "字节流读取错误";
                    }
                    break;
                case EN_EXCEPTION.UNKNOWN_WIRETYPE:
                    {
                        strRet = "wiretype类型错误";
                    }
                    break;

                default:
                    {
                        strRet = "未预期错误, 请联系开发人员";
                    }
                    break;
            }

            return strRet;
        }

    };


    public class CORM_packaux
    {
        //  每字节最大保存位数
        private const int MAX_BYTE_STORE_BITS = 7;

        // 最大的7位二进制
        private const int MAX_7_BITS = 127;

        ByteDataStream m_dataStream = new ByteDataStream();


        List<string> m_arrPackingPath = new List<string>();  //  正在打包的字段路径

        /*
        MemoryStream m_oMemStream;
        BinaryWriter m_oBinWriter;
        BinaryReader m_oBinReader;


        //解包的缓冲区
        byte[] m_arrPackedData = null;
        //缓冲区大小
        int m_iDataSize = 0;//
        */

        public CORM_packaux()
        {
        }

        //   打包用构造函数
        public CORM_packaux(int iPackingCapacity)
        {

            SetCapacity(iPackingCapacity);
        }

        public CORM_packaux(byte[] arrPackedData, int iDataSize)
        {
            Init(arrPackedData, iDataSize);
        }


        //初始化ORM
        public void Init(byte[] arrPackedData, int iDataSize,
            bool careteBinaryWriter = true, bool createBinaryReader = true)
        {


            m_dataStream.Init(arrPackedData, iDataSize);
            m_dataStream.Seek(0);
            /*
            if (Object.Equals(arrPackedData, m_arrPackedData)==false|| iDataSize!= m_iDataSize)
            {
                m_oMemStream = new MemoryStream(arrPackedData, 0, iDataSize);
                if (careteBinaryWriter)
                {
                    m_oBinWriter = new BinaryWriter(m_oMemStream);
                }

                if (createBinaryReader)
                {
                    m_oBinReader = new BinaryReader(m_oMemStream);
                }
               
                
                m_arrPackedData = arrPackedData;
                m_iDataSize = iDataSize;
            }

            m_oMemStream.Seek(0, SeekOrigin.Begin);
            */

        }

        public void Seek(int pos)
        {
            m_dataStream.Seek(pos);
        }

        //存储数据
        public void StorageData(byte[] arrPackedData, int iDataSize)
        {
            /*
            if(m_iDataSize< iDataSize)
            {
                SetCapacity(iDataSize);
            }

            m_oMemStream.Seek(0, SeekOrigin.Begin);
            //拷贝数据
            Array.Copy(arrPackedData, 0, m_arrPackedData, 0, iDataSize);
            */

            m_dataStream.Seek(0);
            m_dataStream.Write(arrPackedData, iDataSize);
        }



        public void PushFieldName(string strFmt, params Object[] arrParams)
        {
            string strTxt = string.Format(strFmt, arrParams);
            m_arrPackingPath.Add(strTxt);
        }

        public void PopFieldName()
        {
            int iNums = m_arrPackingPath.Count;
            if (iNums > 0)
            {
                m_arrPackingPath.RemoveAt(iNums - 1);
            }
        }

        public string GetFieldPath()
        {
            string strRet = "";
            foreach (string strField in m_arrPackingPath)
            {
                strRet += string.Format(".{0}", strField);
            }

            return strRet;
        }

        public int GetDataOffset()
        {
            //return (int)m_oMemStream.Position;
            return m_dataStream.GetPos();
        }

        public byte[] GetData()
        {
            //return m_oMemStream.GetBuffer();
            return m_dataStream.GetData();
        }

        private void __ThrowException(CPackDataException.EN_EXCEPTION enExceptionID)
        {
            throw new CPackDataException(enExceptionID);
        }

        //扩容空间
        public void SetCapacity(int iPackingCapacity)
        {
            /*
            m_oMemStream = new MemoryStream(iPackingCapacity);
            m_oMemStream.SetLength(iPackingCapacity);
            m_oMemStream.Seek(0, SeekOrigin.Begin);
            m_oBinWriter = new BinaryWriter(m_oMemStream);
            m_oBinReader = new BinaryReader(m_oMemStream);
            m_iDataSize = iPackingCapacity;
            m_arrPackedData = m_oMemStream.GetBuffer();
            */

            if (m_dataStream.GetDataSize() >= iPackingCapacity)
            {
                return;
            }

            byte[] data = new byte[iPackingCapacity];
            m_dataStream.Init(data, iPackingCapacity);

        }




        //  往流写入变化数据
        private void __WriteVarintNumber32(uint value)
        {

            while (value > MAX_7_BITS)
            {
                uint uiCur = (value & MAX_7_BITS) | (1 << MAX_BYTE_STORE_BITS);   // 最高位置1
                //m_oBinWriter.Write((byte)uiCur);
                m_dataStream.Write((byte)uiCur);
                value = value >> MAX_BYTE_STORE_BITS;
            }

            m_dataStream.Write((byte)value);
        }

        //  往流写入变化数据
        private void __WriteVarintNumber64(ulong value)
        {
            while (value > MAX_7_BITS)
            {
                byte uiCur = (byte)((value & MAX_7_BITS) | (1 << MAX_BYTE_STORE_BITS));   // 最高位置1
                m_dataStream.Write(uiCur);
                value = value >> MAX_BYTE_STORE_BITS;
            }

            m_dataStream.Write((byte)value);
        }


        private uint __ReadVarintNumber32()
        {
            uint uRet = 0;


            int iFreeSpace = m_dataStream.GetFreeSpaceSize(); //m_oMemStream.Capacity - (int)m_oMemStream.Position;
            int iMaxByte = Math.Min(iFreeSpace, 5);

            //  最多读取5字节
            int iShiftBit = 0;
            for (int i = 0; i < iMaxByte; ++i)
            {
                byte byCur = m_dataStream.ReadByte();

                if (byCur > MAX_7_BITS)
                {
                    uRet += ((uint)(byCur - 128) << iShiftBit);
                    iShiftBit += MAX_BYTE_STORE_BITS;
                }
                else
                {
                    uRet += ((uint)byCur << iShiftBit);
                    return uRet;
                }
            }

            __ThrowException(CPackDataException.EN_EXCEPTION.VARIENT_DECODE_ERROR);
            return uRet;
        }

        private ulong __ReadVarintNumber64()
        {
            ulong uRet = 0;

            int iFreeSpace = m_dataStream.GetFreeSpaceSize(); //m_oMemStream.Capacity - (int)m_oMemStream.Position;
            int iMaxByte = Math.Min(iFreeSpace, 10);

            int iShiftBit = 0;
            for (int i = 0; i < iMaxByte; ++i)
            {
                byte byCur = m_dataStream.ReadByte();

                if (byCur > MAX_7_BITS)
                {
                    uRet += ((ulong)(byCur - 128) << iShiftBit);
                    iShiftBit += MAX_BYTE_STORE_BITS;
                }
                else
                {
                    uRet += ((ulong)byCur << iShiftBit);
                    return uRet;
                }
            }

            __ThrowException(CPackDataException.EN_EXCEPTION.VARIENT_DECODE_ERROR);
            return uRet;
        }


        protected void __CheckFreeSpace(int iNeed)
        {
            int iFreeSpace = m_dataStream.GetFreeSpaceSize();// m_oMemStream.Capacity - (int)m_oMemStream.Position;
            if (iFreeSpace < iNeed)
            {
                __ThrowException(CPackDataException.EN_EXCEPTION.BUFFER_REACH_END);
            }
        }

        public void WriteUINT8(byte val)
        {
            __CheckFreeSpace(sizeof(byte));

            m_dataStream.Write(val);
        }

        public void WriteVarintUINT16(ushort val)
        {
            __CheckFreeSpace(3);
            __WriteVarintNumber32((uint)val);
        }

        public void WriteVarintUINT32(uint val)
        {
            __CheckFreeSpace(5);
            __WriteVarintNumber32((uint)val);
        }

        public void WriteVarintUINT64(ulong val)
        {
            __CheckFreeSpace(10);
            __WriteVarintNumber64(val);
        }

        public void WriteINT8(sbyte val)
        {
            __CheckFreeSpace(sizeof(sbyte));

            m_dataStream.Write(val);
        }

        public void WriteVarintINT16(short val)
        {
            __CheckFreeSpace(3);
            uint _uVal = CPackMisc.ZigZagEncode((int)val);
            __WriteVarintNumber32(_uVal);
        }

        public void WriteVarintINT32(int val)
        {
            __CheckFreeSpace(5);
            uint _uVal = CPackMisc.ZigZagEncode(val);
            __WriteVarintNumber32(_uVal);
        }

        public void WriteVarintINT64(long val)
        {
            __CheckFreeSpace(10);
            ulong _uVal = CPackMisc.ZigZagEncode(val);
            __WriteVarintNumber64(_uVal);
        }


        public void WriteFLOAT(float val)
        {
            uint iVal;
            unsafe
            {

                uint* p = (uint*)(&val);
                iVal = *p;
            }

            WriteVarintUINT32(iVal);
        }

        public void WriteDOUBLE(double val)
        {
            ulong iVal;
            unsafe
            {
                ulong* p = (ulong*)(&val);
                iVal = *p;
            }

            WriteVarintUINT64(iVal);
        }



        public void WriteSTRING(string val)
        {
            Byte[] arrString = Encoding.UTF8.GetBytes((val != null) ? val : string.Empty);
            if (arrString == null)
            {
                throw new Exception("WriteSTRING 转换字符串失败");
            }

            //  字符串数据 + vuint(size )
            int iStrLen = arrString.Length;
            __CheckFreeSpace(iStrLen + 5);

            //  先压vuint32长度
            WriteVarintUINT32((uint)iStrLen);
            if (iStrLen > 0)
            {
                m_dataStream.Write(arrString);
            }

        }

        public void WriteFixedUINT32(uint val)
        {
            __CheckFreeSpace(sizeof(uint));

            uint iCur = val;
            for (int i = 0; i < sizeof(uint); ++i)
            {
                m_dataStream.Write((byte)iCur);
                iCur = iCur >> 8;
            }
        }

        public void WriteFixedNumberAtPos(uint val, uint pos)
        {
            //if (pos >= m_oMemStream.Capacity)
            if (pos >= m_dataStream.GetDataSize())
            {
                __ThrowException(CPackDataException.EN_EXCEPTION.BUFFER_REACH_END);
            }

            int iOldPos = (int)m_dataStream.GetPos();// m_oMemStream.Position;
            m_dataStream.Seek((int)pos);
            WriteFixedUINT32(val);
            m_dataStream.Seek(iOldPos);
        }



        public byte ReadUINT8()
        {
            __CheckFreeSpace(1);
            return m_dataStream.ReadByte();
        }

        public ushort ReadVarintUINT16()
        {
            return (ushort)__ReadVarintNumber32();
        }


        public uint ReadVarintUINT32()
        {
            return (uint)__ReadVarintNumber32();
        }

        public ulong ReadVarintUINT64()
        {
            return (ulong)__ReadVarintNumber64();
        }

        public sbyte ReadINT8()
        {
            __CheckFreeSpace(1);
            return m_dataStream.ReadSByte();
        }

        public short ReadVarintINT16()
        {
            uint uEncodeVal = __ReadVarintNumber32();
            short val = (short)CPackMisc.ZigZagDecode(uEncodeVal);
            return val;
        }

        public int ReadVarintINT32()
        {
            uint uEncodeVal = __ReadVarintNumber32();
            int val = CPackMisc.ZigZagDecode(uEncodeVal);
            return val;
        }

        public long ReadVarintINT64()
        {
            ulong uEncodeVal = __ReadVarintNumber64();
            long val = CPackMisc.ZigZagDecode(uEncodeVal);
            return val;
        }

        public float ReadFLOAT()
        {
            uint iData = ReadVarintUINT32();
            float fVal;
            unsafe
            {

                float* p = (float*)(&iData);
                fVal = *p;
            }
            return fVal;
        }

        public double ReadDOUBLE()
        {
            ulong lData = ReadVarintUINT64();
            double dblVal;
            unsafe
            {
                double* p = (double*)(&lData);
                dblVal = *p;
            }
            return dblVal;
        }

        public string ReadSTRING()
        {
            string strRet;
            int iStrLen = (int)ReadVarintUINT32();
            if (iStrLen > 0)
            {
                //byte[] strUtf8 = m_dataStream.ReadBytes(iStrLen);
                //strRet = Encoding.UTF8.GetString(strUtf8);
                strRet = m_dataStream.GetString(iStrLen);
            }
            else
            {
                strRet = "";
            }


            return strRet;
        }

        public uint ReadFixedUINT32()
        {
            __CheckFreeSpace(sizeof(uint));

            uint uRet = 0;
            for (int i = 0; i < sizeof(uint); ++i)
            {
                uint iCur = m_dataStream.ReadByte();

                uRet += iCur << (8 * i);
            }

            return uRet;
        }

        public void SetDataOffset(int iPos)
        {
            //if (iPos > m_oMemStream.Capacity)
            if (iPos > m_dataStream.GetDataSize())
            {
                __ThrowException(CPackDataException.EN_EXCEPTION.BUFFER_REACH_END);
            }

            // m_oMemStream.Seek(iPos, SeekOrigin.Begin);
            m_dataStream.Seek(iPos);
        }


        public bool SkipField(uint uHeadTag)
        {
            uint uiDataLen;

            uint uWireType = CPackMisc.FHEAD_GET_WIRETYPE(uHeadTag);
            switch ((EN_WIRETYPE)uWireType)
            {
                case EN_WIRETYPE.enWT_VARINT:
                    {
                        ulong temp = __ReadVarintNumber64();
                        return true;
                    }
                case EN_WIRETYPE.enWT_BINDATA:
                    {
                        uiDataLen = ReadFixedUINT32();
                    }
                    break;
                case EN_WIRETYPE.enWT_FIXED8:
                    {
                        uiDataLen = 1;
                    }
                    break;
                case EN_WIRETYPE.enWT_STRING:
                    {
                        uiDataLen = __ReadVarintNumber32();
                    }
                    break;

                default:
                    {
                        __ThrowException(CPackDataException.EN_EXCEPTION.UNKNOWN_WIRETYPE);
                        return false;
                    }
            }

            __CheckFreeSpace((int)uiDataLen);
            //m_oMemStream.Seek(uiDataLen, SeekOrigin.Current);		
            m_dataStream.Seek((int)uiDataLen + m_dataStream.GetPos());
            return true;
        }
    }



    //  打包杂项功能
    public class CPackMisc
    {

        public static uint FHEAD_GET_FID(uint fhead) { return fhead >> 2; }

        public static uint FHEAD_GET_WIRETYPE(uint fhead) { return fhead & 0x3; }

        public static uint FHEAD_MAKE(uint tag, uint wiretype) { return ((tag << 2) | wiretype); }

        public static uint ZigZagEncode(int n)
        {
            return (uint)((n << 1) ^ (n >> 31));
        }
        public static ulong ZigZagEncode(long n)
        {
            return (ulong)((n << 1) ^ (n >> 63));
        }

        public static int ZigZagDecode(uint n)
        {
            return (int)((n >> 1) ^ -(int)(n & 1));
        }

        public static long ZigZagDecode(ulong n)
        {
            return (long)((long)(n >> 1) ^ -(long)(n & 1));
        }



        //   统计arrBits数组中1的个数
        public static uint CountBitsOne(uint[] arrBits)
        {
            uint iCount = 0;
            for (int i = 0; i < arrBits.Length; ++i)
            {
                uint dwCur = arrBits[i];
                while (dwCur != 0)
                {
                    if ((dwCur & 1) == 1)
                    {
                        iCount++;
                    }
                    dwCur = dwCur >> 1;
                }
            }
            return iCount;
        }

    };


}

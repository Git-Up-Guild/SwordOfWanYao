/*******************************************************************
** 文件名:	INet.cs
** 版  权:	(C) 冰川网络网络科技有限公司
** 创建人:	许德纪
** 日  期:	2018.6.27
** 版  本:	1.0
** 描  述:	
** 应  用:  网络解包合并

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Common
{
    public class PackDataMerge
    {
        private byte[] m_aryData = new byte[128*1024] ;
        private int m_dataLen;
        private bool m_isPacking = false;

        //清除数据
        public void StartMerging()
        {
            m_dataLen = 0;
            m_isPacking = true;
        }

        public void EndMerging()
        {
            m_isPacking = false;
        }

        public bool IsMerging()
        {
            return m_isPacking;
        }

        //合并数据
        public void MergeData(byte[] data,int len)
        {
            if(data==null ||len==0)
            {
                return;

            }

            int newLen = len + m_dataLen;
            if (newLen > m_aryData.Length)
            {
                byte[] newData = new byte[newLen + 1024];
                if(m_dataLen>0)
                {
                    Array.Copy(m_aryData, newData, m_dataLen);
                }
               
                m_aryData = newData;
            }

            Array.Copy(data,0, m_aryData, m_dataLen, len);
            m_dataLen = newLen;
        }

        //获取数据源
        public byte[] GetData()
        {
            return m_aryData;
        }

        //获取数据长度
        public int GetDataLen()
        {
            return m_dataLen;
        }



        
    }
}

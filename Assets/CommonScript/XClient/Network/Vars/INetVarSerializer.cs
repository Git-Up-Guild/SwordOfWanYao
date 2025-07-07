/*******************************************************************
** 文件名:	INetVarSerializer.cs
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
using UnityEngine;

namespace XClient.Network
{
    /// <summary>
    /// 网络变量序列化
    /// </summary>
    public interface INetVarSerializer
    {
        long ReadLong();
        float ReadFloat();
        Vector3 ReadVector3();
        string ReadString();

        List<int> ReadIntArray();

        List<float> ReadFloatArray();

        void WriteLong(long val);
        void WriteFloat(float val);
        void WriteVector3(Vector3 val);
        void WriteString(string val);

        void WriteIntArray(List<int> val);
        void WriteFloatArray(List<float> val);
    }
}

/*******************************************************************
** 文件名:	NetVarImpl.cs
** 版  权:	(C) 冰川网络
** 创建人:	郑秀程
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  网络变量类实现

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using static XClient.Network.NetVarValue;

namespace XClient.Network
{
    public class NetVarFloatArray : NetVar<List<float>>
    {
        private static List<float> s_Temp = new List<float>();

        public override NetVarDataType DataType => NetVarDataType.FloatArray;

        protected override void OnInit(object context = null)
        {
            base.OnInit(context);
            if(m_Value == null)
                m_Value = new List<float>();
        }

        protected override bool IsEqual(List<float> val1, List<float> val2)
        {
            if (val1 == null && val2 == null)
                return true;

            if (val1 == null || val2 == null)
                return false;

            return val1.Equals(val2);
        }

        public override void Clear()
        {
            Value.Clear();
        }

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteFloatArray(RemoteValueDelta);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            s_Temp.Clear();
            s_Temp.AddRange(serializer.ReadFloatArray());
            RemoteValueDelta = s_Temp;
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteFloatArray(Value);
        }

        public override void Read(INetVarSerializer serializer)
        {
            s_Temp.Clear();
            s_Temp.AddRange(serializer.ReadFloatArray());
            SyncValue = s_Temp;
        }

        public override void Read(NetVarValue varValue)
        {
            s_Temp.Clear();
            s_Temp.AddRange(varValue.floatArrValue);
            SyncValue = s_Temp;
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.floatArrValue.AddRange(Value);
        }

        protected override void OnSetNewValue(List<float> newValue, ValueUseage useage)
        {
            if (useage == ValueUseage.Value)
            {
                m_Value.Clear();
                m_Value.AddRange(newValue);
            }
            else if (useage == ValueUseage.RemoteValueDelta)
            {
                m_RemoteValueDelta.Clear();
                m_RemoteValueDelta.AddRange(newValue);
            }
            else
            {

            }
        }

        public override string ValueString(List<float> val)
        {
            if (val == null)
                return "";
            return string.Join(",", val.ToArray());
        }
    }

    public class NetVarIntArray : NetVar<List<int>>
    {
        private static List<int> s_Temp = new List<int>();

        public override NetVarDataType DataType => NetVarDataType.IntArray;

        protected override void OnInit(object context = null)
        {
            base.OnInit(context);

            if (m_Value == null)
                m_Value = new List<int>();
        }

        protected override bool IsEqual(List<int> val1, List<int> val2)
        {
            if (val1 == null && val2 == null)
                return true;

            if (val1 == null || val2 == null)
                return false;

            return val1.Equals(val2);
        }

        public override void Clear()
        {
            Value.Clear();
        }

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteIntArray(RemoteValueDelta);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            s_Temp.Clear();
            s_Temp.AddRange(serializer.ReadIntArray());
            RemoteValueDelta = s_Temp;
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteIntArray(Value);
        }

        public override void Read(INetVarSerializer serializer)
        {
            s_Temp.Clear();
            s_Temp.AddRange(serializer.ReadIntArray());
            SyncValue = s_Temp;
        }

        public override void Read(NetVarValue varValue)
        {
            s_Temp.Clear();
            s_Temp.AddRange(varValue.intArrValue);
            SyncValue = s_Temp;
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.intArrValue.AddRange(Value);
        }

        protected override void OnSetNewValue(List<int> newValue, ValueUseage useage)
        {
            if (useage == ValueUseage.Value)
            {
                m_Value.Clear();
                m_Value.AddRange(newValue);
            }
            else if (useage == ValueUseage.RemoteValueDelta)
            {
                m_RemoteValueDelta.Clear();
                m_RemoteValueDelta.AddRange(newValue);
            }
            else
            {

            }
        }

        public override string ValueString(List<int> val)
        {
            if (val == null)
                return "";
            return string.Join(",", val.ToArray());
        }
    }

    public class NetVarBool : NetVar<bool>
    {
        public override NetVarDataType DataType => NetVarDataType.Bool;

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteLong(RemoteValueDelta ? 1 : 0);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            Value = serializer.ReadLong() == 0 ? false : true;
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteLong(Value ? 1 : 0);
        }

        public override void Read(INetVarSerializer serializer)
        {
            SyncValue = serializer.ReadLong() == 0 ? false : true;
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.lValue = Value ? 1 : 0;
        }

        public override void Read(NetVarValue varValue)
        {
            SyncValue = varValue.lValue == 0 ? false : true;
        }

        protected override void OnLocalDeltaValue(bool deltaValue)
        {
            Value = deltaValue;
        }
    }

    public class NetVarInt : NetVar<int>
    {
        public override NetVarDataType DataType => NetVarDataType.Int;

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteLong(RemoteValueDelta);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            var valueDelta = (int)serializer.ReadLong();
            Value = Value + valueDelta;
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteLong(Value);
        }

        public override void Read(INetVarSerializer serializer)
        {
            SyncValue = (int)serializer.ReadLong();
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.lValue = Value;
        }

        public override void Read(NetVarValue varValue)
        {
            Value = (int)varValue.lValue;
        }

        protected override void OnLocalDeltaValue(int deltaValue)
        {
            Value += deltaValue;
        }
    }

    public class NetVarLong : NetVar<long>
    {
        public override NetVarDataType DataType => NetVarDataType.Long;

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteLong(RemoteValueDelta);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            Value = Value + serializer.ReadLong();
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteLong(Value);
        }

        public override void Read(INetVarSerializer serializer)
        {
            SyncValue = serializer.ReadLong();
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.lValue = Value;
        }

        public override void Read(NetVarValue varValue)
        {
            Value = varValue.lValue;
        }

        protected override void OnLocalDeltaValue(long deltaValue)
        {
            Value += deltaValue;
        }
    }

    public class NetVarFloat : NetVar<float>
    {
        public override NetVarDataType DataType => NetVarDataType.Float;

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteFloat(RemoteValueDelta);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            Value = Value + serializer.ReadFloat();
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteFloat(Value);
        }

        public override void Read(INetVarSerializer serializer)
        {
            SyncValue = serializer.ReadFloat();
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.fValue = Value;
        }

        public override void Read(NetVarValue varValue)
        {
            Value = varValue.fValue;
        }

        protected override void OnLocalDeltaValue(float deltaValue)
        {
            Value += deltaValue;
        }
    }

    public class NetVarVector3 : NetVar<Vector3>
    {
        public override NetVarDataType DataType => NetVarDataType.Vector3;

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteVector3(RemoteValueDelta);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            Value = Value + serializer.ReadVector3();
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteVector3(Value);
        }

        public override void Read(INetVarSerializer serializer)
        {
            SyncValue = serializer.ReadVector3();
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.vec3Value = Value;
        }

        public override void Read(NetVarValue varValue)
        {
            Value = varValue.vec3Value;
        }

        protected override void OnLocalDeltaValue(Vector3 deltaValue)
        {
            Value += deltaValue;
        }
    }

    public class NetVarString : NetVar<string>
    {
        public override NetVarDataType DataType => NetVarDataType.String;

        public override void WriteRemoteValueDelta(INetVarSerializer serializer)
        {
            serializer.WriteString(RemoteValueDelta);
        }

        public override void ReadRemoteValueDelta(INetVarSerializer serializer)
        {
            Value =  serializer.ReadString();
        }

        public override void Write(INetVarSerializer serializer)
        {
            serializer.WriteString(Value);
        }

        public override void Read(INetVarSerializer serializer)
        {
            SyncValue = serializer.ReadString();
        }

        public override void Write(NetVarValue varValue)
        {
            varValue.sValue = Value;
        }

        public override void Read(NetVarValue varValue)
        {
            Value = varValue.sValue;
        }

        protected override bool IsEqual(string val1, string val2)
        {
            return string.Equals(val1, val2);
        }

        protected override void OnLocalDeltaValue(string deltaValue)
        {
            Value = deltaValue;
        }
    }
}

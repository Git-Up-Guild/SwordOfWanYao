using gamepol;
using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity.Net;
using XGame.Utils;

namespace XClient.Network
{
    /// <summary>
    /// TCSMessage专属网络变量序列化器
    /// </summary>
    public abstract class NetableSerializerForCSMessage : INetableSerializer
    {
        class CSMessagePropertyNetVarSerializer : INetVarSerializer
        {
            private TAbsValue[] m_propertyValues;

            public TAbsValue[] propertyValues
            {
                get => m_propertyValues;
                set
                {
                    //if (value == null)
                    //     NetworkManager.Debug.Error("set propertyValues to null.");

                    m_propertyValues = value;
                }
            }

            public int propertyIndex;

            private List<int> m_IntVals = new List<int>();
            private List<float> m_FloatVals = new List<float>();

            public float ReadFloat()
            {
                return propertyValues[propertyIndex].get_fVal();
            }

            public List<float> ReadFloatArray()
            {
                m_FloatVals.Clear();

                var prop = propertyValues[propertyIndex];
                var len = prop.get_iFloatValNum();
                var arrVal = prop.get_arrFloatVal();
                for(var i = 0;i < len; i++)
                {
                    m_FloatVals.Add(arrVal[i]);
                }
                return m_FloatVals;
            }

            public List<int> ReadIntArray()
            {
                m_IntVals.Clear();

                var prop = propertyValues[propertyIndex];
                var len = prop.get_iIntValNum();
                var arrVal = prop.get_arrIntVal();
                for (var i = 0; i < len; i++)
                {
                    m_IntVals.Add(arrVal[i]);
                }
                return m_IntVals;
            }

            public long ReadLong()
            {
                return propertyValues[propertyIndex].get_iVal();
            }

            public string ReadString()
            {
                return propertyValues[propertyIndex].get_sVal();
            }

            public Vector3 ReadVector3()
            {
                var val =   propertyValues[propertyIndex].get_vec3();
                return new Vector3(val.get_x(), val.get_y(), val.get_z());
            }

            public void WriteFloat(float val)
            {
                propertyValues[propertyIndex].set_fVal(val);
            }

            public void WriteFloatArray(List<float> val)
            {
                var prop = propertyValues[propertyIndex];
                prop.set_iFloatValNum((byte)val.Count);
                var arrVal = prop.set_arrFloatVal();
                for (var i = 0; i < val.Count; i++)
                {
                    arrVal[i] = val[i];
                }
            }

            public void WriteIntArray(List<int> val)
            {
                var prop = propertyValues[propertyIndex];

                if(val == null || prop == null)
                {
                    NetworkManager.Debug.Error("反序列化NetObject数据出错！");
                    return;
                }

                prop.set_iIntValNum((byte)val.Count);
                var arrVal = prop.set_arrIntVal();
                for (var i = 0; i < val.Count; i++)
                {
                    arrVal[i] = val[i];
                }
            }

            public void WriteLong(long val)
            {
                propertyValues[propertyIndex].set_iVal(val);
            }

            public void WriteString(string val)
            {
                propertyValues[propertyIndex].set_sVal(val);
            }

            public void WriteVector3(Vector3 val)
            {
                var v = propertyValues[propertyIndex].set_vec3();
                v.set_x(val.x);
                v.set_y(val.y);
                v.set_z(val.z);
            }
        }

        private CSMessagePropertyNetVarSerializer m_PropSerializer = new CSMessagePropertyNetVarSerializer();

        private TPropertySet m_PropSet;

        /// <summary>
        /// 是否正在工作
        /// </summary>
        public bool IsWorking => m_PropSet != null;

        /// <summary>
        /// 设置好
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propSet"></param>
        /// <returns></returns>
        public void Setup(TPropertySet propSet)
        {
            m_PropSet = propSet;
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            m_PropSet = null;
            m_PropSerializer.propertyValues = null;
        }

        /// <summary>
        /// 序列化到数据集
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isDirtyOnly"></param>
        public bool Serializer(INetable obj, bool isDirtyOnly = true)
        {
            m_PropSerializer.propertyValues = m_PropSet.set_arrVal();
            m_PropSerializer.propertyIndex = 0;
            
            var arrID = m_PropSet.set_arrID();

            var netVars = obj.NetVars;
            for (var i = 0; i < netVars.Count; i++)
            {
                var netVar = netVars[i];
                if(!isDirtyOnly || (isDirtyOnly && netVar.IsDirty))
                {
                    netVar.ClearDirty();
                    arrID[m_PropSerializer.propertyIndex] = i; //网络变量的下标就是属性的ID

                    if (netVar.IsDebug)
                        NetworkManager.Debug.Log($"发送网络变量广播: {netVar} ");

                    netVar.Write(m_PropSerializer);
                    m_PropSerializer.propertyIndex++;
                }
            }

            //没有任何数据进行了序列化
            if (m_PropSerializer.propertyIndex == 0)
            {
                return true;
            }

            m_PropSet.set_iNum(m_PropSerializer.propertyIndex);

            return true;
        }

        /// <summary>
        /// 从数据集中反序列化
        /// </summary>
        /// <param name="obj"></param>
        public bool Unserializer(INetable obj)
        {
            var arrID = m_PropSet.set_arrID();
            var propNum = m_PropSet.get_iNum();
            var arrVals = m_PropSet.get_arrVal();

            if (arrVals == null && propNum > 0)
            {
                Debug.LogError($"反序列化 NetObject 失败，属性数组的长度与数量不匹配！NetID={obj.NetID}");
                return false;
            }

            m_PropSerializer.propertyValues = arrVals;

            var netVars = obj.NetVars;
            var netVarsCount = netVars.Count;

            for (var i = 0; i < propNum; i++)
            {
                var propID = arrID[i];
                if(propID > netVarsCount-1)
                {
                    Debug.LogError($"反序列化 NetObject 失败，属性ID下标越界！netID={obj.NetID}, total={netVarsCount}, cur={propID}");
                }
                else
                {
                    INetVar netVar = null;
                    try
                    {
                        netVar = netVars[propID];               //属性ID就是网络变量的下标
                        m_PropSerializer.propertyIndex = i;
                        netVar.Read(m_PropSerializer);

                        //if (netVar.IsDebug)
                        //    NetworkManager.Debug.Log($"收到网络变量广播: {netVar} ");
                    }
                    catch(Exception e)
                    {
                        NetworkManager.Debug.Error($"反序列化网络变量失败: {propID}#{netVar},  {e.StackTrace}");
                    }
                }
            }

            return true;
        }

        public bool RemoteValueDeltaSerializer(INetable obj, bool isDirtyOnly = true)
        {
            m_PropSerializer.propertyValues = m_PropSet.set_arrVal();
            m_PropSerializer.propertyIndex = 0;

            var arrID = m_PropSet.set_arrID();

            var netVars = obj.NetVars;
            for (var i = 0; i < netVars.Count; i++)
            {
                var netVar = netVars[i];
                if (netVar.IsRemoteValueDeltaDirty)
                {
                    arrID[m_PropSerializer.propertyIndex] = i; //网络变量的下标就是属性的ID
                    netVar.WriteRemoteValueDelta(m_PropSerializer);
                    netVar.ClearRemoteValueDelta();
                    netVar.ClearRemoteValueDeltaDirty();
                    m_PropSerializer.propertyIndex++;
                }
            }

            m_PropSet.set_iNum(m_PropSerializer.propertyIndex);

            return m_PropSerializer.propertyIndex > 0;
        }

        public bool RemoteValueDeltaUnserializer(INetable obj)
        {
            m_PropSerializer.propertyValues = m_PropSet.get_arrVal();
            var arrID = m_PropSet.set_arrID();
            var propNum = m_PropSet.get_iNum();

            var netVars = obj.NetVars;
            var netVarsCount = netVars.Count;

            for (var i = 0; i < propNum; i++)
            {
                var propID = arrID[i];
                if (propID > netVarsCount - 1)
                {
                    Debug.LogError($"属性增量值反序列化 NetObject 失败，属性ID下标越界！netID={obj.NetID}, total={netVarsCount}, cur={propID}");
                }
                else
                {
                    INetVar netVar = null;
                    try
                    {
                        netVar = netVars[propID];               //属性ID就是网络变量的下标
                        m_PropSerializer.propertyIndex = i;
                        netVar.ReadRemoteValueDelta(m_PropSerializer);
                    }
                    catch(Exception e)
                    {
                        NetworkManager.Debug.Error($"序列化网络变量增量失败: {i}#{netVar},  {e.StackTrace}");
                    }
                }

            }

            return true;
        }
    }

    /// <summary>
    /// TCSMessage专属网络变量序列化器
    /// </summary>
    public class NetableSerializerForReceiveCSMessage : NetableSerializerForCSMessage, INetableSerializer
    {
        public static NetableSerializerForReceiveCSMessage Instance = new NetableSerializerForReceiveCSMessage();
    }

    /// <summary>
    /// TCSMessage专属网络变量序列化器
    /// </summary>
    public class NetableSerializerForSendCSMessage : NetableSerializerForCSMessage, INetableSerializer
    {
        public static NetableSerializerForSendCSMessage Instance = new NetableSerializerForSendCSMessage();
    }
}

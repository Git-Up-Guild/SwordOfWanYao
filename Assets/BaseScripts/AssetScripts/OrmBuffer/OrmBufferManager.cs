/*******************************************************************
** 文件名:    OrmBufferManagerEditor.cs
** 版  权:    (C) 冰川网络网络科技
** 创建人:    郑秀程
** 日  期:    2016/3/16
** 版  本:    1.0
** 描  述:    UI设置脚本

**************************** 修改记录 ******************************
** 修改人:    
** 日  期:    
** 描  述:    
********************************************************************/

using gamepol;
using ORM;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using XClient.Common;
using XGame.Memory;
using XGame.Utils;

namespace XGame.AssetScript.OrmBuffer
{
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS && !UNITY_IPHONE
    class OrmBufferManagerEditor : UnityEditor.Editor
    {
        [MenuItem("XGame/OrmBuffer/统计")]
        public static void Output()
        {
            if (!Application.isPlaying)
                return;

            var ormBuffers = OrmBufferManager.Instance.GetAllOrmBuffers();
            StringBuilder sb = new StringBuilder();
            foreach(var b in ormBuffers.Values)
            {
                sb.AppendLine($"{b.ormTypeName}.{b.fieldPath}, size={b.byteData.nLen}");
            }
            Debug.LogError("[OrmBuffer] 当前缓存OrmBuffer统计：\n" + sb.ToString());
        }

        [MenuItem("XGame/OrmBuffer/测试")]
        public static void Test()
        {
            //OrmBufferManager.Instance.Create();

            //TCSMessage tcsMessage = new TCSMessage();
            //tcsMessage.Init(TCSMessage.MSG_RANKING_ATTACH1_LOAD_RSP);
            //var msg = tcsMessage.stTMSG_RANKING_ATTACH1_LOAD_RSP;

            //TBossRankingAttachment1 attach1 = new TBossRankingAttachment1();

            //CORM_packaux ormPacker = new CORM_packaux();

            //int size = TBossRankingAttachment0.MAX_PACKEDSIZE;
            //byte[] buffer = new byte[size];
            //ormPacker.Init(buffer, size);

            //TBossRankingAttachment0 attch0 = new TBossRankingAttachment0();
            //attch0.set_iFaceID(9991);
            //attch0.set_iWorldID(1);
            //attch0.set_szGuildName("冰川网络");

            ////把attch0打包到attch1
            //attch0.Pack(ormPacker);
            //var packedBuffer = ormPacker.GetData();
            //var attach1Buffer = attach1.set_arrBattleData();
            //packedBuffer.CopyTo(attach1Buffer, 0);
            //attach1.set_iLen(ormPacker.GetDataOffset());

            ////把attch1打包到rsp
            //ormPacker.Seek(0);
            //attach1.Pack(ormPacker);

            //byte[] ormBuffer = msg.set_arrAttach1();
            //buffer.CopyTo(ormBuffer, 0);
            //msg.set_iLen1(ormPacker.GetDataOffset());

            //int key = OrmBufferManager.Instance.CacheBufferField(msg, "arrAttach1");
            //attach1 = OrmBufferManager.Instance.ParseBufferToOrmObject(key, "gamepol.TBossRankingAttachment1") as TBossRankingAttachment1;
            //key = OrmBufferManager.Instance.CacheBufferField(attach1, "arrBattleData");
            //attch0 = OrmBufferManager.Instance.ParseBufferToOrmObject(key, "gamepol.TBossRankingAttachment0") as TBossRankingAttachment0;

            //Debug.Log($"faceid={attch0.get_iFaceID()}, worldid={attch0.get_iWorldID()}, guidename={attch0.get_szGuildName()}");
        }
    }
#endif

    /// <summary>
    /// Orm数据缓存对象
    /// </summary>
    public class OrmBufferManager : Singleton<OrmBufferManager>
    {
        /// <summary>
        /// Buffer对象
        /// </summary>
        public class OrmBuffer
        {
            public ByteData byteData;
            public string ormTypeName;
            public string fieldPath;
        }

        private static int BUFFER_ALLOC_IDX = 1;
        private MemPool m_MemPool;
        private Stack<OrmBuffer> m_OrmBufferPool;
        private Dictionary<int, OrmBuffer> m_DicOrmBuffer;
        private Type m_MessageType;
        private Dictionary<string, uint> m_DicMessageID;
        private CORM_packaux m_OrmPacker;
        private object[] m_UnpackMethodParams;
        private static string s_Tag = "[OrmBuffer]";
        private static Dictionary<string, object> s_DicOrmObj = new Dictionary<string, object>();

        public void Create()
        {
            if(m_MemPool == null)
            {
                m_MemPool = new MemPool();
                m_MemPool.Create(1024*64);              //排行榜的附件1数据是64K，这里先定这么大
                m_MemPool.SetMaxQueueSize(100);
                m_OrmBufferPool = new Stack<OrmBuffer>();

                m_DicOrmBuffer = new Dictionary<int, OrmBuffer>();
                m_MessageType = typeof(TCSMessage);
                m_DicMessageID = new Dictionary<string, uint>();
                m_OrmPacker = new CORM_packaux();
                m_UnpackMethodParams = new object[] { m_OrmPacker };

                var fields = m_MessageType.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var f in fields)
                {
                    try
                    {
                        uint msgID = (uint)f.GetRawConstantValue();
                        m_DicMessageID.Add(f.Name, msgID);
                    }
                    catch(Exception e)
                    {
                        //Debug.LogError($"{s_Tag}{f.Name} is not message id field." + e.Message);
                    }
                }
            }
        }

        public void Release()
        {
            m_MemPool?.Release();
            m_MemPool = null;
        }


        public Dictionary<int, OrmBuffer> GetAllOrmBuffers()
        {
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS && !UNITY_IPHONE
            return m_DicOrmBuffer;
#else
            return null;
#endif

        }


        /// <summary>
        /// 分配
        /// </summary>
        /// <param name="nSize"></param>
        /// <returns></returns>
        private OrmBuffer Alloc(int nSize)
        {
            OrmBuffer ormBuffer;
            if (m_OrmBufferPool.Count == 0)
                ormBuffer = new OrmBuffer();
            else
                ormBuffer = m_OrmBufferPool.Pop();

            ormBuffer.byteData = m_MemPool.Aloc(nSize);
            return ormBuffer;
        }

        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="ormBuffer"></param>
        private void Recycle(OrmBuffer ormBuffer)
        {
            m_MemPool.Recycle(ref ormBuffer.byteData);
            ormBuffer.byteData = null;
            ormBuffer.ormTypeName = string.Empty;
            ormBuffer.fieldPath = string.Empty;
            m_OrmBufferPool.Push(ormBuffer);
        }

        /// <summary>
        /// 获取消息ID
        /// </summary>
        /// <param name="msgName"></param>
        /// <returns></returns>
        public uint GetMessageID(string msgName)
        {
            if (m_DicMessageID.ContainsKey(msgName))
                return m_DicMessageID[msgName];
            return 0;
        }

        /// <summary>
        /// 查询消息体对象
        /// </summary>
        /// <param name="msgName"></param>
        /// <returns></returns>
        private FieldInfo GetMessageBody(string msgName)
        {
            string msgBodyName = $"stT{msgName}";
            FieldInfo msgBodyField = m_MessageType.GetField(msgBodyName, BindingFlags.Public | BindingFlags.Instance);
            if (msgBodyField == null)
                return null;
            return msgBodyField;
        }

        /// <summary>
        /// 获取消息属性值
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="tcsMessage"></param>
        /// <param name="fieldPath"></param>
        /// <returns></returns>
        public object GetMessageFieldValue(string msgName, TCSMessage tcsMessage, string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath))
            {
                Debug.LogError($"{s_Tag}fieldPath 无效");
                return null;
            }

            FieldInfo msgBodyField = GetMessageBody(msgName);
            if (msgBodyField == null)
            {
                Debug.LogError($"{s_Tag}查找 MessageBody 失败， msgName={msgName}");
                return null;
            }
                
            string[] fieldNames = fieldPath.Split('.');
            return GetSubFieldValue(msgBodyField, 0, tcsMessage, 0, ref fieldNames);
        }

        /// <summary>
        /// 获取子Field值
        /// </summary>
        /// <param name="parentField"></param>
        /// <param name="parentFieldOwnerObj"></param>
        /// <param name="deep"></param>
        /// <param name="fieldNames"></param>
        /// <returns></returns>
        private object GetSubFieldValue(FieldInfo parentField, int parentFieldValIdx, object parentFieldOwnerObj, int deep, ref string[] fieldNames)
        {
            object parentFieldValue = parentField.GetValue(parentFieldOwnerObj);

            //处理数组
            Type parentType = parentField.FieldType;
            if (parentField.FieldType.IsArray)
            {
                parentType = parentField.FieldType.GetElementType();
                parentFieldValue = ((Array)parentFieldValue).GetValue(parentFieldValIdx);
            }

            string subFieldName = fieldNames[deep];
            string[] temps = subFieldName.Split('#');
            int subFieldValIdx = -1;
            if (temps.Length >= 2)
            {
                subFieldName = temps[0];
                subFieldValIdx = int.Parse(temps[1]);
                subFieldValIdx = subFieldValIdx < 0 ? 0 : subFieldValIdx;
            }

            FieldInfo childField = parentType.GetField(subFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (childField == null)
            {
                Debug.LogError($"{s_Tag}查找 FieldInfo 失败， fieldName={subFieldName}");
                return null;
            }

            if (deep >= fieldNames.Length - 1)
            {
                object childFieldValue = childField.GetValue(parentFieldValue);

                //处理数组
                if (childField.FieldType.IsArray && subFieldValIdx >= 0)  
                    childFieldValue = ((Array)childFieldValue).GetValue(subFieldValIdx);

                return childFieldValue;
            }

            return GetSubFieldValue(childField, subFieldValIdx, parentFieldValue, deep + 1, ref fieldNames);
        }

        /// <summary>
        /// 获取Buffer属性
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="fieldPath"></param>
        public FieldInfo GetMessageField(string msgName, string fieldPath)
        {
            FieldInfo msgBodyField = GetMessageBody(msgName);
            if (msgBodyField == null)
                return null;
            string[] fieldNames = fieldPath.Split('.');
            return GetSubField(msgBodyField, 0, ref fieldNames);
        }

        /// <summary>
        /// 获取子Field
        /// </summary>
        /// <param name="parentField"></param>
        /// <param name="deep"></param>
        /// <param name="fieldNames"></param>
        /// <returns></returns>
        private FieldInfo GetSubField(FieldInfo parentField, int deep, ref string[] fieldNames)
        {
            string subFieldName = fieldNames[deep];

            Type parentType = parentField.FieldType;
            if (parentField.FieldType.IsArray)
                parentType = parentField.FieldType.GetElementType();

            FieldInfo childField = parentType.GetField(subFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (childField == null)
            {
                Debug.LogError($"{s_Tag}查找 FieldInfo 失败， fieldName={subFieldName}");
                return null;
            }

            if (deep >= fieldNames.Length - 1)
                return childField;

            return GetSubField(childField, deep + 1, ref fieldNames);
        }

        /// <summary>
        /// 将Buffer缓存起来
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="gameMessage"></param>
        /// <param name="fieldPath"></param>
        /// <returns></returns>
        public int CacheBufferField(string msgName, TCSMessage gameMessage, string fieldPath)
        {
            var key = BUFFER_ALLOC_IDX++;
            object fieldVal = GetMessageFieldValue(msgName, gameMessage, fieldPath);

            byte[] buffer = fieldVal as byte[];
            if (buffer == null)
            {
                Debug.LogError($"{s_Tag}缓存Buffer字段 失败，msgName={msgName}, fieldPath={fieldPath}");
                return 0;
            }
            else
            {
                var ormBuffer = Alloc(buffer.Length);
                buffer.CopyTo(ormBuffer.byteData.data, 0);
                ormBuffer.byteData.nLen = buffer.Length;
                ormBuffer.ormTypeName = msgName;
                ormBuffer.fieldPath = fieldPath;
                m_DicOrmBuffer.Add(key, ormBuffer);
                return key;
            }
        }

        /// <summary>
        /// 将Buffer缓存起来
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="fieldPath"></param>
        /// <returns></returns>
        public int CacheBufferField(string msgName, string fieldPath)
        {
            if (GameGlobal.Instance.NetModule == null)
                return 0;
            var gameMessage = GameGlobal.Instance.NetModule.GetGameMsg(false);
            return CacheBufferField(msgName, gameMessage, fieldPath);
        }

        /// <summary>
        /// 缓存指定对虾干的Buffer字段
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldPath"></param>
        /// <returns></returns>
        public int CacheBufferField(object obj, string fieldPath)
        {
            if(obj == null || string.IsNullOrEmpty(fieldPath))
            {
                Debug.LogError($"{s_Tag}缓存Buffer字段 失败，obj is null or fieldPath is null.");
                return 0;
            }    
            Type type = obj.GetType();
            string[] fieldNames = fieldPath.Split('.');

            //解析字段名称和取值索引
            string fieldName = fieldNames[0];
            int fieldValIndex = 0;
            string[] temps = fieldName.Split('#');
            if (temps.Length >= 2)
            {
                fieldName = temps[0];
                fieldValIndex = int.Parse(temps[1]);
                fieldValIndex = fieldValIndex < 0 ? 0 : fieldValIndex;
            }

            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if(fieldInfo == null)
            {
                Debug.LogError($"{s_Tag}缓存Buffer字段 失败，获取FieldInfo失败，fieldName={fieldName}.");
                return 0;
            }

            object val = null;
            if(fieldNames.Length > 1)
            {
                val = GetSubFieldValue(fieldInfo, fieldValIndex, obj, 1, ref fieldNames);
            }
            else
            {
                val = fieldInfo.GetValue(obj);
            }

            byte[] buffer = val as byte[];
            if (buffer == null)
            {
                Debug.LogError($"{s_Tag}缓存Buffer字段 失败，type={type.FullName}, fieldPath={fieldPath}");
                return 0;
            }
            else
            {
                var key = BUFFER_ALLOC_IDX++;
                var ormBuffer = Alloc(buffer.Length);
                buffer.CopyTo(ormBuffer.byteData.data, 0);
                ormBuffer.byteData.nLen = buffer.Length;
                ormBuffer.ormTypeName = type.FullName;
                ormBuffer.fieldPath = fieldPath;
                m_DicOrmBuffer.Add(key, ormBuffer);
                return key;
            }
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exist(int key)
        {
            return m_DicOrmBuffer.ContainsKey(key);
        }

        /// <summary>
        /// 丢弃缓存的Buffer
        /// </summary>
        /// <param name="key"></param>
        public void DiscardBuffer(int key)
        {
            OrmBuffer ormBuffer;
            if(m_DicOrmBuffer.TryGetValue(key, out ormBuffer))
            {
                m_DicOrmBuffer.Remove(key);
                Recycle(ormBuffer);
            }
            else
            {
                Debug.LogError($"{s_Tag}缓存的Buffer数据不存在，key={key}");
            }
        }

        /// <summary>
        /// 直接解析消息中的Buffer到指定类型的Orm类中
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="fieldPath"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public object ParseBufferToOrmObject(string msgName, string fieldPath, string typeName)
        {
            if (GameGlobal.Instance.NetModule == null)
                return null;
            var gameMessage = GameGlobal.Instance.NetModule.GetGameMsg(false);
            return ParseBufferToOrmObject(msgName, gameMessage, fieldPath, typeName);
        }

        /// <summary>
        /// 直接解析消息中的Buffer到指定类型的Orm类中
        /// </summary>
        /// <param name="msgName"></param>
        /// <param name="message"></param>
        /// <param name="fieldPath"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public object ParseBufferToOrmObject(string msgName, TCSMessage message, string fieldPath, string typeName)
        {
            object fieldVal = GetMessageFieldValue(msgName, message, fieldPath);
            byte[] buffer = fieldVal as byte[];
            if (buffer == null)
            {
                Debug.LogError($"{s_Tag}获取Buffer字段 失败，msgName={msgName}, fieldPath={fieldPath}");
                return null;
            }

            object obj = CreateUnpackToOrmObject(typeName);
            if(obj == null)
            {
                Debug.LogError($"{s_Tag}创建Orm对象失败，不存在的目标类型，msgName={msgName}, typeName={typeName}");
                return null;
            }

            if(!UnpackBufferToTargetObject(buffer, obj))
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，目标类型解包失败，msgName={msgName}, typeName={typeName}");
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 将缓存的Buffer解析到目标结构中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public object ParseBufferToOrmObject(int key, Type ormObjType)
        {
            return ParseBufferToOrmObject(key, ormObjType.Name);
        }

        /// <summary>
        /// 将缓存的Buffer解析到目标结构中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ParseBufferToOrmObject<T>(int key) where T:class
        {
            Type type = typeof(T);
            return ParseBufferToOrmObject(key, type.Name) as T;
        }

        /// <summary>
        /// 将缓存的Buffer解析到目标结构中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="typeName"></param>
        public object ParseBufferToOrmObject(int key, string typeName)
        {
            OrmBuffer ormBuffer;
            if (!m_DicOrmBuffer.TryGetValue(key, out ormBuffer))
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，找不到缓存的Buffer数据，key={key}, typeName={typeName}");
                return null;
            }

            //根据类型创建实例
            object obj = CreateUnpackToOrmObject(typeName);
            if (obj == null)
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，不存在的目标类型，key={key}, typeName={typeName}");
                return null;
            }

            //解包Buffer到目标对象失败
            if (!UnpackBufferToTargetObject(ormBuffer.byteData.data, obj))
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，目标类型解包失败，key={key}, typeName={typeName}");
                return null;
            }

            return obj;
        }

        /// <summary>
        /// 将缓存的Buffer解析到目标结构中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ormObj"></param>
        /// <returns></returns>
        public bool ParseBufferToOrmObject(int key, object obj)
        {
            //检查Buffer是否存在
            OrmBuffer ormBuffer;
            if (!m_DicOrmBuffer.TryGetValue(key, out ormBuffer))
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，找不到缓存的Buffer数据，key={key}, typeName={obj.GetType().FullName}");
                return false;
            }

            //解包Buffer到目标对象失败
            if(!UnpackBufferToTargetObject(ormBuffer.byteData.data, obj))
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，目标类型解包失败，key={key}, typeName={obj.GetType().FullName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 解包到Orm对象
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool UnpackBufferToTargetObject(byte[] buffer, object target)
        {
            Type type = target.GetType();
            var methodInfo = type.GetMethod("Unpack", BindingFlags.Public | BindingFlags.Instance);
            if(methodInfo == null)
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，目标类型 没有 Unpack 函数，type={type.FullName}");
                return false;
            }

            try
            {
                m_OrmPacker.Init(buffer, buffer.Length);
                m_OrmPacker.Seek(0);
                methodInfo.Invoke(target, m_UnpackMethodParams);
            }
            catch(Exception e)
            {
                Debug.LogError($"{s_Tag}解析Buffer失败，Orm对象解包异常，type={type.FullName}，e={e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 创建Orm对象
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private object CreateUnpackToOrmObject(string typeName)
        {
            object obj;
            if(s_DicOrmObj.TryGetValue(typeName, out obj))
                return obj;

            // 获取当前程序集 
            Type type = m_MessageType.Assembly.GetType(typeName);

            if (type == null)
                return null;

            //根据类型创建实例
            obj = Activator.CreateInstance(type);
            if(obj != null)
                s_DicOrmObj.Add(typeName, obj);

            return obj;
        }
    }
}

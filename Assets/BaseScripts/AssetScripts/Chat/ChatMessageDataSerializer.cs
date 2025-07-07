
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 聊天记录存储器。为了避免加载大量的数据。将数据分成若干的小文件存储（100条为一组）。具体的逻辑其实在Lua模块中实现，而此类仅仅是辅助存储。
/// </summary>
namespace XGame.AssetScript.Chat
{
    public class ChatMessageDataSerializer
    {
        /// <summary>
        /// 一个临时的静态对象，用来进行快速的数据传递。
        /// </summary>
        public static ChatMessageData tempData = new ChatMessageData();

        /// <summary>
        /// 数据列表
        /// </summary>
        public static List<ChatMessageData> lstData = new List<ChatMessageData>();

        /// <summary>
        /// 对象池
        /// </summary>
        private static ObjectPool<ChatMessageData> dataPool = null;

        private static ChatMessageData AllocData()
        {
            if(dataPool == null)
            {
                dataPool = new ObjectPool<ChatMessageData>(() => {
                    return new ChatMessageData();
                });
            }

            return dataPool.Get();
        }

        private static void RecycleData(ChatMessageData data)
        {
            if (dataPool == null)
            {
                dataPool = new ObjectPool<ChatMessageData>(() =>
                {
                    return new ChatMessageData();
                });
            }

            dataPool.Release(data);
        }

        /// <summary>
        /// 做好清理，准备进行保存
        /// </summary>
        public static void Clear()
        {
            foreach(var d in lstData)
            {
                RecycleData(d);
            }
            lstData.Clear();
            tempData.Clear();
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name=""></param>
        public static void Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var path = $"{Application.persistentDataPath }/{fileName}";

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 从文件中加载数据
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns>加载回来了多少条数据</returns>
        public static int Load(string fileName)
        {
            Clear();

            var path = $"{Application.persistentDataPath }/{fileName}";

            if(!File.Exists(path))
            {
                return 0;
            }

            Stream stream = null;

            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var tempList = (List<ChatMessageData>)formatter.Deserialize(stream);
                lstData.AddRange(tempList);
            }
            catch(Exception e)
            {
            }
           
            if(stream != null)
                stream.Close();

            return lstData.Count;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="fileName"></param>
        public static void Save(string fileName)
        {
            var path = $"{Application.persistentDataPath }/{fileName}";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, lstData);
            stream.Close();
        }

        /// <summary>
        /// 添加数据，需要事先把数据设置到 tempData 中
        /// </summary>
        public static void AddData()
        {
            var data = AllocData();
            tempData.CopyTo(data);
            lstData.Add(data);
        }

        /// <summary>
        /// 获取数据，获取成功后，数据会通过 tempData 返回
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool GetData(int index)
        {
            if (index < lstData.Count)
            {
                lstData[index].CopyTo(tempData);
                return true;
            }

            return false;
        }

    }
}

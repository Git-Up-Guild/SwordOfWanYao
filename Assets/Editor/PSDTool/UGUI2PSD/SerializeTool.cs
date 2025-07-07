using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace UGUI2PSD
{
    public static class SerializeTool
    {
        public static byte[] Serialize<T>(T obj) where T : MonoBehaviour
        {
            using (MemoryStream ms = new MemoryStream())
            {
                //FileStream fs = new FileStream("demo.bin", FileMode.OpenOrCreate);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Close();
                ms.Dispose();
                return ms.GetBuffer();
            }
        }

        public static T Deserialize<T>(byte[] data) where T : MonoBehaviour
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                T obj = bf.Deserialize(ms) as T;
                ms.Close();
                ms.Dispose();
                return obj;
            }
        }

        public static void SerializeXml<T>(T obj) where T : MonoBehaviour
        {
            string path = Application.dataPath + "/class.xml";
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            XmlSerializer xml = new XmlSerializer(typeof(T));
            xml.Serialize(fs, obj);
            fs.Close();
        }

        public static T DeserializeXml<T>() where T : MonoBehaviour
        {
            string path = Application.dataPath + "/class.xml";
            FileStream fs = new FileStream(path, FileMode.Open);
            XmlSerializer bf = new XmlSerializer(typeof(T));
            T demo = bf.Deserialize(fs) as T;
            fs.Close();
            return demo;
        }
    }
}

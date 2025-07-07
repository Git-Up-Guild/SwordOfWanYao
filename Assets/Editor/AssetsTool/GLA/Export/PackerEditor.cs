using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace XGameEditor
{
    public class PackerEditor
    {

        [MenuItem("Assets/XGame/资源/加密资源")]
        public static void EncodeFile()
        {

            string selePatch = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

            byte[] pData = File.ReadAllBytes(selePatch);


            int size = pData.Length;

            #region 加密

            byte[] Key = { (byte)'Q', (byte)'1', (byte)'S', (byte)'Z', (byte)'Y', (byte)'Z' };

            int keylen = 6;
            int idxI = 0;

            size = size / 100;

            for (idxI = 0; idxI < size; ++idxI)
            {
                pData[idxI] ^= (byte)(Key[(idxI) % keylen]);
            }

            #endregion

            File.WriteAllBytes(selePatch + ".ec", pData);

            Debug.Log("加密文件完成!");
        }

        [MenuItem("Assets/XGame/资源/解密资源")]
        public static void DencodeFile()
        {

            string selePatch = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

            byte[] pData = File.ReadAllBytes(selePatch);


            int size = pData.Length;

            #region 加密

            byte[] Key = { (byte)'Q', (byte)'1', (byte)'S', (byte)'Z', (byte)'Y', (byte)'Z' };

            int keylen = 6;
            int idxI = 0;

            size = size / 100;

            for (idxI = 0; idxI < size; ++idxI)
            {
                pData[idxI] ^= (byte)(Key[(idxI) % keylen]);
            }

            #endregion

            File.WriteAllBytes(selePatch + ".ec", pData);

            Debug.Log("加密文件完成!");
        }

        [MenuItem("Assets/XGame/资源/加密资源文件夹")]
        public static void EncodeFiles()
        {
            string selePatch = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            string outputdir = GetArg("-inputdir");
            if (outputdir != null && outputdir != "")
            {
                selePatch = outputdir;
            }
            DirectoryInfo dirInfo = new DirectoryInfo(selePatch);

            FileInfo[] dirs = dirInfo.GetFiles("*.dll", SearchOption.AllDirectories);

            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i].Extension.EndsWith(".dll"))
                {
                    byte[] pData = File.ReadAllBytes(dirs[i].FullName);


                    int size = pData.Length;

                    #region 加密

                    byte[] Key = { (byte)'Q', (byte)'1', (byte)'S', (byte)'Z', (byte)'Y', (byte)'Z' };

                    int keylen = 6;
                    int idxI = 0;

                    size = size / 100;

                    for (idxI = 0; idxI < size; ++idxI)
                    {
                        pData[idxI] ^= (byte)(Key[(idxI) % keylen]);
                    }

                    #endregion

                    File.WriteAllBytes(dirs[i].FullName.Replace(".dll", ".data"), pData);
                }
            }

            Debug.Log("加密文件夹完成!");
        }
        private static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }

            return null;
        }

    }
}
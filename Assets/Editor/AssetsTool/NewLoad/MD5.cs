using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

public class MD5
{

    public static string GetTextureMD5(Sprite sprite)
    {

        string assetPath = AssetDatabase.GetAssetPath(sprite);

        return GetTextureMD5_AssetPath(assetPath);

    }
    public static string GetTextureMD5_AssetPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return null;
        string path = Application.dataPath;
        path = path.Substring(0, path.IndexOf("Assets")) + "/" + assetPath;

        return GetMD5(path);
    }

    public static string GetMD5(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("获取MD5失败：" + ex.ToString());
            return null;
            // System.Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
        }

    }

}

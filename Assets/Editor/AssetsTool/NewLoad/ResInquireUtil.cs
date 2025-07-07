using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;

/// <summary>
/// 资源遍历工具
/// </summary>
public class ResInquireUtil
{

    public delegate void TexutreCallBack(Texture2D texture, string path);
    public delegate void FileCallBack(FileSystemInfo file, string path);

    private TexutreCallBack Texutre_CallBack;
    private FileCallBack File_CallBack;

    public delegate bool FileCallBackWithToggle(string path);

    private FileCallBackWithToggle File_CallBackWithBreak;

    private bool isStop;
    public ResInquireUtil(string path, TexutreCallBack CallBack)
    {
        Texutre_CallBack = CallBack;
        ResInquire(path);
    }
    public ResInquireUtil(string path, FileCallBack CallBack)
    {
        File_CallBack = CallBack;
        ResInquire(path);
    }

    public ResInquireUtil(string path, FileCallBackWithToggle CallBack)
    {
        File_CallBackWithBreak = CallBack;
        ResInquire(path);
    }

    public void StopInquireRes()
    {
        isStop = true;
    }

    private void ResInquire(string path)
    {
        DirectoryInfo dire = new DirectoryInfo(path);
        if (null == dire || !dire.Exists)
        {
            if (dire != null)
            {
                FileSystemInfo file = dire as FileSystemInfo;
                ReadFile(file);
            }
            else
            {
                Debug.LogError(path + "不存在");
            }
        }
        else
        {
            ReadFile(dire);
        }
    }

    private void ReadFile(FileSystemInfo file)
    {
        if (isStop)
            return;

        DirectoryInfo dire = file as DirectoryInfo;
        if (dire != null && dire.Exists)
        {
            FileSystemInfo[] fileArr = dire.GetFileSystemInfos();
            if (fileArr != null)
            {
                int count = fileArr.Length;
                for (int i = 0; i < count; i++)
                {
                    ReadFile(fileArr[i]);
                }
            }

        }
        else
        {

            if (file.Extension != ".meta")
            {
                string path = file.FullName;

                if (File_CallBack != null)
                {
                    File_CallBack(file, path);
                }

                if (File_CallBackWithBreak != null)
                {
                    if (File_CallBackWithBreak(path) == false)
                    {
                        StopInquireRes();
                        return;
                    };
                }

                int Pos = path.IndexOf("Assets");
                if (Pos < 0)
                {
                    return;
                }
                string assetPath = path.Substring(Pos);
                if (IsTexture(file))
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
                    if (tex != null)
                    {
                        if (Texutre_CallBack != null)
                        {
                            Texutre_CallBack(tex, path);
                        }
                    }
                }
            }
        }
    }

    private bool IsTexture(FileSystemInfo file)
    {
        string path = file.FullName;
        string assetPath = path.Substring(path.IndexOf("Assets"));
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
        if (obj is Texture2D)
        {
            return true;
        }
        return false;
    }


}

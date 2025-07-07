/**************************************************
** 文 件 名：FileBrowser.cs
** 版    本：V1.0
** 创 建 人：李世柳
** 创建日期：2020/1/12 16:49:54
** 内容简述：资源文件遍历工具
** 修改记录：
日期 版本  修改人 修改内容
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XGameEditor
{
    /// <summary>
    /// 资源遍历工具
    /// </summary>
    public class FileBrowser
    {
        public delegate void FileCallBack(FileSystemInfo file);
        public delegate void ProgressCallBack(FileSystemInfo file, int curIndex, int count);

        FileCallBack File_CallBack;
        ProgressCallBack direCallBack;

        public FileBrowser(string path, FileCallBack CallBack, ProgressCallBack direCallBack = null)
        {
            DirectoryInfo dire = new DirectoryInfo(path);
            this.File_CallBack = CallBack;
            this.direCallBack = direCallBack;
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

        void ReadFile(FileSystemInfo file)
        {
            DirectoryInfo dire = file as DirectoryInfo;

            if (dire != null && dire.Exists)
            {
                FileSystemInfo[] fileArr = dire.GetFileSystemInfos();
                if (fileArr != null)
                {
                    int count = fileArr.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (direCallBack != null)
                            direCallBack(file, i, count);
                        ReadFile(fileArr[i]);
                    }
                }

            }
            else
            {
                if (File_CallBack != null)
                {
                    File_CallBack(file);
                }
            }
        }
    }

}
/*******************************************************************
** 文件名:	FileUtil.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:	许德纪
** 日  期:	9/27/2019
** 版  本:	1.0
** 描  述:	
** 应  用:  

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using XGame;
using ICSharpCode.SharpZipLib.Zip;

public class UpdateFileUtil
{
    //[MenuItem("MyTools/创建文件信息")]
    public static void CreateFileInfo(string dir,string startVersion = "",bool bCanHotUpdate = true, bool isNeedRestart = false)
    {
     
        List<CSVFileInfo> infos = new List<CSVFileInfo>();
        //string path = Application.streamingAssetsPath + "/Data";
        string path = dir;
        string[] files = Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Contains("meta") || !files[i].Contains(".zip"))
                continue;
            FileInfo fileInfo = new FileInfo(files[i]);
            float size = fileInfo.Length / 1024.0f;
            files[i] = FileSlash(files[i]);
            string[] strs = files[i].Split('/');
            string fileName = strs[strs.Length - 1];
            string md5 = GetFileMD5(files[i]);
            CSVFileInfo cSVFileInfo = new CSVFileInfo();
            if(files[i].Contains("dll") || files[i].Contains("AllAndroidPatchFiles"))
            {
                cSVFileInfo.m_isDll = true;
            }
            else
            {
                cSVFileInfo.m_isDll = false;
            }            
            cSVFileInfo.m_md5 = md5;
            cSVFileInfo.m_size = size;
            cSVFileInfo.m_fileName = fileName;
            cSVFileInfo.m_isHot = bCanHotUpdate;
            cSVFileInfo.m_isNeedRestart = isNeedRestart;
            infos.Add(cSVFileInfo);
        }
        string fileCSV = path + "/"+ UpdateConfig.UPDATE_FILE;
        if(File.Exists(fileCSV))
        {
            File.Delete(fileCSV);            
        }
        FileStream fs = new FileStream(fileCSV, FileMode.Create, FileAccess.Write);
        using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
        {
            string str = "文件名,文件大小（单位KB）,MD5值,是否是DLL(0:否，1：是),起始版本,是否能热更";
            sw.WriteLine(str);
            str = "string,int,string,int,int";
            sw.WriteLine(str);


            for (int i = 0; i < infos.Count; i++)
            {
                CSVFileInfo info = infos[i];
                int isDll = info.m_isDll == false ? 0 : 1;
                int hotUpdate = ((info.m_isHot == false) ? 0 : 1);
                int needRestart = ((info.m_isNeedRestart == false) ? 0 : 1);
                str = info.m_fileName + "," + info.m_size + "," + info.m_md5 + "," + isDll+","+ startVersion+","+ hotUpdate+","+ needRestart;
                sw.WriteLine(str);
            }       
            

            //写一行记录,知名能否热更新
            if(infos.Count==0)
            {
                str = "," + "0," + "0," + "0," + "0," + (bCanHotUpdate?1:0)+","+(isNeedRestart ? 1 : 0);
                sw.WriteLine(str);
            }
        }
        fs.Close();
        fs.Dispose();
        Debug.Log("更新文件生产.....");

        //外网经常有拉取失败的案例，这里加一个长度作为校验
        if(UpdateConfig.checkVersionInfo)
            WriteLenChcekFile(fileCSV);

        //UnityEditor.EditorUtility.DisplayDialog("提示", "更新文件生产", "确定");
    }

    //排序函数
    public static int ComparisonCSVInfo(CSVVersionInfo a, CSVVersionInfo b)
    {
        if(a.m_NO == b.m_NO)
        {
            return 0;
        }
        return a.m_NO > b.m_NO?1:-1;
    }



    //[MenuItem("MyTools/创建版本信息")]
    public static void CreateVerInfo(string dir)
    {
        Dictionary<ulong, CSVVersionInfo> dicFixInfo = new Dictionary<ulong, CSVVersionInfo>();
        //string path = Application.streamingAssetsPath + "/Ver";
        string path = dir;
        string fileCSV = path + "/"+ UpdateConfig.UPDATE_VERSION;

        if(File.Exists(fileCSV))
        {
            File.Delete(fileCSV);
        }

        FileStream fs = new FileStream(fileCSV, FileMode.OpenOrCreate, FileAccess.Write);
        List<CSVVersionInfo> datas = new List<CSVVersionInfo>();
        using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
        {
            string str = "版本号,当前版本的资源url,当前版本的资源大小(单位KB),是否能热更(0:否，1：是),热更后是否需要重启(0:否，1：是),v7a大小,v8a大小,固件大小,热更起始版本,版本说明";
            sw.WriteLine(str);
            str = "int,string,int,int,int,int,int,int,string";
            sw.WriteLine(str);
            //DirectoryInfo root = new DirectoryInfo(path);
            //DirectoryInfo[] dics = root.GetDirectories();

            string[] files = Directory.GetFiles(dir, UpdateConfig.UPDATE_FILE, SearchOption.AllDirectories);
            int nCount = files.Length;

            for (int i = 0; i < nCount; i++)
            {
                string fileName = files[i].Replace('\\','/');
                int nPose = fileName.IndexOf(UpdateConfig.UPDATE_FILE);
                if(nPose<=0)
                {
                    continue;
                }

                float m_fixed_size = 0;
                nPose = fileName.IndexOf("/ver_");
                if (nPose <= 0)
                {
                    //看看是否是固件,计算固件大小
                    nPose = fileName.IndexOf("/fix_");
                    if(nPose>=0)
                    {
                        CSVVersionInfo fixInfo = GetCSVVersionInfo(fileName);
                        string[] strs2 = fileName.Split('/');
                        strs2 = strs2[strs2.Length - 2].Split('_');
                        fixInfo.m_NO = ulong.Parse(strs2[1]);
                        m_fixed_size = fixInfo.m_size;
                        if (dicFixInfo.ContainsKey(fixInfo.m_NO) ==false)
                        {
                            dicFixInfo.Add(fixInfo.m_NO, fixInfo);
                        }
                    }
                   

                    
                    continue;
                }


                Debug.Log("fileName:" + fileName);
                CSVVersionInfo verInfo = GetCSVVersionInfo(fileName);
                string[] strs = fileName.Split('/');
                strs = strs[strs.Length - 2].Split('_');
                verInfo.m_NO = ulong.Parse(strs[1]);
                verInfo.m_fixed_size = m_fixed_size;
                datas.Add(verInfo);
            }

            datas.Sort(ComparisonCSVInfo);
            for (int i = 0; i < datas.Count; i++)
            {
                //同步固件大小
                if(dicFixInfo.ContainsKey(datas[i].m_NO))
                {
                    datas[i].m_fixed_size = dicFixInfo[datas[i].m_NO].m_fixed_size;
                }

                int isHot = datas[i].m_isHot == false ? 0 : 1;
                int isNeedRestart = datas[i].m_isNeedRestart == false ? 0 : 1;
                str = datas[i].m_NO + ",url" + "," + datas[i].m_size + "," + isHot + "," + isNeedRestart+","+ datas[i].m_v7a_size + "," + datas[i].m_v8a_size + "," + datas[i].m_fixed_size+ ","+ datas[i].m_startHotVersion+",";
                sw.WriteLine(str);
            }
        }
        fs.Close();
        fs.Dispose();


        //外网经常有拉取失败的案例，这里加一个长度作为校验
        if (UpdateConfig.checkVersionInfo)
            WriteLenChcekFile(fileCSV);
       
        


        //string fileContent = File.ReadAllText(fileCSV);

        Debug.Log("版本文件创建成功...");




        //UnityEditor.EditorUtility.DisplayDialog("提示", "版本文件创建成功", "确定");
    }


    /// <summary>
    /// 获取本地文件的md5值
    /// </summary>
    /// <returns></returns>
    public static string GetFileMD5(string fileName)
    {
        try
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();
            StringBuilder sb = new StringBuilder();
            foreach (var item in retVal)
            {
                sb.Append(item.ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception e)
        {
            throw new Exception("GetFileMD5 fail,error:" + e);
        }
    }


    /// <summary>
    /// 斜线和反斜线的处理
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string FileSlash(string path)
    {
        string tempPath = null;
        tempPath = path.Replace('\\', '/');        
        return tempPath;
    }


    public static CSVVersionInfo GetCSVVersionInfo(string fileName)
    {
        CSVVersionInfo cSVVersionInfo = new CSVVersionInfo();
        byte[] bytes = File.ReadAllBytes(fileName);
        if (UpdateConfig.checkVersionInfo)
        {
            int content_size = bytes.Length-4;    
            byte[] newBytes = new byte[content_size];
            Array.Copy(bytes,4, newBytes,0, content_size);
            bytes = newBytes; 
        }



            List<CSVFileInfo> datas = new List<CSVFileInfo>();
        try
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (StreamReader sr = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    int index = 0;
                    string dataLine;
                    while (true)
                    {
                        index++;
                        dataLine = sr.ReadLine();
                        if (dataLine == null)
                        {
                            break;
                        }
                        if (index > 2)
                        {
                            CSVFileInfo info = new CSVFileInfo();
                            string[] strs = dataLine.Split(',');
                            info.m_fileName = strs[0];
                            info.m_size = float.Parse(strs[1]);
                            info.m_md5 = strs[2];
                            int isDll = int.Parse(strs[3]);
                            info.m_isDll = isDll == 0 ? false : true;
                            if(strs.Length>4&& string.IsNullOrEmpty(strs[4])==false)
                            {
                                info.m_startHotVersion = ulong.Parse(strs[4]);
                            }

                            if (strs.Length > 5 && string.IsNullOrEmpty(strs[5]) == false)
                            {
                                info.m_isHot = int.Parse(strs[5]) > 0;
                            }
                            if (strs.Length > 6 && string.IsNullOrEmpty(strs[6]) == false)
                            {
                                info.m_isNeedRestart = int.Parse(strs[6]) > 0;
                            }


                            datas.Add(info);
                        }
                    }
                }
            }            
        }
        catch (Exception e)
        {
            Debug.LogError("解析csv错误..." + e.ToString()+ "fileName="+ fileName);
            //UpdateEngine.Instance.UpdateError("解析csv错误..." + e.ToString());
            return null;
        }
        float size = 0f;
        float v7aSize = 0f;
        float v8aSize = 0f;
        cSVVersionInfo.m_isHot = true;
        cSVVersionInfo.m_isNeedRestart = false;
        ulong startVer = 0;
        bool isHot = true;
        bool isNeedRestart = false; 

        for (int i = 0; i < datas.Count; i++)
        {
            CSVFileInfo info = datas[i];
            if(info.m_isDll)
            {
                if (info.m_fileName.Contains("v7a"))
                {
                    v7aSize = info.m_size;
                }
                else if (info.m_fileName.Contains("v8a"))
                {
                    v8aSize = info.m_size;
                }
            }
            size += info.m_size;

            if(info.m_startHotVersion> startVer)
            {
                startVer = info.m_startHotVersion;
            }

            isHot = info.m_isHot;
            isNeedRestart = info.m_isNeedRestart;
        }
        

        cSVVersionInfo.m_size = size;
        cSVVersionInfo.m_v7a_size = size - v8aSize;
        cSVVersionInfo.m_v8a_size = size - v7aSize;
        cSVVersionInfo.m_fixed_size = size - v8aSize - v7aSize;
        cSVVersionInfo.m_startHotVersion = startVer;
        cSVVersionInfo.m_isHot = isHot;
        cSVVersionInfo.m_isNeedRestart = isNeedRestart;
        return cSVVersionInfo;
    }




    public static void CreateVerZipInfo(string src,string dst, float zipSize)
    {
        Debug.Log("压缩大小:" + (zipSize / 1024));
        string tempPaht = src + "/tempVer";

        CopyFile(src, tempPaht, zipSize);
        CreateZip(tempPaht, dst);
        DeleteDir(tempPaht);
        Debug.Log("压缩成功.....");
        AssetDatabase.Refresh();
    }


    public static void CopyFile(string path, string targetPath1, float size)
    {
        string m_path = path;
        //string path1 = Application.streamingAssetsPath + "/ui.zip";
        string targetPath = targetPath1;//path + "/" + tempVer;
        float m_size = size;
        string[] files = Directory.GetFiles(m_path,"*",SearchOption.AllDirectories);
        int count = 0;
        float size1 = 0;
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Contains(".meta"))
            {
                continue;
            }
            FileInfo fileInfo = new FileInfo(files[i]);
            float tempSize = fileInfo.Length / 1024.0f;//PrintFileVersionInfo(files[i]);
            size1 += tempSize;
            if (size1 <= m_size)
            {
                CopyFile2(files[i], targetPath + "/" + count, m_path);
            }
            else
            {
                size1 = tempSize;
                string tempPath = targetPath + "/" + count;
                if (Directory.Exists(tempPath))
                {
                    count++;
                    tempPath = targetPath + "/" + count;
                    CopyFile2(files[i], tempPath, m_path);
                }
                else
                {
                    CopyFile2(files[i], tempPath,m_path);
                    count++;
                }

            }
        }
    }

    static void CopyFile2(string filePath, string path,string rootPath)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        //string rootPath = Path.GetPathRoot(filePath);
        string tempRootPath = Directory.GetParent(filePath).FullName;
        tempRootPath = FileSlash(tempRootPath);
        filePath = FileSlash(filePath);
        rootPath = FileSlash(rootPath);
        string prevPath = "";
        if(tempRootPath != rootPath)
        {
            int length = rootPath.Length;
            prevPath = tempRootPath.Substring(length);
            string dir = path +"/"+ prevPath;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        string fileName = Path.GetFileName(filePath);
        File.Copy(filePath, path + "/" + prevPath + "/" + fileName);
    }

    public static void CreateZip(string sourcePath, string targetPath1)
    {
        //CopyTest();
        string targetPath = sourcePath;//@"F:\CopyTest\Res\tempVer";
        string targetPath2 = targetPath1;//@"F:\CopyTest\Res\Ver";
        if (Directory.Exists(targetPath2))
        {
            DeleteDir(targetPath2);
        }
        Directory.CreateDirectory(targetPath2);
        string[] str = Directory.GetDirectories(targetPath);
        for (int i = 0; i < str.Length; i++)
        {
            string path = str[i];
            string[] names = path.Split('\\');
            string name = targetPath2 + "\\" + names[names.Length - 1] + ".zip";
            CreateZipFile(path, name);
            Debug.Log("name:" + name);
        }
        //AssetDatabase.Refresh();
    }

    /// <summary>
    /// 压缩所有的文件
    /// </summary>
    /// <param name="filesPath"></param>
    /// <param name="zipFilePath"></param>
    public static void CreateZipFile(string filesPath, string zipFilePath)
    {
        if (!Directory.Exists(filesPath)) 
        {
            return;
        }
        ZipOutputStream stream = new ZipOutputStream(File.Create(zipFilePath));
        stream.SetLevel(9); // 压缩级别 0-9
        byte[] buffer = new byte[4096]; //缓冲区大小
        string[] filenames = Directory.GetFiles(filesPath, "*.*", SearchOption.AllDirectories);
        foreach (string file in filenames)
        {
            ZipEntry entry = new ZipEntry(file.Replace(filesPath, ""));
            entry.DateTime = DateTime.Now;
            stream.PutNextEntry(entry);
            using (FileStream fs = File.OpenRead(file))
            {
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    stream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }
        stream.Finish();
        stream.Close();
    }

    public static void DeleteDir(string file)
    {

        try
        {

            //去除文件夹和子文件的只读属性
            //去除文件夹的只读属性
            System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
            fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

            //去除文件的只读属性
            System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

            //判断文件夹是否还存在
            if (Directory.Exists(file))
            {

                foreach (string f in Directory.GetFileSystemEntries(file))
                {

                    if (File.Exists(f))
                    {
                        //如果有子文件删除文件
                        File.Delete(f);
                        Console.WriteLine(f);
                    }
                    else
                    {
                        //循环递归删除子文件夹
                        DeleteDir(f);
                    }

                }

                //删除空文件夹

                Directory.Delete(file);

            }

        }
        catch (Exception ex) // 异常处理
        {
            Console.WriteLine(ex.Message.ToString());// 异常信息
        }

    }

    public static void WriteLenChcekFile(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        int size = fileData.Length;
        byte[] lenData = CSVVersionInfoHelper.intToBytes(size);
        byte[] data = new byte[size + lenData.Length];
        Array.Copy(lenData, data, lenData.Length);
        Array.Copy(fileData,0, data, lenData.Length,size);
        if(File.Exists(path))
        {
            File.Delete(path);
        }
        File.WriteAllBytes(path, data);

    }

    //跳过长度校验选项，重新生成版本文件
    public static void RewriteLenChcekFile(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        int size = fileData.Length;
        byte[] lenData = CSVVersionInfoHelper.intToBytes(size);
        int contentLen = size - lenData.Length;
        if(contentLen<=0)
        {
            Debug.LogError("文件内容不合法，长度太短了 path="+ path);
            return;
        }
        byte[] data = new byte[contentLen];
        Array.Copy(fileData, lenData.Length, data, 0, contentLen);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.WriteAllBytes(path, data);

        //再对原始数据加密
        WriteLenChcekFile(path);

    }

    //跳过长度校验选项，重新生成版本文件
    public static void RewriteForeceUpdateVersionInfo(string path)
    {
        
        string[] allContents = File.ReadAllLines(path);
        if(allContents.Length>1)
        {
            string newVersionItem = allContents[allContents.Length - 1];
            string[] splits = newVersionItem.Split(",");
            if(splits.Length>4)
            {
                ulong version = ulong.Parse(splits[0]);
                ++version;
                splits[0] = version.ToString();
                splits[3] = "0";
                newVersionItem = string.Join(",", splits);//splits[0]+ newVersionItem.Substring(newVersionItem.IndexOf(","));
                string[] newContents = new string[allContents.Length + 1];
                Array.Copy(allContents, 0, newContents, 0, allContents.Length);

                newContents[allContents.Length] = newVersionItem;
                File.WriteAllLines(path,newContents);
            }
        }


        RewriteLenChcekFile(path);



    }

    //强行生成热更的版本号
    public static ulong RewriteHotUpdateVersionInfo(string path)
    {
        ulong version = 0;
        string[] allContents = File.ReadAllLines(path);
        if (allContents.Length > 1)
        {
            string newVersionItem = allContents[allContents.Length - 1];
            string[] splits = newVersionItem.Split(",");
            if (splits.Length > 4)
            {
                 version = ulong.Parse(splits[0]);
                splits[3] = "1";
                newVersionItem = string.Join(",", splits);//splits[0]+ newVersionItem.Substring(newVersionItem.IndexOf(","));

                allContents[allContents.Length-1] = newVersionItem;
                File.WriteAllLines(path, allContents);
            }
        }


        RewriteLenChcekFile(path);

        return version;

    }
}

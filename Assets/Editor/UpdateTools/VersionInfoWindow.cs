/*******************************************************************
** 文件名:	VersionInfoWindow.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:	郭潭宝
** 日  期:	10/9/2019
** 版  本:	1.0
** 描  述:	
** 应  用:  版本生成工具面板

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using UnityEditor;
using System.IO;
using XGame;
using System.Collections.Generic;
using XGame.Asset.Load;

namespace XGameEditor
{
    public class VersionInfoWindow : EditorWindow
    {

        static public string fileInfoPath = @"D:\HotUpdate\data\ver_xxxx";
        static public string versionInfoPath = @"D:\HotUpdate\data";
        static public string zipInfoPath = "Assets";
        static public string fileName = "";
        static public float zipSize = 0;
        static public int SIZE = 1024;
        static public int inputSize = 10;

        static string[] soFilters = { "libunity.so", "libmain.so" };

        //[MenuItem("XGame/版本生成工具", false, 10)]
        static void ShowWindow()
        {
            EditorWindow window = GetWindow<VersionInfoWindow>();
            window.Show();
        }

        public static void Draw()
        {
            GUILayout.BeginHorizontal();
            fileInfoPath = EditorGUILayout.TextField("Folder Path :", fileInfoPath);
            if (GUILayout.Button("选择资源目录"))
                fileInfoPath = EditorUtility.OpenFolderPanel("选择资源目录", fileInfoPath, "");
            if (GUILayout.Button("生成热更文件信息"))
            {
                UpdateFileUtil.CreateFileInfo(fileInfoPath);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            versionInfoPath = EditorGUILayout.TextField("Folder Path :", versionInfoPath);
            if (GUILayout.Button("选择版本目录"))
                versionInfoPath = EditorUtility.OpenFolderPanel("选择版本目录", versionInfoPath, "");
            if (GUILayout.Button("生成热更版本文件信息"))
            {
                UpdateFileUtil.CreateVerInfo(versionInfoPath);
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            zipInfoPath = EditorGUILayout.TextField("Folder Path :", zipInfoPath);
            if (GUILayout.Button("选择压缩目录"))
                zipInfoPath = EditorUtility.OpenFolderPanel("选择压缩目录", zipInfoPath, "");

            inputSize = EditorGUILayout.IntField("单个压缩文件大小(MB) :", inputSize);

            if (GUILayout.Button("生成压缩文件信息"))
            {
                float tempSize2 = inputSize * SIZE;
                UpdateFileUtil.CreateVerZipInfo(zipInfoPath, zipInfoPath+"/ver", tempSize2);
            }
            GUILayout.EndHorizontal();

        }


        static public void BuildPublishPackage(string dir,string version,bool bCopy2Publish,string publishWebDir,bool isCheckCodeModified,bool bCanUpdateSo,bool supportFix,bool isNeedRestart)
        {
            Debug.LogError("代码变化情况: codeModified = " + isCheckCodeModified+ ",CanUpdateSo = "+ bCanUpdateSo);

            string publishDir = dir + "/"+ UpdateConfig.EDITOR_PUBLISH_DIR;

            //创建发布资源目录
            if (Directory.Exists(publishDir)==false)
            {
                Directory.CreateDirectory(publishDir);
            }

            string hotUpdateDst = publishDir + "/ver_"+ version;
            string fixUpdateDst = publishDir + "/fix_" + version;

            if (Directory.Exists(hotUpdateDst))
            {
                Directory.Delete(hotUpdateDst, true);
            }
            Directory.CreateDirectory(hotUpdateDst);

            if (Directory.Exists(fixUpdateDst))
            {
                Directory.Delete(fixUpdateDst, true);
            }

            //需要支持固件的
            if(supportFix)
            {
                Directory.CreateDirectory(fixUpdateDst);
            }


            

            dir = Directory.GetParent(publishDir).FullName;  //去掉..
            string hotUpdateSrc = dir + "/"+ UpdateConfig.EDITOR_HOT_UPDATE_DIR;
            string fixUpdateSrc = dir + "/fix";

          
            bool bGenFileInfo = true;
            bool bCanHotUpdate = true;

            //处理热更新目录
            if (Directory.Exists(hotUpdateSrc))
            {
                //压缩热更文件
                UpdateFileUtil.CreateVerZipInfo(hotUpdateSrc, hotUpdateDst, inputSize * SIZE);
                bGenFileInfo = true;
            }


            //代码有变更了，但是又不能热更so，那就只能重新安装了
            if(bCanUpdateSo==false&&true== isCheckCodeModified)
            {
                bCanHotUpdate = false;
            }


            Debug.LogError("当前是否能热更 CanHotUpdate = " + bCanHotUpdate);




            //生成热更资源列表
            if (bGenFileInfo)
            {
                UpdateFileUtil.CreateFileInfo(hotUpdateDst,"", bCanHotUpdate, isNeedRestart);
            }


            //处理固件目录
            if (Directory.Exists(fixUpdateSrc))
            {
                UpdateFileUtil.CreateVerZipInfo(fixUpdateSrc, fixUpdateDst, inputSize * SIZE);
                UpdateFileUtil.CreateFileInfo(fixUpdateDst,"", bCanHotUpdate, isNeedRestart);
            }

            //生成版本信息
            string Versiondir = Directory.GetParent(dir).FullName;
            UpdateFileUtil.CreateVerInfo(Versiondir);

            //拷贝versioninfo到发布目录
            string fileCSV = Versiondir + "/" + UpdateConfig.UPDATE_VERSION;
            if(File.Exists(fileCSV))
            {
                FileUtil.ReplaceFile(fileCSV, publishDir + "/" + UpdateConfig.UPDATE_VERSION);
            }

         
            //拷贝发布资源到httpweb服务器
            if(bCopy2Publish)
            {
                CopyDirFiles(publishDir, publishWebDir, SearchOption.AllDirectories);
                //FileUtil.ReplaceDirectory(publishDir, publishWebDir);
            }

            //打开资源发布目录
            EditorUtility.RevealInFinder(publishDir + "/" + UpdateConfig.UPDATE_VERSION);


        }


        public static void MergeHotPackage(string dir, string startVersion,string curVersion, bool bCopy2Publish, string publishWebDir)
        {
            dir = Path.GetFullPath(dir);

            //找到目标的热更资源
            ulong nLastVerSion = ulong.Parse(startVersion);
            ulong nCurVersion = ulong.Parse(curVersion);
            List<string> listVersionDir = new List<string>();
            string[]  dirs = Directory.GetDirectories(dir);
            int nLen = dirs.Length;
            string path = null;
            string subVersion = null;
            for (int i=0;i<nLen;++i)
            {
                path = dirs[i];
                int nPos = path.LastIndexOf("pkg_");
                if(nPos>=0)
                {
                    subVersion = path.Substring(nPos+4);
                    ulong ver = ulong.Parse(subVersion);
                    if(ver> nLastVerSion&&ver<= nCurVersion)
                    {
                        path = path + "/" + UpdateConfig.EDITOR_HOT_UPDATE_DIR;
                        if(Directory.Exists(path))
                        {
                            listVersionDir.Add(path);
                        }
                        
                    }
                }

            }
            listVersionDir.Sort();

            //目标合并的路径
            string mergePath = dir  + "/pkg_"+ curVersion + "/merge";
            if(Directory.Exists(mergePath))
            {
                Directory.Delete(mergePath,true);
            }
            Directory.CreateDirectory(mergePath);

            //拷贝资源到目标文件夹
            nLen = listVersionDir.Count;
            for(int i=0;i<nLen;++i)
            {
                path = listVersionDir[i];
                CopyDirFiles(path, mergePath, SearchOption.AllDirectories);
            }


            string publishDir = dir +"/pkg_" + curVersion+"/" + UpdateConfig.EDITOR_PUBLISH_DIR;

            //创建发布资源目录
            if (Directory.Exists(publishDir) == false)
            {
                Directory.CreateDirectory(publishDir);
            }

            string hotUpdateDst = publishDir + "/ver_" + curVersion;
            
            if (Directory.Exists(hotUpdateDst))
            {
                Directory.Delete(hotUpdateDst, true);
            }
            Directory.CreateDirectory(hotUpdateDst);

            //压缩热更文件
            UpdateFileUtil.CreateVerZipInfo(mergePath, hotUpdateDst, inputSize * SIZE);
            UpdateFileUtil.CreateFileInfo(hotUpdateDst, startVersion, true);

            //生成版本信息
            string Versiondir = Directory.GetParent(dir).FullName;
            UpdateFileUtil.CreateVerInfo(Versiondir);

            //拷贝versioninfo到发布目录
            string fileCSV = Versiondir + "/" + UpdateConfig.UPDATE_VERSION;
            if (File.Exists(fileCSV))
            {
                FileUtil.ReplaceFile(fileCSV, publishDir + "/" + UpdateConfig.UPDATE_VERSION);
            }


            //拷贝发布资源到httpweb服务器
            if (bCopy2Publish)
            {
                CopyDirFiles(publishDir, publishWebDir, SearchOption.AllDirectories);
                // CopyDirFiles(hotUpdateDst, publishWebDir+ "/ver_" + curVersion, SearchOption.AllDirectories);
                // FileUtil.ReplaceFile(fileCSV, publishWebDir + "/" + UpdateConfig.UPDATE_VERSION);
            }

            //打开资源发布目录
            EditorUtility.RevealInFinder(publishDir + "/" + UpdateConfig.UPDATE_VERSION);

        }

       



        static void CopyDirFiles(string srcDir,string dstDir, SearchOption sp = SearchOption.TopDirectoryOnly)
        {
            if(Directory.Exists(srcDir)==false)
            {
                Debug.LogError("源文件夹不存在：" + srcDir);
                return;
            }

            if(Directory.Exists(dstDir)==false)
            {
                Directory.CreateDirectory(dstDir);
            }

            string[] filePaths = Directory.GetFiles(srcDir, "*.*", sp);
            int nCount = filePaths.Length;
            int nRootPos = srcDir.Length;
           
            for(int i =0;i<nCount;++i)
            {
                string dstPath = dstDir + "/"+ filePaths[i].Substring(nRootPos);

                string parantName = Directory.GetParent(dstPath).FullName;
                if(Directory.Exists(parantName)==false)
                {
                    Directory.CreateDirectory(parantName);
                }

                if(File.Exists(dstPath))
                {
                    File.Delete(dstPath);
                }

                FileUtil.ReplaceFile(filePaths[i], dstPath);
            }
        }

        public void OnGUI()
        {
            Draw();
        }
    }
}
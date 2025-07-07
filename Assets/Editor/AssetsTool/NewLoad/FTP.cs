using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using XGame.Asset.Manager;
using XGame.I18N;
using XGameEditor.AssetImportTool;
using XGameEditor.NewLoad;
using XGame;
using XGame.Asset.Load;
#if SUPPORT_I18N
using XGameEditor.I18N;
#endif
using XGameEditor;
using static XGameEditor.NewGAssetDataCache;



namespace FTPNetwork
{
    public class FTP
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>

        
        static public void Uploads()
        {
            string urls = @"\\192.168.2.33\download\";
            string userNames = "SZHYL";
            string passwords = "123456";
            string filePaths = $"{BuildAPPWindow.GenVersionPath}/publish";
            UploadFolder(urls,userNames,passwords,filePaths);
        }

        static public void cpyfiles()
        {
            string str2 = System.IO.Directory.GetCurrentDirectory();
            string strpaths = str2 + "/Temp/StagingArea/symbols/";
            string filePaths = $"{BuildAPPWindow.GenVersionPath}/symbols";
            CopyDir(strpaths, filePaths);
            UploadSymbolv();//上传符号表

        }
        static private bool UploadFile(string url, string userName, string password, string filePath)
        {
            Debug.Log($"上传文件：url:{url}, userName:{userName}, password:{password}, filePath:{filePath}");
            bool isSuccess = true;
            
            if (!File.Exists(filePath))
            {
                Debug.Log($"不存在文件：{filePath}");
                return false;
            }
            try
            {
                //_waitForDeleteFile = filePath;
                WebClient client = new System.Net.WebClient();
                var finalURL = $"{url}/{Path.GetFileNameWithoutExtension(filePath) + Path.GetExtension(filePath)}";
                Debug.Log($"上传文件：{finalURL}, file:{filePath}");
                _startUploadTime = DateTime.Now;
                Uri uri = new Uri(finalURL);
                client.UploadProgressChanged += new UploadProgressChangedEventHandler(OnFileUploadProgressChanged);
                client.UploadFileCompleted += new UploadFileCompletedEventHandler(OnFileUploadCompleted);
                client.Credentials = new System.Net.NetworkCredential(userName, password);
                client.UploadFileAsync(uri, filePath);

            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                isSuccess = false;
            }
            return isSuccess;
        }

        static private DateTime _startUploadTime;
        const float MaxUploadTime = 600f;
        static private WebClient _upLoadClient;
        /// <summary>
        /// 上传文件
        /// </summary>
        /// 
      
        static private bool UploadFolder(string url, string userName, string password, string folderPath)
        {
            Debug.Log($"上传文件目录：url:{url}, userName:{userName}, password:{password}, filePath:{folderPath}");
            bool isSuccess = true;
            var finURL = "";
            var finalURL = "";
            int folderleght = folderPath.Length;
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            try
            {
                //var targetZIP = $"{dirInfo.Parent}/{dirInfo.Name}.zip";
                //Debug.Log($"压缩目录：filePath:{folderPath}, zip:{targetZIP}");
                //if (File.Exists(targetZIP))
                //{
                //    File.Delete(targetZIP);
                //}

                //if (!ZipHelper.ZipDirectory($"{folderPath}", $"{targetZIP}"))
                //{
                //    Debug.Log($"压缩目录失败：filePath:{folderPath}, zip:{targetZIP}");
                //    return false;
                //}

                string[] FilePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < FilePaths.Length; i++)
                {
                    string file = FilePaths[i];
                    string filename = Path.GetFullPath(file);
                    string filenames = Path.GetFileName(file);
                    string filefolder = Path.GetDirectoryName(file);
                    DirectoryInfo dirInfos = new DirectoryInfo(filefolder);
                     finalURL = $"{url}/{dirInfos.Name}";
                    int flenght = filefolder.Length;
                    // dstName = curDataDir + "/" + file.Name;
                    _upLoadClient = new System.Net.WebClient();
                    if (flenght > folderleght)
                    {
                        finURL = $"{url}/{dirInfos.Name}/{filenames}";
                        finalURL = $"{url}/{dirInfos.Name}";
                        if (!Directory.Exists(finalURL))
                        {
                            Directory.CreateDirectory(finalURL);
                        }
                    }
                    else 
                    {
                        finURL = $"{url}/{filenames}";
                        finalURL = $"{url}/{dirInfos.Name}";
                    }
                    _startUploadTime = DateTime.Now;
                    Debug.Log($"上传文件：{finURL},  {filename}");
                    Uri uri = new Uri(finURL);
                    _upLoadClient.UploadProgressChanged += new UploadProgressChangedEventHandler(OnFileUploadProgressChanged);
                    _upLoadClient.UploadFileCompleted += new UploadFileCompletedEventHandler(OnFileUploadCompleted);
                    _upLoadClient.Credentials = new System.Net.NetworkCredential(userName, password);
                    _upLoadClient.UploadFileAsync(uri, filename);      
                }

            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                isSuccess = false;

            }
            return isSuccess;
        }

        static private void OnFileUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            Debug.Log("传输进度");
        }

        static private void OnFileUploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            Debug.Log("传输成功");
        }

        static void CopyDir(string srcPath, string aimPath)
        {
            try
            {
                //检查目标目录是否以目录分割字符结束，如果不是则添加
                if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                {
                    aimPath += Path.DirectorySeparatorChar;
                }
                //判断目标目录是否存在，如不存在则创建
                if (!Directory.Exists(aimPath))
                {
                    Directory.CreateDirectory(aimPath);
                }
                string[] filelist = Directory.GetFileSystemEntries(srcPath);
                foreach (string file in filelist)
                {
                    if (Directory.Exists(file))
                    {
                        CopyDir(file, aimPath + Path.GetFileName(file));
                    }
                    else
                    {
                        System.IO.File.Copy(file, aimPath + Path.GetFileName(file), true);
                    }
                }
                Debug.Log("kaobei:srcPath"+srcPath+ ",file:"+aimPath);
            }
            catch (Exception e)
            {
                throw;
            }
        }
        static void UploadSymbolv()
        {
            var bundleid = PlayerSettings.Android.bundleVersionCode;
            var version = Application.version;
            var paths = @"C:\X1002\X1002_Game\buglytools\buglyqq-upload-symbol\2_il2cpp-0.1-v1.symbols";
            string command =string.Format( @"java -jar C:\X1002\X1002_Game\buglytools\buglyqq-upload-symbol\buglyqq-upload-symbol.jar -appid 8db1a05b2a -appkey 0488787f-4997-426f-8b3a-35ffdce96cd3 -bundleid {0} -version {1} -platform Android -inputSymbol {2}",bundleid,version,paths);
            System.Diagnostics.Process.Start("CMD.exe", "/K " + command);
            Debug.Log("bundleid:"+bundleid+",version:"+version+",paths"+paths);
        }

        }
}



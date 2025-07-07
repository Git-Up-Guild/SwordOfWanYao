/*******************************************************************
** 文件名:	UpdateTest.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:	郭潭宝
** 日  期:	9/29/2019
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
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using XGame;
public class UpdateTest : MonoBehaviour  
{

    public bool isDelteFile = false;


    public UpdateClient updateClient;

    public Button btnStart;
    public Button btnRun;
    public Text text1;
    public Text text2;
    public Text text3;
    public Slider slider;
    public Button btnOpenUrl;

    public Button btnOK;
    public Text textTip;
    public GameObject tipPanle;

    public Text localVerText;
    public Text updateVerText;


    private string URL;
    private void Awake()
    {
        btnStart.onClick.AddListener(OnBtnStart);
        btnOK.onClick.AddListener(OnBtnRun);
        btnOpenUrl.onClick.AddListener(OnBtnOpenURL);
        btnOpenUrl.gameObject.BetterSetActive(false);
        slider.value = 0;
        tipPanle.gameObject.BetterSetActive(false);
        URL = "";
        if (isDelteFile)
        {
            DleteUpdateFile();
        }
       
    }


    // Start is called before the first frame update
    void Start()
    {
        //string path = Application.persistentDataPath + "/Android/Data";
        //string path2 = Application.streamingAssetsPath + "/Android/Data";
        string path = Application.persistentDataPath;
        string path2 = Application.streamingAssetsPath;
        string pathDll = Application.persistentDataPath + "/DLL";
       
        //更新系统初始化
        updateClient.Init(path, path2, pathDll, "Arm8");

        //下面是下载进度回调、解压进度回调，更新大小回调、更新错误回调，整包更新回调、更新结束回调设置

        updateClient.SetReinstallCallback((url, reason) => {
            UpdateDownload(url);
        });

        updateClient.SetDownLoadProgressCallback((complete, total) =>
        {
            DownloadCallback(complete, total);
        });

        updateClient.SetUnZipProgressCallback((complete, total) => {
            UnZipCallback(complete, total);
        });

        updateClient.SetUpdateFinishCallback((state, restart, msg, isUpdated) => {
            UpdateFinish(state, restart, msg);
        });

        updateClient.SetUpdateErrorCallback((msg,reason) => {
            UpdateError(msg);
        });

        updateClient.SetGetUpdateSizeCallback((size) =>
        {
            UpdateSize(size);
        });

        updateClient.SetDownLoadFileNameCallback((fileNames) =>
        {
            FileNameCallback(fileNames);
        });

        updateClient.SetShowVerinfoCallback((localVer, updateVer) => {
            ShowVerinfo(localVer, updateVer);
        });

        updateClient.StartUpdate();
    }


    private void OnBtnRun()
    {
        tipPanle.gameObject.BetterSetActive(false);
        //计算完更新大小后，开始下载资源
        updateClient.Run();
    }

    private void OnBtnStart()
    {
        //开始更新流程
        updateClient.StartUpdate();
    }

    /// <summary>
    /// 下载进度回调
    /// </summary>
    /// <param name="complete"></param>
    /// <param name="total"></param>
    private void DownloadCallback(float complete,float total)
    {       
        slider.value = complete / total;
        text1.text = slider.value * 100 + "%";
        text2.text = "下载进度:" + complete + "KB/" + total + "KB";
    }

    /// <summary>
    /// 解压进度回调
    /// </summary>
    /// <param name="complete"></param>
    /// <param name="total"></param>
    private void UnZipCallback(int complete,int total)
    {
        float value = complete * 1.0f / total;
        slider.value = value;
        text1.text = slider.value * 100 + "%";
        text2.text = "解压进度:" + complete + "/" + total + "";
    }

    /// <summary>
    /// 更新完成回调
    /// </summary>
    /// <param name="state"></param>
    /// <param name="restart"></param>
    /// <param name="msg"></param>
    private void UpdateFinish(bool state,bool restart,string msg)
    {
        Debug.Log("state:" + state + ",restart:" + restart + ",msg:" + msg);
        slider.gameObject.BetterSetActive(false);
        text1.gameObject.BetterSetActive(false);
        text2.gameObject.BetterSetActive(false);
        text3.gameObject.BetterSetActive(false);
        btnOK.gameObject.BetterSetActive(false);
        tipPanle.gameObject.BetterSetActive(true);
        textTip.text = "更新完毕...";

        //LoadLoginScence();
    }

    /// <summary>
    /// 加载登录场景
    /// </summary>
    public void LoadLoginScence()
    {
        Debug.LogWarning("切换Login场景。。。。");
        SceneManager.LoadScene("Login");//要切换到的场景名
    }
    
    /// <summary>
    /// 加载登录场景
    /// </summary>
    public void LoadIL2CPPUpdateScence()
    {
        Debug.LogWarning("切换IL2CPPUpdate场景。。。。");
        SceneManager.LoadScene("il2cppUpdate");//要切换到的场景名
    }


    /// <summary>
    /// 更新错误回调
    /// </summary>
    /// <param name="msg"></param>
    private void UpdateError(string msg)
    {
        Debug.Log("error:" + msg);
        tipPanle.gameObject.BetterSetActive(true);
        textTip.text = "本次更新出现错误：" + msg + ",需要下载安装包";
        btnOK.gameObject.BetterSetActive(false);
    }

    /// <summary>
    /// 获取更新大小回调
    /// </summary>
    /// <param name="size"></param>
    private void UpdateSize(float size)
    {
        text3.text = size + "KB";
        tipPanle.gameObject.BetterSetActive(true);
        textTip.text = "本次更新需要下载" + size + "KB，是否确认下载？";
    }

    private void FileNameCallback(List<string> fileNames)
    {
        
        /*
        List<UpdateMicroData> microUpdateList = new List<UpdateMicroData>();
        if (fileNames != null)
        {
            string fileName,fullPath;
            for (int i = 0; i < fileNames.Count; i++)
            {
                fullPath = fileNames[i].Replace("//", "/");
                fileName = Path.GetFileName(fileNames[i]);
                Debug.LogFormat("=====>>>HotUpdate: {0}  {1}", fileName, fullPath);
                microUpdateList.Add(new UpdateMicroData() { filefullPath = fullPath, filename = fileName });
            }
        }
        // 通知微端更新一下本地配置表
        MicroClientUtil.Instance.ModifyLocalFileIndex(microUpdateList);
        */
        
    }

    /// <summary>
    /// 整包更新回调
    /// </summary>
    /// <param name="url"></param>
    private void UpdateDownload(string url)
    {
        Debug.Log("url:" + url);
        tipPanle.gameObject.BetterSetActive(true);
        textTip.text = "本次更新需要下载安装包";
        btnOK.gameObject.BetterSetActive(false);
        URL = url;
        btnOpenUrl.gameObject.BetterSetActive(true);
    }

    /// <summary>
    /// 版本信息回调
    /// </summary>
    /// <param name="localVer"></param>
    /// <param name="updateVer"></param>
    private void ShowVerinfo(string localVer,string updateVer)
    {
        localVerText.text = "当前版本号:" + localVer;
        updateVerText.text = "更新版本号:" + updateVer;
    }

    private void OnBtnOpenURL()
    {
        if(!string.IsNullOrEmpty(URL))
        {
            Application.OpenURL(URL);
        }
    }


    /// <summary>
    /// 测试使用，删除下载的文件
    /// </summary>
    private void DleteUpdateFile()
    {
        string path = Application.persistentDataPath + "/Data";
        string pathDll = Application.persistentDataPath + "/DLL";
        string fileName = Application.persistentDataPath + "/updateConfig.xml";
        DeleteDir(path);
        DeleteDir(pathDll);
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    private void DeleteDir(string file)
    {
        try
        {

            

            //判断文件夹是否还存在
            if (Directory.Exists(file))
            {

                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

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
            Debug.LogError(ex.Message.ToString());// 异常信息
        }
    }


}

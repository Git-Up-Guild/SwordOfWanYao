/*******************************************************************
** 文件名:	IUpdateEngine.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	李涛 
** 日  期:	2017/3/10
** 版  本:	1.0
** 描  述:	
** 应  用:  更新引擎

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using XGame.EventEngine;

namespace XClient.Common
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////
    /**
    @name     : 更新引擎 接口
    */
    ///////////////////////////////////////////////////////////////////////////////////////////////////
    public interface IUpdateEngine
    {
        /// <summary>
        ///  请求开始分块下载
        /// </summary>
        /// <returns></returns>
        bool RequestBlockDownload(string szFileName);

        /// <summary>
        /// 请求停止分块下载
        /// </summary>
        /// <returns></returns>
        bool StopBlockDownload();

        /// <summary>
        ///  获取当前还需下载的字节数（返回格式：10M）
        ///  这里的还需下载的字节数是指，GetBlockDownloadFinish中还未完成的
        /// </summary>
        /// <returns></returns>
        string GetNeedDownloadSize();

        /// <summary>
        /// 获取当前已下载的字节数（返回格式：10M）
        /// 这里的已下载的字节数是指，GetBlockDownloadFinish中还未完成的
        /// </summary>
        /// <returns></returns>
        string GetFinishDownloadSize();

        /// <summary>
        /// 分块下载当前的完成进展百分比（取值范围：0~100）
        /// 这里的完成进度是指，GetBlockDownloadFinish中还未完成的
        /// </summary>
        /// <returns></returns>
        int GetTotalProgress();

        /// <summary>
        /// 获取渠道版本整包下载地址
        /// </summary>
        /// <returns></returns>
        string GetChannelDownloadUrl();

        /// <summary>
        /// 输出更新过程中的各种信息
        /// </summary>
        /// <param name="msg"></param>
        void OutputMsg(string msg);

        /// <summary>
        /// 重启客户端
        /// </summary>
        void DoRestart();

        /// <summary>
        /// 是否更新成功
        /// </summary>
        /// <returns></returns>
        bool IsUpdateOK(); 

        /// <summary>
        /// 取得更新进度
        /// </summary>
        /// <returns></returns>
           
	    int GetProgress(); 
        
	    /// <summary>
        /// 设置更新进度
        /// </summary>
        /// <param name="value"></param>
	    void SetProgress(int value);
 
        /// 取得更新文本信息
        string GetUpdateMsg();

        /// <summary>
        /// 获取版本信息
        /// </summary>
        /// <returns></returns>
        string GetVer();

        /// <summary>
        /// 是否需要重启
        /// </summary>
        /// <returns></returns>
        bool GetNeedReStart();

        /// <summary>
        /// 初始化是否完成
        /// </summary>
        /// <returns></returns>
        bool GetInitFinish();

        /**
	    @ name  : 从服务器获取最新的服务入口地址
	    @ param : entry_list_url 服务入口地址
	    @ param : backup_url 备用服务入口地址
	    */
        bool RequestEntry(string entry_list_url, string backup_url);

        /// <summary>
        /// 设置获取需更新的新版本流程的状态
        /// </summary>
        /// <returns></returns>
        void SetCheckConfirmFlag();

        /// <summary>
        /// 获得事件引擎
        /// </summary>
        /// <returns></returns>
        IEventEngine GetEventEngine();

        /// <summary>
        /// 获取更新远程配置的最新版本
        /// </summary>
        /// <param name="CallBack">获取成功后的回调</param>
        void CheckNewVersion(System.Action CallBack = null);

        /// <summary>
        /// 获取更新远程配置的最新版本是否比本地版本新
        /// </summary>
        /// <returns></returns>
        bool GetNeedUpdate();

        /// <summary>
        /// 获取QQ群信息
        /// </summary>
        /// <returns></returns>
        string GetQQinfo();

        /// <summary>
        ///  获取网页版公告地址
        /// </summary>
        /// <returns></returns>
        string GetBillboardinfo();

        /// <summary>
        ///  获取信息地址
        /// </summary>
        /// <returns></returns>
        string GetSubscribeinfo();

        /// <summary>
        /// 获取微端信息地址
        /// </summary>
        /// <returns></returns>
        string GetMicroDownloadInfo();

        /// <summary>
        ///  切换更新配置文件地址
        /// </summary>
        /// <returns></returns>
        void SwitchVersionUrl();

        /// <summary>
        /// 是否正在下载中
        /// </summary>
        /// <returns></returns>
        bool GetIsDownload();


		/// <summary>
		/// 获取iOS 12 是否播放CG
		/// </summary>
		/// <returns></returns>
		bool GetiOS12Play();
    }

    public interface IBoot
    {
        /// <summary>
        /// 关闭启动器
        /// </summary>
        void CloseLauncher();
    }
}    
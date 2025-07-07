using System;
using System.Collections;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using XGame.Http;
using System.Collections.Generic;
using XGame;
using XGame.CoroutinePool;

namespace XClient.Client
{
    /// <summary>
    /// 用来推动更新
    /// </summary>
    public class WWWRequestMono : MonoBehaviour
    {
		private static WWWRequestMono _instance;
		public static WWWRequestMono Instance
        {
			get
			{
				if (_instance == null)
				{
					GameObject go = new GameObject();
					go.hideFlags = HideFlags.HideAndDontSave;
					_instance = go.AddComponent<WWWRequestMono>();
				}
				return _instance;
			}
		}

		private Dictionary<WWWRequest, bool> m_dicRequest = new Dictionary<WWWRequest, bool>();
		private List<WWWRequest> m_lstUnregisters = new List<WWWRequest>();

		public void Register(WWWRequest reqest)
        {
			m_dicRequest.Add(reqest, true);
		}

		public void Unregister(WWWRequest request)
        {
			//m_dicRequest.Remove(request);
			m_lstUnregisters.Add(request);
		}

        private void FixedUpdate()
        {
            foreach(var k in m_dicRequest.Keys)
            {
				if(!k.RequestFinish)
                {
					k.OnFixedUpdate();
				}
            }

			if(m_lstUnregisters.Count > 0)
            {
                foreach (var r in m_lstUnregisters)
                {
                    m_dicRequest.Remove(r);
                }

                m_lstUnregisters.Clear();
            }
		}
    }

    /// <summary>
    /// WWW请求模块基类
    /// </summary>
    public class WWWRequest
	{
		/// <summary>
		/// 请求状态
		/// </summary>
		enum RequestState
		{
			Idle = -1,
			Get,							//请求
			Get_Timeout,			//超时
			Get_Err,					//错误
			Get_Finish,				//成功
		}

        //完成回调
        public Action<bool, string> onFinishedCallBack;

        //进度回调
        public Action<float, string> onProgressCallBack;

        //是否要将请求结果保存到文件中
        public bool isSave2File = true;

        //请求结果保存文件
        public string saveFileName = "serverlist.txt";

        //请求失败的时候是否使用本地缓存文件
        public bool useLocalFileWhenRequestFail = true;

        //请求超时重试次数
        protected int m_requestMaxCount = 3;

		//请求超时时间
		protected float m_timeoutSecond = 6.0f;

		//请求完成标志
		private bool m_bRequestFinish = false;

		//最近一次请求的时间
		private float m_fLastRequestTime = 0.0f;

		//当前模块的状态
		private RequestState m_nState;

		//当前请求的次数
		private int m_nCount = 0;

		//请求的服务器地址
		public string m_Url = string.Empty;

		//请求本地处理成功
		private bool m_OK = false;

		//请求本地处理失败
		private bool m_Fail = false;

		private bool isOK
        {

			get => m_OK;
			set
            {
				m_OK = value;
				if(value)
					m_Fail = false;
            }
        }

        private bool isFail
        {

            get => m_Fail;
            set
            {
				m_Fail = value;
				if(value)
					m_OK = false;
            }
        }

        //协程执行器
        private IRawTaskExecutor m_rawTaskExecutor;

		//请求对象
		private UnityWebRequest m_request;

		/// <summary>
		/// 请求列表成功
		/// </summary>
		public bool RequestFinish
		{
			get { return m_bRequestFinish; }
		}

		static WWWRequest()
        {
			HttpCertificate.InitCertificate();
		}

		/// <summary>
		/// 构造
		/// </summary>
		/// <param name="url">请求的url地址</param>
		public WWWRequest()
		{
			m_nState = RequestState.Idle;
			m_nCount = 0;
			WWWRequestMono.Instance.Register(this);
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public void Dispose()
        {
            //停止协程
            StopGetTaskExecutor();

			//销毁请求对象
			if(m_request != null)
            {
				m_request.Dispose();
				m_request = null;
			}

			//注销自己
            WWWRequestMono.Instance.Unregister(this);
		}

		/// <summary>
		/// 开始请求
		/// </summary>
		public void StartRequest(string url)
		{
			if (RequestState.Get == m_nState )
			{
				return;
			}

			//空url？
			if (string.IsNullOrEmpty(url))
			{
				SetState(RequestState.Get_Err);
				XGame.Trace.TRACE.ErrorLn("WWWRequestBase::StartRequest url is empty");
				return;
			}

			m_bRequestFinish = false;
			m_OK = false;
			m_Url = url;
			SetState(RequestState.Get);
		}

		public virtual bool CheckDownloadData(string data)
		{
			if (string.IsNullOrEmpty(data))
				return false;
			return true;
		}

		/// <summary>
		/// 发送http请求
		/// </summary>
		/// <returns></returns>
		private IEnumerator SendRequest()
		{
			if (m_request != null)
				m_request.Dispose();

			m_fLastRequestTime = Time.realtimeSinceStartup;
			m_request = UnityWebRequest.Get(m_Url);
			m_request.certificateHandler = HttpCertificate.CreateAlwaysPassCertificate();

			UnityWebRequestAsyncOperation asyncOP = m_request.SendWebRequest();
			while (!asyncOP.isDone)
			{
				onProgressCallBack?.Invoke(m_request.downloadProgress, m_Url);
				yield return null;
			}

            if (string.IsNullOrEmpty(m_request.error) == false || m_request.result != UnityWebRequest.Result.Success || m_request.downloadedBytes == 0)
            {
				m_Fail = true;
				Debug.LogError("WWWRequestBase::SendRequest 请求URL失败 error=" + m_request.error + ",URL=" + m_Url);
			}
			else
            {
				string Result = m_request.downloadHandler.text;
				if (Result != null)
				{
					if (CheckDownloadData(Result))
					{
						try
                        {
							//回调到外部
							onFinishedCallBack?.Invoke(true, Result);

                            //保持信息到本地
                            if (isSave2File)
                                SaveDataToLocalFile(Result);

							isOK = true;
						}
						catch(Exception e)
                        {
							Debug.LogError("WWWRequest 回调处理失败！Url=" + m_Url + "\n" + e.Message);
							isFail = true;
						}
					}
					else
					{
						isFail = true;
					}
				}
			}

			//可能在回调的时候已经销毁了
			if(m_request != null)
            {
                m_request.Dispose();
                m_request = null;
            }

			//停止协程
			StopGetTaskExecutor();
		}

		private void StopGetTaskExecutor()
        {
            if (m_rawTaskExecutor != null)
            {
                ICoroutineManager coroutineManager = XGame.XGameComs.Get<ICoroutineManager>();
                coroutineManager.RecycleRawTaskExecutor(m_rawTaskExecutor);
                m_rawTaskExecutor = null;
            }
        }

		//保持服务器列表到本地文件
		private bool SaveDataToLocalFile(string data)
		{
			string strFilePath = GetSaveFilePath();
			try
			{
				FileStream fs = new FileStream(strFilePath, FileMode.Create);
				StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
				sw.Write(data);
				//清空缓冲区
				sw.Flush();
				//关闭流
				sw.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
				XGame.Trace.TRACE.ErrorLn("WWWRequestBase::SaveDataToLocalFile 写文件错误错误 path=" + strFilePath + ",ex=" + ex.ToString());
				return false;
			}

			return true;
		}

		private string GetSaveFilePath()
        {
			string strFilePath = GamePath.GetAssetBundleRootPathExternal() + saveFileName;
			return strFilePath;
		}

		//从本地文件读取服务器列表
		private string LoadDataFromFile()
		{
			string strServerlistData = string.Empty;
			string strFilePath = GetSaveFilePath();

			// 文件不存在创建文件
			if (File.Exists(strFilePath))
			{
				try
				{
					StreamReader reader = new StreamReader(strFilePath, Encoding.UTF8);
					strServerlistData = reader.ReadToEnd();
					reader.Close();
					Debug.LogError("远端获取数据失败，尝试从本地读数据：" + strFilePath);
				}
				catch (Exception ex)
				{
					Debug.LogError("WWWRequestBase::LoadDataFromFile 读文件错误错误 path=" + strFilePath + ", ex=" + ex.ToString());
					strServerlistData = string.Empty;
				}
			}
			else
            {
				Debug.LogError("WWWRequestBase::LoadDataFromFile 读文件错误错误 path=" + strFilePath + ", ex=文件不存在！");
			}

			return strServerlistData;
		}

		private bool SetState(RequestState nState)
		{
			// 旧的流程
			RequestState nOldState = m_nState;
			// 当游戏流程退出
			OnExit(nOldState, nState);
			// 改变流程
			m_nState = nState;
			// 当游戏流程进入
			OnEnter(nState, nOldState);
#if DEBUG_LOG
///#///#			//XGame.Trace.TRACE.TraceLn("WWWRequestBase.SetState():" + nOldState.ToString() + "->" + nState.ToString());
#endif
			return true;
		}

		private void OnEnter(RequestState nState, RequestState nOldState)
		{
			// 流程
			switch (nState)
			{
				case RequestState.Get:
					m_nCount++;
					m_fLastRequestTime = Time.realtimeSinceStartup;

					//先停止原来的协程
					if(m_rawTaskExecutor != null)
                    {
						Debug.LogError("原来的协程还没有执行完成！！");
                        m_rawTaskExecutor.StopAllTask();
                    }

					ICoroutineManager coroutineManager = XGame.XGameComs.Get<ICoroutineManager>();
					m_rawTaskExecutor = coroutineManager.GetRawTaskExecutor("WWWRequestBase");
					m_rawTaskExecutor.StartTask(SendRequest());

					break;
				case RequestState.Get_Finish:
					m_bRequestFinish = true;
					break;
				case RequestState.Get_Err:
					break;
				case RequestState.Get_Timeout:
					break;
			}
		}

		public void OnFixedUpdate()
		{
			switch (m_nState)
			{
				case RequestState.Idle:
					break;
				case RequestState.Get:
					{
						if (m_OK)
						{
							SetState(RequestState.Get_Finish);
							break;
						}

						if (m_Fail)
						{
							SetState(RequestState.Get_Err);
							break;
						}

						//超时检查
						if (Time.realtimeSinceStartup > m_fLastRequestTime + m_timeoutSecond)
						{
							SetState(RequestState.Get_Timeout);
							break;
						}
					}
					break;
				case RequestState.Get_Finish:
					break;
				case RequestState.Get_Err:
					{
						//如果请求错误，就从本地文件读取
						if(useLocalFileWhenRequestFail)
                        {
                            string fileData = LoadDataFromFile();
                            if (CheckDownloadData(fileData))
                            {
                                onFinishedCallBack?.Invoke(true, fileData);
                                SetState(RequestState.Get_Finish);
                            }
                            else
                            {
                                onFinishedCallBack?.Invoke(false, fileData);
								string strFilePath = GetSaveFilePath();
                                Debug.LogError("WWWRequestBase::OnFixedUpdate 从本地获取" + strFilePath + "出错");
                                SetState(RequestState.Get_Finish);
                            }
                        }
						else
                        {
                            onFinishedCallBack?.Invoke(false, string.Empty);
                            SetState(RequestState.Get_Finish);
                        }
					}
					break;
				case RequestState.Get_Timeout:
					//请求超时了，就重新请求
					if (m_nCount < m_requestMaxCount)
					{
						SetState(RequestState.Get);
						break;
					}
					SetState(RequestState.Get_Err);
					break;
				default:
					break;
			}
		}

		private void OnExit(RequestState nState, RequestState nNewState)
		{
			switch (nState)
			{
				case RequestState.Get:
					break;
				case RequestState.Get_Finish:
					break;
				case RequestState.Get_Err:
					break;
				case RequestState.Get_Timeout:
					break;
			}
		}
	}
}




/*******************************************************************
** 文件名: VerifyResult.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/12/7
** 版  本: 1.0
** 描  述: 资源检测工具—检测结果 
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XGameEditor.AssetVerifyTools
{
	public class VerifyResult
	{
		//资源名称
		private string m_szAssetName;
		//资源路径
		private string m_szAssetPath;
		//输出信息列表
		private List<string> m_verifyInfoList;

		//检测结果
		private VerifyResultType m_eResultType;

		public VerifyResult()
		{
			m_eResultType = VerifyResultType.UnCheck;
			m_verifyInfoList = new List<string>();
		}

		/// <summary>
		/// 获取检测结果
		/// </summary>
		/// <returns></returns>
		public VerifyResultType GetVerifyResult()
		{
			return m_eResultType;
		}

		/// <summary>
		/// 设置检测结果
		/// </summary>
		/// <param name="type"></param>
		public void SetResultType(VerifyResultType type)
		{
			m_eResultType = type;
		}

		/// <summary>
		/// 填充资源基础信息
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="assetPath"></param>
		public void FillAssetBaseInfo(string assetName, string assetPath)
		{			
			m_szAssetName = assetName;
			m_szAssetPath = assetPath;

			if (m_eResultType == VerifyResultType.UnCheck)
			{
				m_eResultType = VerifyResultType.Passed;
			}
		}

		/// <summary>
		/// 增加检测输出日志信息
		/// </summary>
		/// <param name="infoLevel">信息等级</param>
		/// <param name="info">信息内容</param>
		/// <param name="asset">资源对象，方便Unity中定位资源</param>
		public void AddOutputInfo(LogLevel infoLevel, string info, UnityEngine.Object asset)
		{
			string logInfo = string.Format("{0}({1}):{2}", m_szAssetName, m_szAssetPath, info);
			if (infoLevel == LogLevel.Log)
			{
				Debug.Log(logInfo, asset);
			}
			else if (infoLevel==LogLevel.Warning)
			{
				Debug.LogWarning(logInfo, asset);
			}
			else if (infoLevel==LogLevel.Error)
			{
				Debug.LogError(logInfo, asset);
			}

			m_verifyInfoList.Add(info);
		}
	}
}

/*******************************************************************
** 文件名: VerifyManager.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/11/2
** 版  本: 1.0
** 描  述: 资源检测工具—检测管理器 
** 应  用: 
**************************** 修改记录 ******************************
** 修改人:  梁成
** 日  期:  2018-12-05
** 描  述: 增加附加条件功能，检测时可以检测不同条件下的资源（例如可以就技能音效和背景音乐设置不同的限制条件）
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XGameEditor.AssetVerifyTools
{
	public class VerifyManager 
	{
		private Dictionary<VerifyActionType, VerifyActionBase> m_verifyActionMap;

		private static VerifyManager m_Instance;
		public static VerifyManager GetInstance()
		{
			if(m_Instance == null)
			{			
				m_Instance = new VerifyManager();
			}
			
			return m_Instance;
		}

		public VerifyManager()
		{
			m_verifyActionMap = new Dictionary<VerifyActionType, VerifyActionBase>();
			Init();
		}

		~VerifyManager()
		{
			Release();
		}

		public void Release()
		{
			if(m_verifyActionMap != null)
			{
				m_verifyActionMap.Clear();
			}

			m_Instance = null;
		}

		///初始化配置
		public void Init()
		{
			m_verifyActionMap.Add(VerifyActionType.ValueVerify, new ValueVerifyAction());
			m_verifyActionMap.Add(VerifyActionType.ToggleVerify, new ToggleVerifyAction());
			m_verifyActionMap.Add(VerifyActionType.OptionVerify, new OptionVerifyAction());
		}

		/// <summary>
		/// 获取Action
		/// </summary>
		/// <param name="actionType">动作类型</param>
		/// <returns></returns>
		public VerifyActionBase GetVerifyAction(VerifyActionType actionType)
		{
			VerifyActionBase verifyAction = null;

			if(m_verifyActionMap != null)
			{
				m_verifyActionMap.TryGetValue(actionType, out verifyAction);
			}

			return verifyAction;
		} 
	}
}

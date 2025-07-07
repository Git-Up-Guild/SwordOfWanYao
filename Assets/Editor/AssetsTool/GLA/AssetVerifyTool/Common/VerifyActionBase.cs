/*******************************************************************
** 文件名: VerifyActionBase.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/11/2
** 版  本: 1.0
** 描  述: 资源检测工具—检测行为基类   
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;

namespace XGameEditor.AssetVerifyTools
{
	public abstract class VerifyActionBase 
	{
		/// <summary>
		/// 获取动作类型
		/// </summary>
		/// <returns></returns>
		public abstract VerifyActionType GetActionType();

		/// <summary>
		/// 执行检测
		/// </summary>
		/// <param name="verifyObj">检测对象</param>
		/// <param name="ruleData">检测规则</param>
		/// <param name="resultData">检测结果</param>
		public abstract void Execute(UnityEngine.Object verifyObj, VerifyRuleData ruleData, ref VerifyResult resultData);
	}
}

/*******************************************************************
** 文件名: CommonVerifyAction.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/12/7
** 版  本: 1.0
** 描  述: 资源检测工具—封装公共检测行为  
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections;

namespace XGameEditor.AssetVerifyTools
{
	public class CommonVerifyAction : VerifyActionBase
	{
		public override VerifyActionType GetActionType()
		{
			UnityEngine.Debug.LogError("子类必须重写GetActionType方法！！！");
			return VerifyActionType.Unknown;
		}

		/// <summary>
		/// 执行检测
		/// </summary>
		/// <param name="verifyObj">检测对象</param>
		/// <param name="ruleData">检测规则</param>
		/// <param name="resultData">检测结果</param>
		public override void Execute(UnityEngine.Object verifyObj, VerifyRuleData ruleData, ref VerifyResult resultData)
		{
			if(verifyObj == null)
			{
				resultData.SetResultType(VerifyResultType.Failed);
				resultData.AddOutputInfo(LogLevel.Error,"校验对象为空!", verifyObj);
				return;
			}

			if(ruleData == null)
			{
				resultData.SetResultType(VerifyResultType.Failed);
				resultData.AddOutputInfo(LogLevel.Error,"校验规则为空!", verifyObj);
				return;
			}

			if(ruleData.ruleId == 0 || ruleData.actionType != GetActionType())
			{
				resultData.SetResultType(VerifyResultType.Failed);
				resultData.AddOutputInfo(LogLevel.Error,"检测规则为空或与当前动作类型不一致! 规则id:" + ruleData.ruleId, verifyObj);
				return;
			}

			if(string.IsNullOrEmpty(ruleData.paramArry[0]))
			{
				resultData.SetResultType(VerifyResultType.Failed);
				resultData.AddOutputInfo(LogLevel.Error,"检测规则属性名为空! 规则id:" + ruleData.ruleId, verifyObj);
				return;
			}

			if(string.IsNullOrEmpty(ruleData.paramArry[1]))
			{
				resultData.SetResultType(VerifyResultType.Failed);
				resultData.AddOutputInfo(LogLevel.Error,"检测规则范围值为空! 规则id:" + ruleData.ruleId, verifyObj);
				return;
			}         
		} //end of Execute
	} //end of class
} //end of namespace

/*******************************************************************
** 文件名: VerifyRuleData.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/12/7
** 版  本: 1.0
** 描  述: 资源检测工具—检测规则
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;

namespace XGameEditor.AssetVerifyTools
{
	public class VerifyRuleData
	{
		//索引ID
		public int ruleId;
		//检测动作类型
		public VerifyActionType actionType;
		// //规则属性名
		// public string ruleName;
		// //取值范围
		// public string valueRange;
		// //自动修复
		// public bool autoFix;
		// //标准值
		// public string standardValue;
		public string[] paramArry;

		public VerifyRuleData(int id, VerifyActionType type, string[] valueArry)
		{
			ruleId = id;
			actionType = type;

			paramArry = new string[VerifyDefine.VerifyRuleParamNum];

			int index = 0;
			foreach(string value in valueArry)
			{
				if(index >= VerifyDefine.VerifyRuleParamNum)
				{
					break;
				}

				paramArry[index++] = value;
			}
		}
	}
}



/*******************************************************************
** 文件名: VerifyStrategyBase.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/11/2
** 版  本: 1.0
** 描  述: 资源检测工具—检测策略基类  
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;

namespace XGameEditor.AssetVerifyTools
{
	public abstract class VerifyStrategyBase
	{
		/// <summary>
		/// 加载检测规则配置
		/// </summary>
		/// <param name="ruleConfigPath">配置路径</param>
		/// <returns></returns>
		public abstract bool LoadVerifyRuleConfig(string ruleConfigPath);

		/// <summary>
		/// 检测资源
		/// </summary>
		/// <param name="asset">资源对象</param>
		/// <returns></returns>
		public abstract VerifyResult VerifyAsset(UnityEngine.Object asset);
	}
}

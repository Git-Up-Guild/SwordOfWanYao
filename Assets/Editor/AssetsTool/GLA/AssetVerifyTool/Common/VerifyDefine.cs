/*******************************************************************
** 文件名: VerifyDefine.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/11/2
** 版  本: 1.0
** 描  述: 资源检测工具—检测项目定义   
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;

namespace XGameEditor.AssetVerifyTools
{	
	///检测工具检测对应的实际类型
	///除了Fbx和Scene之外，其他跟实际检测对象的GetType同名（省略UnitnEngine的namespace)
	///除了Fbx和Scene之外，检测工具检测界面的标签页，配置表里的类型，以及获取检测资源的筛选类型，都用这个名字
	/// <summary>
	/// 检测资源类型
	/// </summary>
	public enum VerifyAssetType
    {
        Model = 0,
        Texture,
        Prefab,
        Animation,
        Material,
        Scene,
        Audio,
		Max,
    }

	/// <summary>
	/// 检测动作类型
	/// </summary>
	public enum VerifyActionType : int
	{
		//未知类型
		Unknown = 0,
		//值类型
		ValueVerify,
		//勾选型
		ToggleVerify,
		//选项型
		OptionVerify,
	}

	/// <summary>
	/// 检测结果类型
	/// </summary>
	public enum VerifyResultType : int
	{
		//未检测
		UnCheck,
		//警告
		Warning,
		//失败
		Failed,
		//已修复
		Fixed,
		//通过
		Passed,
	}

	/// <summary>
	/// 检测日志等级
	/// </summary>
	public enum LogLevel
	{
		Log,
		Warning,
		Error,
	}

	public static class VerifyDefine
	{
		public static readonly int VerifyRuleParamNum = 8;
	}
}






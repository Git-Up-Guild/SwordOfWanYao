/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: ModuleConfigGenerate.cs 
Author: 2025.06.28
Date: 2024.06.11
Description: 自动生成模块配置，请不要手动修改
***************************************************************************/

using XClient.Common;

namespace GameScripts.TestDemo
{
    /// <summary>
    /// 模块ID声明
    /// </summary>
    public partial class DModuleIDEx
    {
		public static int GEN_CUR_EX = 24;     
        public static readonly int MODULE_ID_ZCCARD =  ++GEN_CUR_EX;     //卡牌模块
		//@AppendDeclareModuleID
    }
	
	/// <summary>
    /// 全局对象扩展
    /// </summary>
    public class GameGlobalEx : GameGlobal
    {
        public static ZCCard.IZCCardModule ZCCard = null;
		//@AppendDeclareModuleInstanceVars
    }

    /// <summary>
    /// 模块实例变量
    /// </summary>
    public static class ExtenModulesSetupGenerate
    {
        public static void Setup(IModule[] modules)
        {
			GameGlobalEx.ZCCard = Setup<ZCCard.ZCCardModule>(DModuleIDEx.MODULE_ID_ZCCARD, modules);
			//@AppendSetupModuleInstanceVar
        }

        public static void Clear()
        {
			GameGlobalEx.ZCCard = null;
			//@AppendClearModuleInstanceVar
        }

        private static T Setup<T>(int moduleID, IModule[] modules) where T : class, IModule, new()
        {
            T module = new T();
            modules[moduleID] = module;
            return module;
        }
    }
}


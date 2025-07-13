/**************************************************************************    
文　　件：GameExtensions.cs
作　　者：郑秀程
创建时间：2025.07.12
描　　述：游戏扩展启动脚本
***************************************************************************/

using XClient.Common;
using UnityEngine;

namespace GameScripts.UITsetDemo
{
	public class GameExtensions : MonoBehaviour, IGameExtensions
    {
        public void OnSetupExtenModules(IModule[] modules)
        {
            //可以将非自动生成代码的模块写在这里
            //modules[DModuleIDEx.MODULE_ID_ENTITY_CLIENT] = new XXXModule();

            //因为代码是自动生成的，如果没有启用此功能，则此类不存在。通过反射可以避免报错
            GameScripts.UITsetDemo.ExtenModulesSetupGenerate.Setup(modules);
        }
		
		public void OnAfterSetupExtenModules()
		{
		}
    }
}

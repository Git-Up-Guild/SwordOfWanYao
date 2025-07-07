
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XClient.Entity.Part;
using XClient.Network;
using XGame;
using XGame.Poolable;
using XGame.UI.State;
using XGame.Utils;

namespace GameScripts
{
    public class GameExtensions : MonoBehaviour, IGameExtensions
    {
        public void OnSetupExtenModules(IModule[] modules)
        {
            //可以将非自动生成代码的模块写在这里
            //modules[DModuleIDEx.MODULE_ID_ENTITY_CLIENT] = new XXXModule();

            //因为代码是自动生成的，如果没有启用此功能，则此类不存在。通过反射可以避免报错
            string typePath = GameConfig.Instance.strNamespace + ".GameScripts.ExtenModuleConfigGenerate";
            var type = ReflectionUtils.GetTypeByTypePath(typePath);
            if (type != null)
            {
                var setupMethod = type.GetMethod("Setup");
                if (setupMethod != null)
                {
                    setupMethod.Invoke(null, new object[] { modules });
                }
            }
        }

        public void OnAfterSetupExtenModules()
        {
        }
    }
}

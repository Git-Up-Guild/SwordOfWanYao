
using UnityEditor;
using System.Collections.Generic;
using XGame.Effect;

namespace XGameEditor.Effect
{
    namespace TrailsSystem
    {
        [InitializeOnLoad]
        public class ExecutionOrderSetter
        {
            static ExecutionOrderSetter()
            {
                List<string> scriptlist = new List<string>
            {
                typeof (Effect_TrailRenderer_Base).Name,
                typeof (Effect_Trail).Name,
                typeof (Effect_SmoothTrail).Name,
                typeof (Effect_SmokeTrail).Name,
                typeof (Effect_SmokePlume).Name
            };

                foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
                {
                    if (!scriptlist.Contains(monoScript.name)) continue;

                    if (MonoImporter.GetExecutionOrder(monoScript) != 1000)
                        MonoImporter.SetExecutionOrder(monoScript, 1000);
                }
            }
        }
    }
}


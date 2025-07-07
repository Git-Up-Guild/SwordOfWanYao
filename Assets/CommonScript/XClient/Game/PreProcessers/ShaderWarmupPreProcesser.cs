//

using UnityEngine;
using XGame.Asset;
using XGame.Utils;

namespace XClient.Game
{
    /// <summary>
    /// Shader预处理
    /// </summary>
    public class ShaderWarmupPreProcesser : GamePreProcesser
    {
        public ResourceRef shaderWarmUp;

        public override void Execute()
        {
            if (string.IsNullOrEmpty(shaderWarmUp.path))
            {
                isFinished = true;
                return;
            }

            IGAssetLoader loader = XGame.XGameComs.Get<IGAssetLoader>();
            uint handle;
            ShaderCollectionWarmUp warmUp = loader.LoadResSync<ShaderCollectionWarmUp>(shaderWarmUp.path, out handle) as ShaderCollectionWarmUp;
            if (warmUp != null)
            {
                warmUp.Warmup(() =>
                {
                    isFinished = true;
                });
            }
            else
            {
                Debug.LogWarning("加载 ShaderCollectionWarmUp 失败！！");
                isFinished = true;
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

namespace XClient.Game
{
    public class ShaderCollectionWarmUp : ScriptableObject //: MonoBehaviour
    {
        [SerializeField]
        public ShaderVariantCollection[] shaderVariantCollections = null;

#pragma warning disable 0414
        //[SerializeField]
        //private string[] m_ExtraShaderNames = null;
#pragma warning restore 0414

        [SerializeField]
        public List<Shader> shaders = null;

        public void Warmup(System.Action callback)
        {
            foreach (var shaderCollection in shaderVariantCollections)
            {
                shaderCollection.WarmUp();
            }

            callback?.Invoke();
        }
    }
}
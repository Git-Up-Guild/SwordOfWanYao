using UnityEditor;
using UnityEngine;
using XGame.Effect;

namespace XGameEditor.Effect
{
    [CustomEditor(typeof(UVParticle))]
    public class UVUVParticleInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UVParticle uvcomponent = (UVParticle)target;
            if (uvcomponent.IsPlay())
            {
                if (GUILayout.Button("Stop"))
                {
                    uvcomponent.Stop();
                }
            }
            else
            {
                if (GUILayout.Button("Play"))
                {
                    uvcomponent.Play();
                }
            }
        }
    }
}

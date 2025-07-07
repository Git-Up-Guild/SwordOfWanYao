using UnityEditor;
using UnityEngine;
using XGame.Effect;

namespace XGameEditor.Effect
{
    [CustomEditor(typeof(UVProjector))]
    public class UVProjectorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UVProjector uvcomponent = (UVProjector)target;
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

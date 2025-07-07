using System.Collections.Generic;
using UnityEditor;
using XGame.Effect;

namespace XGameEditor.Effect
{
    namespace TrailsSystem
    {
        [CustomEditor(typeof(Effect_SmokePlume))]
        [CanEditMultipleObjects]
        public class SmokePlumeEditor : TrailEditor_Base
        {
            protected override void DrawTrailSpecificGUI()
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ConstantForce"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RandomForceScale"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TimeBetweenPoints"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxNumberOfPoints"));
            }
        }
    }
}


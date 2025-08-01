﻿using System.Collections.Generic;
using UnityEditor;
using XGame.Effect;

namespace XGameEditor.Effect
{
    namespace TrailsSystem
    {
        [CustomEditor(typeof(Effect_SmoothTrail))]
        [CanEditMultipleObjects]
        public class SmoothTrailEditor : TrailEditor_Base
        {
            protected override void DrawTrailSpecificGUI()
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MinControlPointDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxControlPoints"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PointsBetweenControlPoints"));
            }
        }
    }
}


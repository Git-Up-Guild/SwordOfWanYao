﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using XGame.Effect;

namespace XGameEditor.Effect
{
    [CustomEditor(typeof(Effect_WeaponTrail))]
    public class Effect_WeaponTrailEditor : Editor
    {
        //Effect_WeaponTrail weap = null;
        void OnEnable()
        {
            //weap = target as Effect_WeaponTrail;
        }
        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            DrawDefaultInspector();
            //if (GUILayout.Button("Play"))
            //{
            //    weap.Play();
            //}
            //if (GUILayout.Button("Stop"))
            //{
            //    weap.Stop();
            //}
        }
    }

}



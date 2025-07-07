/*******************************************************************
** 文件名:	Effect_UVScrollEditor.cs
** 版  权:	(C) 深圳冰川网络技术有限公司 2015 - Speed
** 创建人:	谭强均
** 日  期:	2015/6/9
** 版  本:	1.0
** 描  述:	Effect_UVScroll编辑器脚本
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using XGame.Effect;

namespace XGameEditor.Effect
{
    [CustomEditor(typeof(Effect_UVScroll))]
    public class Effect_UVScrollEditor : Editor
    {

        Effect_UVScroll uvscroll;
        SerializedProperty TexturePropertyNamesProperty = null;
        void OnEnable()
        {
            uvscroll = target as Effect_UVScroll;
            TexturePropertyNamesProperty = serializedObject.FindProperty("TexturePropertyNames");
        }

        public override void OnInspectorGUI()
        {
            //uvscroll.isLoop = EditorGUILayout.Toggle("是否循环:", uvscroll.isLoop);
            //uvscroll.IncludeChildren = EditorGUILayout.Toggle("是否包括子节点:", uvscroll.IncludeChildren);
            EditorGUILayout.HelpBox("Effect_UVScroll脚本不能和Effect_UVTextureAnimator同时作用于同一张贴图", MessageType.Warning);

            uvscroll.RunMode = (EffectRunMode)EditorGUILayout.EnumPopup("运行模式:", uvscroll.RunMode);

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(TexturePropertyNamesProperty, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            uvscroll.UseModelUV = EditorGUILayout.Toggle("使用模型UV:", uvscroll.UseModelUV);
            uvscroll.XScrollSpeed = EditorGUILayout.FloatField("X方向速度:", uvscroll.XScrollSpeed);
            uvscroll.YScrollSpeed = EditorGUILayout.FloatField("Y方向速度:", uvscroll.YScrollSpeed);
            //uvscroll.UVOffset = EditorGUILayout.Vector2Field("UV偏移:", uvscroll.UVOffset);
            uvscroll.RoateSpeed = EditorGUILayout.Vector3Field("旋转速度:", uvscroll.RoateSpeed);
            uvscroll.RoateInWorldSpace = EditorGUILayout.Toggle("是否在世界空间旋转:", uvscroll.RoateInWorldSpace);
            //uvscroll.RoateCenter = EditorGUILayout.Vector2Field("旋转中心点:", uvscroll.RoateCenter);
            //uvscroll.ClampRoateUVRange = EditorGUILayout.Toggle("修正UV旋转范围:", uvscroll.ClampRoateUVRange);
            //if (uvscroll.ClampRoateUVRange)
            //{
            //    EditorGUILayout.LabelField("UV旋转的时候四个角会出现多余的纹理，\n选中这个可以修复，但是会导致UV流动动画出\n现问题~", GUILayout.Height(50.0f));
            //}

            if (GUILayout.Button("Play"))
            {
                uvscroll.Play();
            }

            if (GUILayout.Button("Stop"))
            {
                uvscroll.Stop();
            }
            if (GUILayout.Button("Reset"))
            {
                uvscroll.Reset();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(uvscroll);
            }
        }
    }
}


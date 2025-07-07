/*******************************************************************
** 文件名:	Effect_PlayEditor.cs
** 版  权:	(C) 深圳冰川网络技术有限公司 2014 - Speed
** 创建人:	谭强均
** 日  期:	2014/11/21
** 版  本:	1.0
** 描  述:	特效功能脚本
** 应  用:  	播放特效编辑器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using System.Collections;
using UnityEditor;
using XGame.Effect;
namespace XGameEditor.Effect
{
    public class Editor_PreparationEffect : Editor
    {
        IEffectPreparation m_preparationEffect;

        void OnEnable()
        {
            m_preparationEffect = target as IEffectPreparation;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("Please click UpdateEffectData after changing the animator or particleSystems.", MessageType.Info);

            if (GUILayout.Button("Update Effect Data"))
            {
                m_preparationEffect.UpdateEffectData();
            }
        }
    }

    [CustomEditor(typeof(Effect_ParticleSystem), false)]
    public class Editor_Effect_ParticleSystem : Editor_PreparationEffect { }

    [CustomEditor(typeof(Effect_Animator), false)]
    public class Editor_Effect_Animator : Editor_PreparationEffect { }

}

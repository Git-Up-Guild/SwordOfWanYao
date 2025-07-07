/*******************************************************************
** 文件名:	Effect_FadeInOutMaterialFloatEditor.cs
** 版  权:	(C) 深圳冰川网络技术有限公司 2014 - Speed
** 创建人:	谭强均
** 日  期:	2015/8/4
** 版  本:	1.0
** 描  述:	特效功能脚本
** 应  用:  	float淡入淡出编辑器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using UnityEditor;
using XGame.Effect;

namespace XGameEditor.Effect
{
    [CustomEditor(typeof(Effect_FadeInOutMaterialFloat))]
    public class Effect_FadeInOutMaterialFloatEditor : Editor
    {
        Effect_FadeInOutMaterialFloat pa;
        void OnEnable()
        {
            pa = target as Effect_FadeInOutMaterialFloat;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.TextField("所有跟时间有关的变量小于0.03秒，等同于无效!");
            if (GUILayout.Button("Play"))
            {
                pa.Play();
            }
            if (GUILayout.Button("Stop"))
            {
                pa.Stop();
            }
        }
    }
}

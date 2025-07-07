/*****************************************************
** 文 件 名：GameAppInspector
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/6/19 22:09:41
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using XGame.Utils;
using XClient.Game;
using UnityEditor;
using UnityEngine;

namespace XGameEditor
{
    [CustomEditor(typeof(GameApp))]
    class GameAppInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            GameApp gameApp = target as GameApp;
            base.OnInspectorGUI();

            bool bRet;

            bRet = gameApp.isEnableTransformDebug = EditorGUILayout.Toggle("开启Transform调试", gameApp.isEnableTransformDebug);
            if (bRet != gameApp.isEnableTransformDebug)
            {
                gameApp.isEnableTransformDebug = bRet;
                EditorUtility.SetDirty(target);
            }

            if(gameApp.isEnableTransformDebug)
            {
                gameApp.transformDebugMode = (TransformDebugMode)EditorGUILayout.EnumPopup("    调试方式", gameApp.transformDebugMode);
                if (gameApp.transformDebugMode == TransformDebugMode.NameContains)
                    gameApp.transformDebugName = EditorGUILayout.TextField("    目标Transform名称包含", gameApp.transformDebugName);
                else
                    gameApp.transformDebugTarget = EditorGUILayout.ObjectField("    目标Transform对象", gameApp.transformDebugTarget, typeof(Transform), true) as Transform;
            }

            bRet = gameApp.isEnablePoolableLog = EditorGUILayout.Toggle("开启对象池日志", gameApp.isEnablePoolableLog);
            if (bRet != gameApp.isEnablePoolableLog)
            {
                gameApp.isEnablePoolableLog = bRet;
                EditorUtility.SetDirty(target);
            }

            bRet = EditorGUILayout.Toggle("开启光效调试", gameApp.isEnableLightingEffectDebug);
            if (bRet != gameApp.isEnableLightingEffectDebug)
            {
                gameApp.isEnableLightingEffectDebug = bRet;
                EditorUtility.SetDirty(target);
            }
        }
    }
}

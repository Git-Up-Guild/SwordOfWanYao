using UnityEngine;
using UnityEditor;
using XGame.Effect;

namespace XGameEditor.Effect
{
    [CustomEditor(typeof(Effect_UVTextureAnimator))]
    public class Effect_UVTextureAnimatorEditor : Editor
    {

        Effect_UVTextureAnimator pa;
        SerializedProperty TexturePropertyNamesProperty = null;
        void OnEnable()
        {
            pa = target as Effect_UVTextureAnimator;
            TexturePropertyNamesProperty = serializedObject.FindProperty("TexturePropertyNames");
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Effect_UVScroll脚本不能和Effect_UVTextureAnimator同时作用于同一张贴图", MessageType.Warning);

            pa.RunMode = (EffectRunMode)EditorGUILayout.EnumPopup("运行模式:", pa.RunMode);

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(TexturePropertyNamesProperty, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            pa.Rows = EditorGUILayout.IntField("行:",pa.Rows);
            pa.Columns = EditorGUILayout.IntField("列:",pa.Columns);
            pa.Fps = EditorGUILayout.FloatField("帧数:",pa.Fps);
            pa.SelfTiling = EditorGUILayout.Vector2Field("从哪里开始播[0-1]:",pa.SelfTiling);
            pa.IsLoop = EditorGUILayout.Toggle("循环:",pa.IsLoop);
            pa.IsReverse = EditorGUILayout.Toggle("逆向:",pa.IsReverse);

            pa.UseModelUV = EditorGUILayout.Toggle("使用模型UV:", pa.UseModelUV);

            if (GUILayout.Button("Play"))
            {
                pa.Play();
            }
            if (GUILayout.Button("Stop"))
            {
                pa.Stop();
            }
            if (GUI.changed)
            {
                EditorUtility.SetDirty(pa);
            }
        }
    }
}

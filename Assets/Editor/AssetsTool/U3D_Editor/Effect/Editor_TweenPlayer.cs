using UnityEngine;
using UnityEditor;
using XGame.Effect;
namespace XGameEditor
{
    [CustomEditor(typeof(Effect_TweenPlayer))]
    public class Editor_TweenPlayer : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("Please click UpdateEffectData after changing the tweens when you use autoFindTween.", MessageType.Info);

            if (GUILayout.Button("Update Effect Data"))
            {
                Effect_TweenPlayer tweenPlayer = target as Effect_TweenPlayer;
                tweenPlayer.UpdateTweensData();
            }

            if (GUILayout.Button("Play Tween"))
            {
                Effect_TweenPlayer tweenPlayer = target as Effect_TweenPlayer;
                tweenPlayer.Play();
            }
        }
    }
}
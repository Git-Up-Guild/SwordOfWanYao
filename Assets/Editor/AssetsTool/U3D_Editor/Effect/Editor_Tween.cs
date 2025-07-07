using UnityEngine;
using UnityEditor;
using XGame.Anim.Tween;

[CustomEditor(typeof(TweenCore),true)]
public class Editor_Tween : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Play To"))
        {
            TweenCore tween = target as TweenCore;
            tween.PlayTo();
        }

        if (GUILayout.Button("Play From"))
        {
            TweenCore tween = target as TweenCore;
            tween.PlayFrom();
        }

        if (GUILayout.Button("All Play To"))
        {
            TweenCore tween = target as TweenCore;
            var ts = tween.GetComponents<TweenCore>();
            foreach(var t in ts)
            {
                if (t.enabled)
                    t.PlayTo();
            }
        }

        if (GUILayout.Button("All Play From"))
        {
            TweenCore tween = target as TweenCore;
            var ts = tween.GetComponents<TweenCore>();
            foreach (var t in ts)
            {
                if (t.enabled)
                    t.PlayFrom();
            }
        }
    }
}

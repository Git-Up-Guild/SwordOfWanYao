/**************************************************************************    
文　　件：SkeletonDataAssetPreviewerWindow
作　　者：郑秀程
创建时间：2021/1/31 10:10:46
描　　述：
***************************************************************************/
using System;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{

    using Event = UnityEngine.Event;

    public class AnimationPreviewerData : ScriptableObject
    {
        public SkeletonDataAsset skeletonDataAsset;
    }

    class AnimationPreviewerWindow : EditorWindow
    {
        [MenuItem("Spine/预览窗口", false, 100)]
        public static void ShowSkeletonDataAssetPreviewerWindow()
        {
            var window = GetWindow<AnimationPreviewerWindow>("动作预览");
            window.minSize = new UnityEngine.Vector2(400, 400);
            window.Show();
        }

        private AnimationPreviewerDataEditor previewerEditor;
        private AnimationPreviewerData previewerData;
        private string animationName = string.Empty;
        private int playFrameIndex = 0;
        private float animationDuration = 0;
        private bool isFrameStepMode = false;

        private void OnEnable()
        {
            previewerData = CreateInstance<AnimationPreviewerData>();
            previewerEditor = UnityEditor.Editor.CreateEditor(previewerData) as AnimationPreviewerDataEditor;

            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        }

        private void OnDestroy()
        {
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
            if(previewerEditor != null)
            {
                UnityEditor.Editor.DestroyImmediate(previewerEditor);
                previewerEditor = null;
            }
        }

        private void OnDomainUnload(object sender, EventArgs e)
        {
            OnDestroy();
        }

        private void OnGUI()
        {
            SkeletonDataAsset dataAsset = EditorGUILayout.ObjectField("选择资源", previewerData.skeletonDataAsset, typeof(SkeletonDataAsset), true) as SkeletonDataAsset;
            if(dataAsset != previewerData.skeletonDataAsset)
            {
                previewerData.skeletonDataAsset = dataAsset;
            }

            if (previewerData.skeletonDataAsset == null)
                return;

            OnDrawAnimationsGUI();

            if (string.IsNullOrEmpty(animationName))
                return;

            OnDrawFrameSliderGUI();

            (previewerEditor as AnimationPreviewerDataEditor).SetDrawRect(new Rect(0, 64, position.width, position.height - 64));
            previewerEditor?.OnInspectorGUI();
             
            Repaint();
        }

        private void OnDrawFrameSliderGUI()
        {
            //if (UnityEngine.Event.current.type == EventType.Layout || UnityEngine.Event.current.type == EventType.Repaint)
            {
                EditorGUILayout.BeginHorizontal();

                int frameCount = (int)(animationDuration / (1f / 33f));

                if (isFrameStepMode)
                    playFrameIndex = EditorGUILayout.IntSlider("逐帧播放", playFrameIndex, 0, frameCount);
                else
                    EditorGUILayout.IntSlider("逐帧播放", playFrameIndex, 0, frameCount);

                FontStyle bakeStyle = GUI.skin.button.fontStyle;
                Color bakeNormalColor = GUI.skin.button.normal.textColor;
                Color bakeHoverColor = GUI.skin.button.hover.textColor;
                if (isFrameStepMode)
                {
                    GUI.skin.button.fontStyle = FontStyle.Bold;
                    GUI.skin.button.normal.textColor = Color.red;
                    GUI.skin.button.hover.textColor = Color.red;
                }
                if (GUILayout.Button("逐帧", GUILayout.Width(36)))
                {
                    isFrameStepMode = true;
                }

                if (!isFrameStepMode)
                {
                    GUI.skin.button.fontStyle = FontStyle.Bold;
                    GUI.skin.button.normal.textColor = Color.red;
                    GUI.skin.button.hover.textColor = Color.red;
                }
                else
                {
                    GUI.skin.button.fontStyle = bakeStyle;
                    GUI.skin.button.normal.textColor = bakeNormalColor;
                    GUI.skin.button.hover.textColor = Color.red;
                }

                if (GUILayout.Button("自动", GUILayout.Width(36)))
                {
                    isFrameStepMode = false;
                    previewerEditor.CancelPause();
                }

                GUI.skin.button.fontStyle = bakeStyle;
                GUI.skin.button.normal.textColor = bakeNormalColor;
                GUI.skin.button.hover.textColor = bakeHoverColor;

                EditorGUILayout.EndHorizontal();
            }

            if (isFrameStepMode)
            {
                previewerEditor.SetPlayAndPauseTime(playFrameIndex * (1f / 33f));
            }
        }

        private void OnDrawAnimationsGUI()
        {
            if (previewerData == null || previewerData.skeletonDataAsset == null)
                return;

            AnimationPreviewerDataEditor pe = previewerEditor as AnimationPreviewerDataEditor;
            var activeTrack = pe.previewer.ActiveTrack;
            SkeletonData data = previewerData.skeletonDataAsset.GetSkeletonData(true);

            string newAnimationName = string.Empty;
            string[] arrNames = new string[data.Animations.Count];
            int index = 0;
            int selectIndex = -1;
            foreach (Animation animation in data.Animations)
            {
                arrNames[index] = animation.Name;
                if(animationName.Equals(animation.Name))
                {
                    selectIndex = index;
                }
                index++;
            }

            bool newAnim = false;
            if(selectIndex == -1 || selectIndex >= data.Animations.Count - 1)
            {
                selectIndex = 0;
                newAnim = true;
            }
            int newIndex = EditorGUILayout.Popup("选择动画", selectIndex, arrNames);
            if(newIndex != selectIndex)
            {
                selectIndex = newIndex;
                newAnim = true;
            }

            if(arrNames.Length == 0)
            {
                animationName = string.Empty;
            }
            else if (newAnim && arrNames.Length > 0)
            {
                animationDuration = data.Animations.ToArray()[selectIndex].Duration;
                animationName = arrNames[selectIndex];
                pe.ChangeAnimation(animationName);
            }
        }
    }
}

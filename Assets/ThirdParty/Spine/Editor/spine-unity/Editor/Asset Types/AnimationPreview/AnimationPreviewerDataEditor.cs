/**************************************************************************    
文　　件：SkeletonDataAssetPreviewerWindow
作　　者：郑秀程
创建时间：2021/1/31 10:10:46
描　　述：
***************************************************************************/
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{
    [CustomEditor(typeof(AnimationPreviewerData))]
    public class AnimationPreviewerDataEditor : UnityEditor.Editor
    {
        public AnimationPreviewer previewer = new AnimationPreviewer();

        private string animationName = "Attack";
        private SkeletonDataAsset skeletonDataAsset;
        private SkeletonData skeletonData;
        private bool isTargetChanged = false;
        private Rect drawRect;

        private void OnEnable() { HandleOnEnablePreview(); }
        private void OnDestroy() { HandleOnDestroyPreview(); }

        public void SetDrawRect(Rect r)
        {
            drawRect = r;
        }

        public void ChangeAnimation(string name)
        {
            animationName = name;
            isTargetChanged = true;
        }

        public void CancelPause()
        {
            previewer.CancelPause();
        }

        public void SetPlayAndPauseTime(float time)
        {
            previewer.SetPauseTime(time);
        }

        public override void OnInspectorGUI()
        {
            AnimationPreviewerData previewerData = target as AnimationPreviewerData;
            if(previewerData.skeletonDataAsset != skeletonDataAsset)
            {
                skeletonDataAsset = previewerData.skeletonDataAsset;
                skeletonData = skeletonDataAsset.GetSkeletonData(true);
                isTargetChanged = true;
            }

            if (skeletonDataAsset == null)
            {
                EditorGUILayout.HelpBox("SkeletonDataAsset is missing.", MessageType.Error);
                return;
            }
            else if (string.IsNullOrEmpty(animationName))
            {
                EditorGUILayout.HelpBox("No animation selected.", MessageType.Warning);
                return;
            }

            if (skeletonDataAsset == null || skeletonData == null)
                return;

            //查找动画
            Animation animation = null;
            animation = skeletonData.FindAnimation(animationName);

            bool animationNotFound = (animation == null);

            if (animationNotFound)
            {
                EditorGUILayout.HelpBox(string.Format("Animation named {0} was not found for this Skeleton.", animationName), MessageType.Warning);
                return;
            }

            if (isTargetChanged)
            {
                isTargetChanged = false;

                previewer.Clear();
                previewer.Initialize(Repaint, skeletonDataAsset, LastSkinName);
                previewer.ClearAnimationSetupPose();

                if (!string.IsNullOrEmpty(animationName))
                    previewer.PlayPauseAnimation(animationName, true);
            }

            OnPreviewSettings();
            OnInteractivePreviewGUI(drawRect, GUIStyle.none);
        }

        #region Preview Handlers
        string TargetAssetGUID { get { return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(skeletonDataAsset)); } }
        string LastSkinKey { get { return TargetAssetGUID + "_lastSkin"; } }
        string LastSkinName { get { return EditorPrefs.GetString(LastSkinKey, ""); } }

        void HandleOnEnablePreview()
        {
            if (skeletonDataAsset != null && skeletonDataAsset.skeletonJSON == null)
                return;

            previewer.Initialize(this.Repaint, skeletonDataAsset, LastSkinName);
            previewer.PlayPauseAnimation(animationName, true);
            previewer.OnSkinChanged -= HandleOnSkinChanged;
            previewer.OnSkinChanged += HandleOnSkinChanged;
            EditorApplication.update -= previewer.HandleEditorUpdate;
            EditorApplication.update += previewer.HandleEditorUpdate;
        }

        private void HandleOnSkinChanged(string skinName)
        {
            EditorPrefs.SetString(LastSkinKey, skinName);
            previewer.PlayPauseAnimation(animationName, true);
        }

        void HandleOnDestroyPreview()
        {
            EditorApplication.update -= previewer.HandleEditorUpdate;
            previewer.OnDestroy();
        }

        override public bool HasPreviewGUI()
        {
            return skeletonDataAsset != null && skeletonData != null;
        }

        override public void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            previewer.Initialize(this.Repaint, skeletonDataAsset);
            previewer.HandleInteractivePreviewGUI(r, background);
        }

        public override GUIContent GetPreviewTitle() { return SpineInspectorUtility.TempContent("Preview"); }
        //public override void OnPreviewSettings() { previewer.HandleDrawSettings(); }
        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height) { return previewer.GetStaticPreview(width, height); }
        #endregion
    }
}

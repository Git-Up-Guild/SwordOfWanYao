
#define SPINE_SKELETON_MECANIM
#if (UNITY_2017_4 || UNITY_2018_1_OR_NEWER)
#define SPINE_UNITY_2018_PREVIEW_API
#endif

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor
{
    using Event = UnityEngine.Event;
    using Icons = SpineEditorUtilities.Icons;

    public class AnimationPreviewer
    {
        Color OriginColor = new Color(0.3f, 0.3f, 0.3f, 1);
        static readonly int SliderHash = "Slider".GetHashCode();

        SkeletonDataAsset skeletonDataAsset;
        SkeletonData skeletonData;

        SkeletonAnimation skeletonAnimation;
        GameObject previewGameObject;
        internal bool requiresRefresh;
        float animationLastTime;

        static float CurrentTime { get { return (float)EditorApplication.timeSinceStartup; } }

        Action Repaint;
        public event Action<string> OnSkinChanged;

        Texture previewTexture;
        PreviewRenderUtility previewRenderUtility;
        Camera PreviewUtilityCamera
        {
            get
            {
                if (previewRenderUtility == null) return null;
#if UNITY_2017_1_OR_NEWER
                return previewRenderUtility.camera;
#else
				return previewRenderUtility.m_Camera;
#endif
            }
        }

        static Vector3 lastCameraPositionGoal;
        static float lastCameraOrthoGoal;
        float cameraOrthoGoal = 1;
        Vector3 cameraPositionGoal = new Vector3(0, 0, -10);
        double cameraAdjustEndFrame = 0;

        List<Spine.Event> currentAnimationEvents = new List<Spine.Event>();
        List<float> currentAnimationEventTimes = new List<float>();
        List<SpineEventTooltip> currentAnimationEventTooltips = new List<SpineEventTooltip>();

        private bool loop = true;

        public bool IsValid { get { return skeletonAnimation != null && skeletonAnimation.valid; } }

        public Skeleton Skeleton { get { return IsValid ? skeletonAnimation.Skeleton : null; } }

        public float TimeScale
        {
            get { return IsValid ? skeletonAnimation.timeScale : 1f; }
            set { if (IsValid) skeletonAnimation.timeScale = value; }
        }

        public bool IsPlayingAnimation
        {
            get
            {
                if (!IsValid) return false;
                var currentTrack = skeletonAnimation.AnimationState.GetCurrent(0);
                return currentTrack != null && currentTrack.TimeScale > 0;
            }
        }

        public TrackEntry ActiveTrack { get { return IsValid ? skeletonAnimation.AnimationState.GetCurrent(0) : null; } }

        public Vector3 PreviewCameraPosition
        {
            get { return PreviewUtilityCamera.transform.position; }
            set { PreviewUtilityCamera.transform.position = value; }
        }

        public void CancelPause()
        {
            var trackEntity = skeletonAnimation.AnimationState.GetCurrent(0);
            trackEntity.AnimationStart = 0;
            trackEntity.AnimationEnd = trackEntity.Animation.Duration;
        }

        public void SetPauseTime(float time)
        {
            if (skeletonAnimation == null)
                return;

            if(time < 0)
            {
                time = 0;
            }

            var trackEntity = skeletonAnimation.AnimationState.GetCurrent(0);
            trackEntity.AnimationStart = time;
            trackEntity.AnimationEnd = time;
        }

        private void HandleDrawSettings(Rect r)
        {
            const float SliderSnap = 0.05f; 
            const float SliderMin = 0f;
            const float SliderMax = 2f;

            if (IsValid)
            {
                Rect loopRect = new Rect(r);
                loopRect.y += 25;
                loopRect.x += 4;
                loopRect.width = 16;
                loopRect.height = 16;
                bool loop2 = EditorGUI.Toggle(loopRect, loop); 
                if(loop2 != loop)
                {
                    loop = loop2;
                    ActiveTrack.Loop = loop;
                }

                loopRect.x = r.x + 20;
                loopRect.width = 32;
                loopRect.height = 16;
                EditorGUI.LabelField(loopRect, "循环");

                Rect sliderRect = new Rect(r);
                sliderRect.y = r.y + 25;
                sliderRect.x = r.x + 50;
                sliderRect.height = 16;
                sliderRect.width = r.width - 52;
                float timeScale = EditorGUI.Slider(sliderRect, TimeScale, SliderMin, SliderMax);
                timeScale = Mathf.RoundToInt(timeScale / SliderSnap) * SliderSnap;
                if(TimeScale != timeScale)
                {
                    TimeScale = timeScale;
                    Repaint();
                }
            }
        }

        public void HandleEditorUpdate()
        {
            AdjustCamera();
            if (IsPlayingAnimation)
            {
                RefreshOnNextUpdate();
                Repaint();
            }
            else if (requiresRefresh)
            {
                Repaint();
            }
        }

        public void Initialize(Action repaintCallback, SkeletonDataAsset skeletonDataAsset, string skinName = "")
        {
            if (skeletonDataAsset == null) return;
            if (skeletonDataAsset.GetSkeletonData(false) == null)
            {
                DestroyPreviewGameObject();
                return;
            }

            this.Repaint = repaintCallback;
            this.skeletonDataAsset = skeletonDataAsset;
            this.skeletonData = skeletonDataAsset.GetSkeletonData(false);

            if (skeletonData == null)
            {
                DestroyPreviewGameObject();
                return;
            }

            const int PreviewLayer = 30;
            const int PreviewCameraCullingMask = 1 << PreviewLayer;

            if (previewRenderUtility == null)
            {
                previewRenderUtility = new PreviewRenderUtility(true);
                animationLastTime = CurrentTime;

                {
                    var c = this.PreviewUtilityCamera;
                    c.orthographic = true;
                    c.cullingMask = PreviewCameraCullingMask;
                    c.nearClipPlane = 0.01f;
                    c.farClipPlane = 1000f;
                    c.orthographicSize = lastCameraOrthoGoal;
                    c.transform.position = lastCameraPositionGoal;
                }

                DestroyPreviewGameObject();
            }

            if (previewGameObject == null)
            {
                try
                {
                    previewGameObject = EditorInstantiation.InstantiateSkeletonAnimation(skeletonDataAsset, skinName, useObjectFactory: false).gameObject;

                    if (previewGameObject != null)
                    {
                        previewGameObject.hideFlags = HideFlags.HideAndDontSave;
                        previewGameObject.layer = PreviewLayer;
                        skeletonAnimation = previewGameObject.GetComponent<SkeletonAnimation>();
                        skeletonAnimation.initialSkinName = skinName;
                        skeletonAnimation.LateUpdate();
                        previewGameObject.GetComponent<Renderer>().enabled = false;

#if SPINE_UNITY_2018_PREVIEW_API
                        previewRenderUtility.AddSingleGO(previewGameObject);
#endif
                    }

                    if (this.ActiveTrack != null) cameraAdjustEndFrame = EditorApplication.timeSinceStartup + skeletonAnimation.AnimationState.GetCurrent(0).Alpha;
                    AdjustCameraGoals();
                }
                catch
                {
                    DestroyPreviewGameObject();
                }

                RefreshOnNextUpdate();
            }
        }

        public void HandleInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (requiresRefresh)
                {
                    previewRenderUtility.BeginPreview(r, background);
                    DoRenderPreview(true);
                    previewTexture = previewRenderUtility.EndPreview();
                    requiresRefresh = false;
                }
                if (previewTexture != null)
                    GUI.DrawTexture(r, previewTexture, ScaleMode.StretchToFill, false);
            }

            DrawTimeBar(r);
            HandleMouseScroll(r);
            HandleDrawSettings(r);
        }

        public Texture2D GetStaticPreview(int width, int height)
        {
            var c = this.PreviewUtilityCamera;
            if (c == null)
                return null;

            RefreshOnNextUpdate();
            AdjustCameraGoals();
            c.orthographicSize = cameraOrthoGoal / 2;
            c.transform.position = cameraPositionGoal;
            previewRenderUtility.BeginStaticPreview(new Rect(0, 0, width, height));
            DoRenderPreview(false);
            var tex = previewRenderUtility.EndStaticPreview();

            return tex;
        }

        public void DoRenderPreview(bool drawHandles)
        {
            if (this.PreviewUtilityCamera.activeTexture == null || this.PreviewUtilityCamera.targetTexture == null)
                return;

            GameObject go = previewGameObject;
            if (requiresRefresh && go != null)
            {
                var renderer = go.GetComponent<Renderer>();
                renderer.enabled = true;


                if (!EditorApplication.isPlaying)
                {
                    float current = CurrentTime;
                    float deltaTime = (current - animationLastTime);
                    skeletonAnimation.Update(deltaTime);
                    animationLastTime = current;
                    skeletonAnimation.LateUpdate();
                }

                var thisPreviewUtilityCamera = this.PreviewUtilityCamera;

                if (drawHandles)
                {
                    Handles.SetCamera(thisPreviewUtilityCamera);
                    Handles.color = OriginColor;

                    // Draw Cross
                    float scale = skeletonDataAsset.scale;
                    float cl = 1000 * scale;
                    Handles.DrawLine(new Vector3(-cl, 0), new Vector3(cl, 0));
                    Handles.DrawLine(new Vector3(0, cl), new Vector3(0, -cl));
                }

                thisPreviewUtilityCamera.Render();

                if (drawHandles)
                {
                    Handles.SetCamera(thisPreviewUtilityCamera);
                    SpineHandles.DrawBoundingBoxes(skeletonAnimation.transform, skeletonAnimation.skeleton);
                    if (SkeletonDataAssetInspector.showAttachments)
                        SpineHandles.DrawPaths(skeletonAnimation.transform, skeletonAnimation.skeleton);
                }

                renderer.enabled = false;
            }
        }

        public void AdjustCamera()
        {
            if (previewRenderUtility == null)
                return;

            if (CurrentTime < cameraAdjustEndFrame)
                AdjustCameraGoals();

            lastCameraPositionGoal = cameraPositionGoal;
            lastCameraOrthoGoal = cameraOrthoGoal;

            var c = this.PreviewUtilityCamera;
            float orthoSet = Mathf.Lerp(c.orthographicSize, cameraOrthoGoal, 0.1f);

            c.orthographicSize = orthoSet;

            float dist = Vector3.Distance(c.transform.position, cameraPositionGoal);
            if (dist > 0f)
            {
                Vector3 pos = Vector3.Lerp(c.transform.position, cameraPositionGoal, 0.1f);
                pos.x = 0;
                c.transform.position = pos;
                c.transform.rotation = Quaternion.identity;
                RefreshOnNextUpdate();
            }
        }

        void AdjustCameraGoals()
        {
            if (previewGameObject == null) return;

            Bounds bounds = previewGameObject.GetComponent<Renderer>().bounds;
            cameraOrthoGoal = bounds.size.y;
            cameraPositionGoal = bounds.center + new Vector3(0, 0, -10f);
        }

        void HandleMouseScroll(Rect position)
        {
            Event current = Event.current;
            int controlID = GUIUtility.GetControlID(SliderHash, FocusType.Passive);
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.ScrollWheel:
                    if (position.Contains(current.mousePosition))
                    {
                        cameraOrthoGoal += current.delta.y * 0.06f;
                        cameraOrthoGoal = Mathf.Max(0.01f, cameraOrthoGoal);
                        GUIUtility.hotControl = controlID;
                        current.Use();
                    }
                    break;
            }
        }

        public void RefreshOnNextUpdate()
        {
            requiresRefresh = true;
        }

        public void ClearAnimationSetupPose()
        {
            if (skeletonAnimation == null)
            {
                Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
            }

            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.Skeleton.SetToSetupPose();
        }

        public void PlayPauseAnimation(string animationName, bool l)
        {
            if (skeletonData == null) return;

            if (skeletonAnimation == null)
            {
                //Debug.LogWarning("Animation was stopped but preview doesn't exist. It's possible that the Preview Panel is closed.");
                return;
            }

            if (!skeletonAnimation.valid) return;

            if (string.IsNullOrEmpty(animationName))
            {
                skeletonAnimation.Skeleton.SetToSetupPose();
                skeletonAnimation.AnimationState.ClearTracks();
                return;
            }

            var targetAnimation = skeletonData.FindAnimation(animationName);
            if (targetAnimation != null)
            {
                var currentTrack = this.ActiveTrack;
                bool isEmpty = (currentTrack == null);
                bool isNewAnimation = isEmpty || currentTrack.Animation != targetAnimation;

                var skeleton = skeletonAnimation.Skeleton;
                var animationState = skeletonAnimation.AnimationState;

                if (isEmpty)
                {
                    skeleton.SetToSetupPose();
                    animationState.SetAnimation(0, targetAnimation, loop);
                }
                else
                {
                    bool sameAnimation = (currentTrack.Animation == targetAnimation);
                    if (sameAnimation)
                    {
                        currentTrack.TimeScale = (currentTrack.TimeScale == 0) ? 1f : 0f; // pause/play
                    }
                    else
                    {
                        currentTrack.TimeScale = 1f;
                        animationState.SetAnimation(0, targetAnimation, loop);
                    } 
                }

                if (isNewAnimation)
                {
                    currentAnimationEvents.Clear();
                    currentAnimationEventTimes.Clear();
                    foreach (Timeline timeline in targetAnimation.Timelines)
                    {
                        var eventTimeline = timeline as EventTimeline;
                        if (eventTimeline != null)
                        {
                            for (int i = 0; i < eventTimeline.Events.Length; i++)
                            {
                                currentAnimationEvents.Add(eventTimeline.Events[i]);
                                currentAnimationEventTimes.Add(eventTimeline.Frames[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogFormat("The Spine.Animation named '{0}' was not found for this Skeleton.", animationName);
            }
        }

        void DrawTimeBar(Rect r)
        {
            if (skeletonAnimation == null)
                return;

            Rect barRect = new Rect(r);
            barRect.height = 20;

            GUI.Box(barRect, "");

            Rect lineRect = new Rect(barRect);
            float lineRectWidth = lineRect.width;
            TrackEntry t = skeletonAnimation.AnimationState.GetCurrent(0);

            if (t != null)
            {
                //int loopCount = (int)(t.TrackTime / t.TrackEnd);
                //float currentTime = t.TrackTime - (t.TrackEnd * loopCount);
                //float normalizedTime = currentTime / t.Animation.Duration;
                float normalizedTime = t.AnimationTime / t.Animation.Duration;
                float wrappedTime = normalizedTime % 1f;

                //Debug.Log("TrackTime=" + t.TrackTime + ", AnimationTime=" + t.AnimationTime);

                lineRect.x = barRect.x + (lineRectWidth * wrappedTime) - 0.5f;
                lineRect.width = 2;

                GUI.color = Color.red;
                GUI.DrawTexture(lineRect, EditorGUIUtility.whiteTexture);
                GUI.color = Color.white;

                currentAnimationEventTooltips = currentAnimationEventTooltips ?? new List<SpineEventTooltip>();
                currentAnimationEventTooltips.Clear();

                 if(!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    for (int i = 0; i < currentAnimationEvents.Count; i++)
                    {
                        float eventTime = currentAnimationEventTimes[i];
                        var userEventIcon = Icons.userEvent;
                        var evRect = new Rect(barRect)
                        {
                            x = Mathf.Max(((eventTime / t.Animation.Duration) * lineRectWidth) - (userEventIcon.width / 2), barRect.x),
                            y = barRect.y + userEventIcon.height - 10,
                            width = userEventIcon.width,
                            height = userEventIcon.height
                        };
                        GUI.DrawTexture(evRect, userEventIcon);

                        Event ev = Event.current;
                        if (ev.type == EventType.Repaint)
                        {
                            if (evRect.Contains(ev.mousePosition))
                            {
                                string eventName = currentAnimationEvents[i].Data.Name;
                                Rect tooltipRect = new Rect(evRect)
                                {
                                    width = EditorStyles.helpBox.CalcSize(new GUIContent(eventName)).x
                                };
                                tooltipRect.y -= 4;
                                tooltipRect.y -= tooltipRect.height * currentAnimationEventTooltips.Count; // Avoid several overlapping tooltips.
                                tooltipRect.x += 4;

                                // Handle tooltip overflowing to the right.
                                float rightEdgeOverflow = (tooltipRect.x + tooltipRect.width) - (barRect.x + barRect.width);
                                if (rightEdgeOverflow > 0)
                                    tooltipRect.x -= rightEdgeOverflow;

                                currentAnimationEventTooltips.Add(new SpineEventTooltip { rect = tooltipRect, text = eventName });
                            }
                        }
                    }
                }

                // Draw tooltips.
                for (int i = 0; i < currentAnimationEventTooltips.Count; i++)
                {
                    GUI.Label(currentAnimationEventTooltips[i].rect, currentAnimationEventTooltips[i].text, EditorStyles.helpBox);
                    GUI.tooltip = currentAnimationEventTooltips[i].text;
                }
            }
        }

        public void OnDestroy()
        {
            DisposePreviewRenderUtility();
            DestroyPreviewGameObject();
        }

        public void Clear()
        {
            DisposePreviewRenderUtility();
            DestroyPreviewGameObject();
        }

        void DisposePreviewRenderUtility()
        {
            if (previewRenderUtility != null)
            {
                previewRenderUtility.Cleanup();
                previewRenderUtility = null;
            }
        }

        void DestroyPreviewGameObject()
        {
            if (previewGameObject != null)
            {
                GameObject.DestroyImmediate(previewGameObject);
                previewGameObject = null;
            }
        }

        internal struct SpineEventTooltip
        {
            public Rect rect;
            public string text;
        }
    }

}

/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2019_1_OR_NEWER
#define TIMELINE_HAS_CLIPEDITOR_CLASS
#endif

#if TIMELINE_HAS_CLIPEDITOR_CLASS

using System;
using UnityEditor;
using Spine.Unity.Playables;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Spine.Unity.Editor
{
    [CustomTimelineEditor(typeof(DynamicSpineAnimationStateClip))]
    public class DynamicSpineAnimationStateClipEditor : ClipEditor
    {
        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            DynamicSpineAnimationStateBehaviour settings = GetClipSetting(clip);

            if (null != settings)
            {
                if (null != settings.templateReferenceAsset
                    && null != settings.templateReferenceAsset.GetAnimation())
                {
                    var ani = settings.templateReferenceAsset.GetAnimation();
                    settings.aniName = ani.Name;
                    clip.displayName = ani.Name;
                    settings.aniDuration = ani.Duration;
                }
                clip.duration = settings.aniDuration;
            }
        }

        public override void OnClipChanged(TimelineClip clip)
        {
            SetDisplayName(clip);
        }

        protected void SetDisplayName(TimelineClip clip)
        {
            DynamicSpineAnimationStateBehaviour settings = GetClipSetting(clip);
            string aniName = null;
            if (null != settings)
            {
                aniName = settings.aniName;
            }
            clip.displayName = String.IsNullOrEmpty(aniName) ? "ani" : aniName;
        }

        private DynamicSpineAnimationStateBehaviour GetClipSetting(TimelineClip clip)
        {
            var stateClip = (DynamicSpineAnimationStateClip)clip.asset;
            if (stateClip != null)
            {
                var settings = stateClip.template;
                if (settings != null)
                {
                    return settings;
                }
            }

            return null;
        }
    }
}

#endif // TIMELINE_HAS_CLIPEDITOR_CLASS
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

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Spine.Unity.Playables {
	[TrackColor(0.9960785f, 0.2509804f, 0.003921569f)]
	[TrackClipType(typeof(DynamicSpineAnimationStateClip))]
	[TrackBindingType(typeof(SkeletonGraphic))]
	public class DynamicSpineAnimationStateGraphicTrack : TrackAsset {
		public int trackIndex = 0;
		[Tooltip("轨道信息 格式 a_b a为1是攻击者2是防御者3是第三方 b是第几个")]
		public string trackParam;

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
			var scriptPlayable = ScriptPlayable<DynamicSpineAnimationStateMixerBehaviour>.Create(graph, inputCount);
			var mixerBehaviour = scriptPlayable.GetBehaviour();
			mixerBehaviour.trackIndex = this.trackIndex;
			return scriptPlayable;
		}
	}
}

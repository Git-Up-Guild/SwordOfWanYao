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

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Spine.Unity.Playables {

	using Animation = Spine.Animation;

	[Serializable]
	public class DynamicSpineAnimationStateBehaviour : PlayableBehaviour
	{
		public AnimationReferenceAsset templateReferenceAsset;
		public string aniName;
		public bool loop;
		public double aniDuration = 1f;
		
		// Mix Properties
		public bool customMixDuration = false;
		public bool useBlendDuration = true;
		[SerializeField]
		#pragma warning disable 414
		private bool isInitialized = false; // required to read preferences values from editor side.
		#pragma warning restore 414
		public float mixDuration = 0.05f;
		public bool holdPrevious = false;

		[Range(0, 1f)]
		public float attachmentThreshold = 0.5f;

		[Range(0, 1f)]
		public float eventThreshold = 0.5f;

		[Range(0, 1f)]
		public float drawOrderThreshold = 0.5f;
		
		private Animation animation = null;

		public Animation GetAnimation()
        {
			if(null!= templateReferenceAsset&& animation==null)
            {
				animation = templateReferenceAsset.GetAnimation(); 

			}
			return animation; 
		}

		public double GetDuration()
		{
			return aniDuration;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			
		}


	}

}

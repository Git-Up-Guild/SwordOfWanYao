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
using XGame.Poolable;

namespace Spine {
	public class PathConstraintData : ConstraintData,IPoolable
	{
		internal ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal SlotData target;
		internal PositionMode positionMode;
		internal SpacingMode spacingMode;
		internal RotateMode rotateMode;
		internal float offsetRotation;
		internal float position, spacing, rotateMix, translateMix;


		public PathConstraintData()
        {

        }
		public PathConstraintData (string name) : base(name) {
		}

		public void Initalize(string name)
		{
			base.Initalize(name);
		}

		public ExposedList<BoneData> Bones { get { return bones; } }
		public SlotData Target { get { return target; } set { target = value; } }
		public PositionMode PositionMode { get { return positionMode; } set { positionMode = value; } }
		public SpacingMode SpacingMode { get { return spacingMode; } set { spacingMode = value; } }
		public RotateMode RotateMode { get { return rotateMode; } set { rotateMode = value; } }
		public float OffsetRotation { get { return offsetRotation; } set { offsetRotation = value; } }
		public float Position { get { return position; } set { position = value; } }
		public float Spacing { get { return spacing; } set { spacing = value; } }
		public float RotateMix { get { return rotateMix; } set { rotateMix = value; } }
		public float TranslateMix { get { return translateMix; } set { translateMix = value; } }

        public bool Create()
        {
			return true;
        }

        public void Init(object context = null)
        {
           
        }

        public void Release()
        {
           
        }

        public void Reset()
		{
			bones.Clear(false);

		}
	}

	public enum PositionMode {
		Fixed, Percent
	}

	public enum SpacingMode {
		Length, Fixed, Percent
	}

	public enum RotateMode {
		Tangent, Chain, ChainScale
	}

	
}

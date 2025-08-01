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
	/// <summary>Stores the setup pose values for an Event.</summary>
	public class EventData : IPoolable
	{
		internal string name;

		/// <summary>The name of the event, which is unique across all events in the skeleton.</summary>
		public string Name { get { return name; } }
		public int Int { get; set; }
		public float Float { get; set; }
		public string @String { get; set; }

		public string AudioPath { get; set; }
		public float Volume { get; set; }
		public float Balance { get; set; }

		public EventData (string name) {
			Initalize(name);
		}

		public EventData()
		{
		
		}

		public void Initalize(string name)
		{
			if (name == null) throw new ArgumentNullException("name", "name cannot be null.");
			this.name = name;
		}

		override public string ToString () {
			return Name;
		}

        public bool Create()
        {
			return true;
        }

        public void Init(object context = null)
        {
           
        }

        public void Reset()
        {
            
        }

        public void Release()
        {
           
        }
    }
}

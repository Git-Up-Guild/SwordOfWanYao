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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XGame.Poolable;
using XGame;

#if UNITY_EDITOR
using UnityEditor;
#endif

using CompatibilityProblemInfo = Spine.Unity.SkeletonDataCompatibility.CompatibilityProblemInfo;

namespace Spine.Unity {

	[CreateAssetMenu(fileName = "New SkeletonDataAsset", menuName = "Spine/SkeletonData Asset")]
	public class SkeletonDataAsset : ScriptableObject {
		#region Inspector
		public AtlasAssetBase[] atlasAssets = new AtlasAssetBase[0];

		#if SPINE_TK2D
		public tk2dSpriteCollectionData spriteCollection;
		public float scale = 1f;
		#else
		public float scale = 0.01f;
#endif

#if UNITY_EDITOR

		public TextAsset skeletonJSON;
#else

#endif
		public string skeletonJsonPath;

		public bool isUpgradingBlendModeMaterials = false;
		public BlendModeMaterials blendModeMaterials = new BlendModeMaterials();

		[Tooltip("Use SkeletonDataModifierAssets to apply changes to the SkeletonData after being loaded, such as apply blend mode Materials to Attachments under slots with special blend modes.")]
		public List<SkeletonDataModifierAsset> skeletonDataModifiers = new List<SkeletonDataModifierAsset>();

		[SpineAnimation(includeNone: false)]
		public string[] fromAnimation = new string[0];
		[SpineAnimation(includeNone: false)]
		public string[] toAnimation = new string[0];
		public float[] duration = new float[0];
		public float defaultMix;
		public RuntimeAnimatorController controller;

		public bool IsLoaded { get { return this.skeletonData != null; } }

		//private int refCount = 0;

		private HashSet<object> m_refHash = new HashSet<object>();

		#endregion

		SkeletonData skeletonData;
		AnimationStateData stateData;

#region Runtime Instantiation
		/// <summary>
		/// Creates a runtime SkeletonDataAsset.</summary>
		public static SkeletonDataAsset CreateRuntimeInstance (TextAsset skeletonDataFile, AtlasAssetBase atlasAsset, bool initialize, float scale = 0.01f) {
			return CreateRuntimeInstance(skeletonDataFile, new [] {atlasAsset}, initialize, scale);
		}

		/// <summary>
		/// Creates a runtime SkeletonDataAsset.</summary>
		public static SkeletonDataAsset CreateRuntimeInstance (TextAsset skeletonDataFile, AtlasAssetBase[] atlasAssets, bool initialize, float scale = 0.01f) {
			SkeletonDataAsset skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
			skeletonDataAsset.Clear();
#if UNITY_EDITOR

			skeletonDataAsset.skeletonJSON = skeletonDataFile;
			skeletonDataAsset.skeletonJsonPath = AssetDatabase.GetAssetPath(skeletonDataAsset.skeletonJSON);
			skeletonDataAsset.skeletonJsonPath = skeletonDataAsset.skeletonJsonPath.Replace("Assets/", "");
#else

#endif
			skeletonDataAsset.atlasAssets = atlasAssets;
			skeletonDataAsset.scale = scale;

			if (initialize)
				skeletonDataAsset.GetSkeletonData(true);

			return skeletonDataAsset;
		}
#endregion

		/// <summary>Clears the loaded SkeletonData and AnimationStateData. Use this to force a reload for the next time GetSkeletonData is called.</summary>
		public void Clear () {

			skeletonData = null;
			stateData = null;
		}

		public void Reset()
        {
			if (null != skeletonData)
			{
				//Debug.LogError("Release skeletonData" + skeletonData.name);
				SpineObjectPoolFacade.Instance().Push(skeletonData);
				skeletonData = null;
			}

			if(null!= stateData)
            {
				SpineObjectPoolFacade.Instance().Push(stateData);
				stateData = null;
			}

			Clear();
		}

#if UNITY_EDITOR

		private bool m_isInEditor = false;

		public void SetEditor(bool isInEditor)
        {
			m_isInEditor = isInEditor;

		}
#endif

		public void IncRef(object obj)
        {

			//后面加宏定义，不用判断是否包含
			if(m_refHash.Contains(obj))
            {
				/*
				if (Application.isPlaying)
					Debug.LogError("重复增加引用:  " + skeletonJsonPath);
				*/
				return;
            }

			m_refHash.Add(obj);

			//++refCount;
			//Debug.LogError("hashcode = "+this.GetHashCode()+" ,"+skeletonJsonPath+",增加引用次数 refCount= " + refCount);

		}

		public void DecRef(object obj)
        {
			//后面加宏定义不用判断是否存在
			if (m_refHash.Contains(obj)==false)
			{
				/*
                if(Application.isPlaying)
				Debug.LogError("重复消除引用:  " + skeletonJsonPath);
				*/
				return;

				//Debug.LogError("hashcode = " + this.GetHashCode() + " ," + skeletonJsonPath + ",重复增加引用");
				//return;
			}

			m_refHash.Remove(obj);

			//if (--refCount<=0)
			if (m_refHash.Count==0)
            {
#if UNITY_EDITOR
				if(m_isInEditor)
                {
					return ;
                }
#endif
				Reset();
            }

			//Debug.LogError("hashcode = " + this.GetHashCode() + " ," + skeletonJsonPath + ",降低引用次数 refCount= " + refCount);
		}

		public AnimationStateData GetAnimationStateData () {
			if (stateData != null)
				return stateData;
			GetSkeletonData(false);
			return stateData;
		}

		public void DecodeSkeletonData(byte[] data)
        {
			GetSkeletonData(false, data);

		}


#if !SPINE_TK2D
	static RegionlessAttachmentLoader s_RegionlessAttachmentLoader = new RegionlessAttachmentLoader();
	static  AtlasAttachmentLoader s_AtlasAttachmentLoader = new AtlasAttachmentLoader();
#endif
		/// <summary>Loads, caches and returns the SkeletonData from the skeleton data file. Returns the cached SkeletonData after the first time it is called. Pass false to prevent direct errors from being logged.</summary>
		public SkeletonData GetSkeletonData (bool quiet, byte[] data = null) {



			//skeletonJSON = null;
#if UNITY_EDITOR

			if (Application.isPlaying == false)
            {
				if (data == null && skeletonJSON == null)
				{
					if (!quiet)
						Debug.LogError("Skeleton JSON file not set for SkeletonData asset: " + name, this);
					Clear();
					return null;
				}

			}
			
#else
#endif

			// Disabled to support attachmentless/skinless SkeletonData.
			//			if (atlasAssets == null) {
			//				atlasAssets = new AtlasAsset[0];
			//				if (!quiet)
			//					Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
			//				Clear();
			//				return null;
			//			}
			//			#if !SPINE_TK2D
			//			if (atlasAssets.Length == 0) {
			//				Clear();
			//				return null;
			//			}
			//			#else
			//			if (atlasAssets.Length == 0 && spriteCollection == null) {
			//				Clear();
			//				return null;
			//			}
			//			#endif


			if (skeletonData != null)
				return skeletonData;

			AttachmentLoader attachmentLoader;
			float skeletonDataScale;
			Atlas[] atlasArray = this.GetAtlasArray();

#if !SPINE_TK2D
			//attachmentLoader = (atlasArray.Length == 0) ? (AttachmentLoader)new RegionlessAttachmentLoader() : (AttachmentLoader)new AtlasAttachmentLoader(atlasArray);

			if(atlasArray.Length == 0)
            {
				attachmentLoader = (AttachmentLoader)s_RegionlessAttachmentLoader;

			}
			else
            {
				s_AtlasAttachmentLoader.Init(atlasArray);
				attachmentLoader = (AttachmentLoader)s_AtlasAttachmentLoader;
			}

			skeletonDataScale = scale;
#else
			if (spriteCollection != null) {
				attachmentLoader = new Spine.Unity.TK2D.SpriteCollectionAttachmentLoader(spriteCollection);
				skeletonDataScale = (1.0f / (spriteCollection.invOrthoSize * spriteCollection.halfTargetHeight) * scale);
			} else {
				if (atlasArray.Length == 0) {
					Reset();
					if (!quiet) Debug.LogError("Atlas not set for SkeletonData asset: " + name, this);
					return null;
				}
				attachmentLoader = new AtlasAttachmentLoader(atlasArray);
				skeletonDataScale = scale;
			}
#endif

			SkeletonData loadedSkeletonData = null;
#if UNITY_EDITOR

			if (data==null)
            {
				if(null== skeletonJSON)
                {
					return null;
                }

				bool hasBinaryExtension = skeletonJSON.name.ToLower().Contains(".skel");
				try
				{
					if (hasBinaryExtension)
						loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(skeletonJSON.bytes, attachmentLoader, skeletonDataScale);
					else
						loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(skeletonJSON.text, attachmentLoader, skeletonDataScale);
				}
				catch (Exception ex)
				{
					if (!quiet)
						Debug.LogError("Error reading skeleton JSON file for SkeletonData asset: " + name + "\n" + ex.Message + "\n" + ex.StackTrace, skeletonJSON);
				}
			}else //外部传data进行解析
#else		
#endif
			{

				if (null == data)
				{
					return null;
				}

				loadedSkeletonData = SkeletonDataAsset.ReadSkeletonData(data, attachmentLoader, skeletonDataScale);
			}



#if UNITY_EDITOR

			if(string.IsNullOrEmpty(skeletonJsonPath))
            {
				skeletonJsonPath = AssetDatabase.GetAssetPath(skeletonJSON);
				skeletonJsonPath = skeletonJsonPath.Replace("Assets/", "");
			}
			

			if (loadedSkeletonData == null && !quiet && skeletonJSON != null) {
				string problemDescription = null;
				bool isSpineSkeletonData;
				SkeletonDataCompatibility.VersionInfo fileVersion = SkeletonDataCompatibility.GetVersionInfo(skeletonJSON, out isSpineSkeletonData, ref problemDescription);
				if (problemDescription != null) {
					if (!quiet)
						Debug.LogError(problemDescription, skeletonJSON);
					return null;
				}
				CompatibilityProblemInfo compatibilityProblemInfo = SkeletonDataCompatibility.GetCompatibilityProblemInfo(fileVersion);
				if (compatibilityProblemInfo != null) {
					SkeletonDataCompatibility.DisplayCompatibilityProblem(compatibilityProblemInfo.DescriptionString(), skeletonJSON);
					return null;
				}
			}
#endif
			if (loadedSkeletonData == null)
				return null;

			if (skeletonDataModifiers != null) {
				foreach (var modifier in skeletonDataModifiers) {
					if (modifier != null && !(isUpgradingBlendModeMaterials && modifier is BlendModeMaterialsAsset)) {
						modifier.Apply(loadedSkeletonData);
					}
				}
			}
			if (!isUpgradingBlendModeMaterials)
				blendModeMaterials.ApplyMaterials(loadedSkeletonData);

			this.InitializeWithData(loadedSkeletonData);

			return skeletonData;
		}

		internal void InitializeWithData (SkeletonData sd) {
			this.skeletonData = sd;
			this.stateData = SpineObjectPoolFacade.Instance().Pop<AnimationStateData>();//  new AnimationStateData(skeletonData);
			this.stateData.Initalize(skeletonData);
			//refCount = 0;
			FillStateData();
			//Debug.LogError("hashcode = " + this.GetHashCode() + " ," + skeletonJsonPath + ",初始化 refCount= " + refCount);

			
		}

		public void FillStateData () {
			if (stateData != null) {
				stateData.defaultMix = defaultMix;

				for (int i = 0, n = fromAnimation.Length; i < n; i++) {
					if (fromAnimation[i].Length == 0 || toAnimation[i].Length == 0)
						continue;
					stateData.SetMix(fromAnimation[i], toAnimation[i], duration[i]);
				}
			}
		}

		public void LoadAtlas(int index,TextAsset data)
        {
			if(index>= atlasAssets.Length)
            {
				Debug.LogError("Atlas 加载越界 index=" + index + ",atlasAssets.Length=" + atlasAssets.Length);
				return;
            }
			var aa = atlasAssets[index];
			if(aa!=null&& aa.IsLoaded==false)
			{
				aa.LoadAtlas(data);
			}

		}

		internal Atlas[] GetAtlasArray () {
			var returnList = new System.Collections.Generic.List<Atlas>(atlasAssets.Length);
			for (int i = 0; i < atlasAssets.Length; i++) {
				var aa = atlasAssets[i];
				if (aa == null) continue;
				var a = aa.GetAtlas();
				if (a == null) continue;
				returnList.Add(a);
			}
			return returnList.ToArray();
		}

		static SkeletonBinary s_SkeletonBinary = new SkeletonBinary();
		static BinaryStream s_BinaryStream = new BinaryStream();
		internal static SkeletonData ReadSkeletonData (byte[] bytes, AttachmentLoader attachmentLoader, float scale) {
			//using (var input = new MemoryStream(bytes)) 

			{
				s_BinaryStream.Init(bytes, bytes.Length);
				s_SkeletonBinary.Init(attachmentLoader, scale);

				/*
				var s_SkeletonBinary = new SkeletonBinary(attachmentLoader) 
				{
					Scale = scale
				};
				*/

				return s_SkeletonBinary.ReadSkeletonData(s_BinaryStream);
			}
		}

		internal static SkeletonData ReadSkeletonData (string text, AttachmentLoader attachmentLoader, float scale) {
			var input = new StringReader(text);
			var json = new SkeletonJson(attachmentLoader) {
				Scale = scale
			};
			return json.ReadSkeletonData(input);
		}
	}

}

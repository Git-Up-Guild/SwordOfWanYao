//author: weihua.cui 
//date: 2025.01.09
//SkeletonAnimation 纹理换装

using Spine.Unity.AttachmentTools;
using Spine.Unity;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XGame.Entity.Part
{ 
    public class SpineSkinAnimation : ISpineSkinSprite
    {
        Skin customSkin;
        SkeletonAnimation skeletonAnimation;
        string baseSkinName = "base";

        public List<SkinPartHook> equippables = new List<SkinPartHook>();
        public void Init(Object graphic, List<SkinPartHook> equipableList)
        {
            customSkin = customSkin ?? new Skin("custom skin");
            skeletonAnimation = (SkeletonAnimation)graphic;
            var templateSkin = skeletonAnimation.Skeleton.Data.FindSkin(baseSkinName);
            if (templateSkin != null)
                customSkin.AddAttachments(templateSkin);

            skeletonAnimation.Skeleton.Skin = customSkin;

            equippables.Clear();
            defaultAttachMent.Clear();
            for (int i = 0; i < equipableList.Count; i++)
            {
                equippables.Add(equipableList[i]);
                InitDefaultAttachMent(equipableList[i]);
            }

            //EquipHook equipHook = new EquipHook()
            //{
            //    type = EquipType.Gun,
            //    slot = "gun",
            //    templateSkin = "base",
            //    templateAttachment = "gun",
            //};
            //equippables.Add(equipHook);
            //InitDefaultAttachMent(equipHook);
            //equipHook = new EquipHook()
            //{
            //    type = EquipType.Goggles,
            //    slot = "goggles",
            //    templateSkin = "base",
            //    templateAttachment = "goggles",
            //};
            //equippables.Add(equipHook);
            //InitDefaultAttachMent(equipHook);

        }
        public Material sourceMaterial
        {
            get
            {
                return skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial;
            }
        }
        public Dictionary<int, Attachment> defaultAttachMent = new Dictionary<int, Attachment>();
        private void InitDefaultAttachMent(SkinPartHook equipHook)
        {
            var skeleton = skeletonAnimation.Skeleton;
            var baseSkin = skeleton.Data.FindSkin(baseSkinName);
            int visorSlotIndex = skeleton.FindSlotIndex(equipHook.slot); // You can access GetAttachment and SetAttachment via string, but caching the slotIndex is faster.
            Attachment baseAttachment = baseSkin.GetAttachment(visorSlotIndex, equipHook.templateAttachment); // STEP 1.1
            defaultAttachMent.Add((int)equipHook.type, baseAttachment);
        }

        public Texture2D runtimeAtlas;
        public Material runtimeMaterial;
        public void Equip(EquipAsset asset)
        {
            //var skeletonGraphic = GetComponent<SkeletonGraphic>();
            var skeleton = skeletonAnimation.Skeleton;

            // STEP 0: PREPARE SKINS
            // Let's prepare a new skin to be our custom skin with equips/customizations. We get a clone so our original skins are unaffected.
            // This requires that all customizations are done with skin placeholders defined in Spine.
            //customSkin = customSkin ?? skeleton.UnshareSkin(true, false, skeletonAnimation.AnimationState); // use this if you are not customizing on the default skin and don't plan to remove
            // Next let's get the skin that contains our source attachments. These are the attachments that
            var baseSkin = skeleton.Data.FindSkin(baseSkinName);

            // STEP 1: "EQUIP" ITEMS USING SPRITES
            // STEP 1.1 Find the original attachment.
            // Step 1.2 Get a clone of the original attachment.
            // Step 1.3 Apply the Sprite image to it.
            // Step 1.4 Add the remapped clone to the new custom skin.
            var equipType = asset.partType;
            SkinPartHook howToEquip = equippables.Find(x => x.type == equipType);

            // Let's do this for the visor.
            int visorSlotIndex = skeleton.FindSlotIndex(howToEquip.slot); // You can access GetAttachment and SetAttachment via string, but caching the slotIndex is faster.
            Attachment baseAttachment = baseSkin.GetAttachment(visorSlotIndex, howToEquip.templateAttachment); // STEP 1.1
            Attachment newAttachment = baseAttachment.GetRemappedClone(asset.sprite, sourceMaterial); // STEP 1.2 - 1.3
                                                                                                      // Note: Each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true` creates
                                                                                                      // a cached Texture copy which can be cleared by calling AtlasUtilities.ClearCache() as done below.
            customSkin.SetAttachment(visorSlotIndex, howToEquip.templateAttachment, newAttachment); // STEP 1.4

            //// And now for the gun.
            //int gunSlotIndex = skeleton.FindSlotIndex(gunSlot);
            //Attachment baseGun = baseSkin.GetAttachment(gunSlotIndex, gunKey); // STEP 1.1
            //Attachment newGun = baseGun.GetRemappedClone(gunSprite, sourceMaterial); // STEP 1.2 - 1.3
            //if (newGun != null) customSkin.SetAttachment(gunSlotIndex, gunKey, newGun); // STEP 1.4

            // customSkin.RemoveAttachment(gunSlotIndex, gunKey); // To remove an item.
            // customSkin.Clear()
            // Use skin.Clear() To remove all customizations.
            // Customizations will fall back to the value in the default skin if it was defined there.
            // To prevent fallback from happening, make sure the key is not defined in the default skin.

            // STEP 3: APPLY AND CLEAN UP.
            // Recommended: REPACK THE CUSTOM SKIN TO MINIMIZE DRAW CALLS
            // 				Repacking requires that you set all source textures/sprites/atlases to be Read/Write enabled in the inspector.
            // 				Combine all the attachment sources into one skin. Usually this means the default skin and the custom skin.
            // 				call Skin.GetRepackedSkin to get a cloned skin with cloned attachments that all use one texture.


            // `GetRepackedSkin()` and each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true`
            // cache necessarily created Texture copies which can be cleared by calling AtlasUtilities.ClearCache().
            // You can optionally clear the textures cache after multiple repack operations.
            // Just be aware that while this cleanup frees up memory, it is also a costly operation
            // and will likely cause a spike in the framerate.

        }
        public Skin repackedSkin = new Skin("repacked skin");
        public void Apply()
        {
            var skeleton = skeletonAnimation.Skeleton;

            if (true)
            {
                repackedSkin.Clear(false);
                repackedSkin.AddAttachments(skeleton.Data.DefaultSkin);
                repackedSkin.AddAttachments(customSkin);
                // Note: materials and textures returned by GetRepackedSkin() behave like 'new Texture2D()' and need to be destroyed
                if (runtimeMaterial)
                    GameObject.Destroy(runtimeMaterial);
                if (runtimeAtlas)
                    GameObject.Destroy(runtimeAtlas);
                repackedSkin = repackedSkin.GetRepackedSkin("repacked skin", sourceMaterial, out runtimeMaterial, out runtimeAtlas);
                skeleton.SetSkin(repackedSkin);
            }
            else
            {
                //这里有问题
                skeleton.SetSkin(customSkin);
            }

            //skeleton.SetSlotsToSetupPose();
            skeleton.SetToSetupPose();
            skeletonAnimation.Update(0);


            AtlasUtilities.ClearCache();
            Resources.UnloadUnusedAssets();
        }

        public void Reset(SkinPartType equipType)
        {
            var skeleton = skeletonAnimation.Skeleton;

            // STEP 0: PREPARE SKINS
            // Let's prepare a new skin to be our custom skin with equips/customizations. We get a clone so our original skins are unaffected.
            // This requires that all customizations are done with skin placeholders defined in Spine.
            //customSkin = customSkin ?? skeleton.UnshareSkin(true, false, skeletonAnimation.AnimationState); // use this if you are not customizing on the default skin and don't plan to remove
            // Next let's get the skin that contains our source attachments. These are the attachments that
            var baseSkin = skeleton.Data.FindSkin(baseSkinName);


            SkinPartHook howToEquip = equippables.Find(x => x.type == equipType);

            if (howToEquip != null) 
            {
                // Let's do this for the visor.
                int visorSlotIndex = skeleton.FindSlotIndex(howToEquip.slot); // You can access GetAttachment and SetAttachment via string, but caching the slotIndex is faster.
                                                                              // Note: Each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true` creates                                                                                                  // a cached Texture copy which can be cleared by calling AtlasUtilities.ClearCache() as done below.
                customSkin.SetAttachment(visorSlotIndex, howToEquip.templateAttachment, defaultAttachMent[(int)equipType]); // STEP 1.4
            }

            //// 获取 Skeleton 对象
            //var skeleton = skeletonAnimation.Skeleton;

            //// 遍历所有插槽并还原到默认 attachment
            //foreach (var slot in skeleton.Slots)
            //{
            //    var setupAttachment = slot.Data.AttachmentName != null
            //        ? skeleton.GetAttachment(slot.Data.Index, slot.Data.AttachmentName)
            //        : null;

            //    slot.Attachment = setupAttachment;
            //}
            //skeleton.SetSlotsToSetupPose();
            //skeletonAnimation.Update(0);

            /*
            if (null != resultCombinedSkin)
            {
                resultCombinedSkin.Clear(false);
                SpineObjectPoolFacade.Instance().Push(resultCombinedSkin);
                resultCombinedSkin = null;
            }*/
        }
    }
}
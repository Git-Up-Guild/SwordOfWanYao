using Spine.Unity;
using Spine;
using XGame.Entity.Part;
using System.Collections.Generic;
using UnityEngine;
using XGame.Ini;
using XGame.Poolable;
//author weihua.cui
//替换单张纹理

namespace XGame.Entity.Part
{
    public interface ISpineSkin:IPoolable
    {
       // void Init(object context = null);
        //bool ResetClean();
        void Equip(EquipAsset asset);
        void Apply();
        //void Reset();
    }
    public class SpineMixSkinGraphic : ISpineSkin
    {
        private Skin resultCombinedSkin = null; // new Skin("character-combined");
        //private Skin characterSkin;
        private SkeletonGraphic skeletonGraphic;


        public void Init(object context = null)
        {
            skeletonGraphic = context as SkeletonGraphic;
            if(null== resultCombinedSkin)
            {
                resultCombinedSkin = SpineObjectPoolFacade.Instance().Pop<Skin>();
            }
           
            //characterSkin = new Skin("character-base");
            applied = true;
        }
        public void Equip(EquipAsset asset)
        {
            if (applied)
            {
                resultCombinedSkin.Clear(false);
                applied = false;
            }
            var skeleton = skeletonGraphic.Skeleton;
            var skeletonData = skeleton.Data;
            Skin skin = skeletonData.FindSkin(asset.skin);
            if (skin != null)
            {
                resultCombinedSkin.AddSkin(skin);
            }
            else
            {
                if (GameIni.Instance.enableDebug)
                    Debug.LogError($" 部位{asset.partType} 皮肤{asset.skin}找不到");
            }
        }
        private bool applied = false;
        public void Apply()
        {
            var skeleton = skeletonGraphic.Skeleton;
            //todo 减少new
            //var resultCombinedSkin = new Skin("character-combined");
            //resultCombinedSkin.Clear(false) ;
            //resultCombinedSkin.AddSkin(characterSkin);
            //AddEquipmentSkinsTo(resultCombinedSkin);

            skeleton.SetSkin(resultCombinedSkin);
            skeleton.SetSlotsToSetupPose();
            applied = true;
        }

        public void Reset()
        {
            
            skeletonGraphic = null;
            applied = true;

            if(null!= resultCombinedSkin)
            {
                resultCombinedSkin.Clear(false);
                SpineObjectPoolFacade.Instance().Push(resultCombinedSkin);
                resultCombinedSkin = null;
            }
          

            //characterSkin.Clear(false);
        }

        public bool Create()
        {
            return true;
        }


        public void Release()
        {
            Reset();
           
        }
    }
}
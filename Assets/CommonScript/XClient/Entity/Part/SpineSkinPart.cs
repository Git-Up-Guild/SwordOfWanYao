
//**文件名:	SpinePrefabPart.cs
//** 版  权:	(C)冰川网络
//* *创建人:	崔卫华
//** 日  期:	2024.12.31
//* *版  本: 1.0
//* *描  述:
//**应  用: 负责加载spine的换装

using System.Collections;
using System.Collections.Generic;
using GameScripts;
using Spine;
using Spine.Unity;
using UnityEngine;
using XClient.Entity;
using XClient.Common;
using XGame.Ini;


namespace XGame.Entity.Part
{
    public class SpineSkinPart : BasePart
    {

        private Dictionary<int, int> m_skins;
        private int m_displayId;
        private bool m_isPlayer;

        protected override void OnInit(object context)
        {
            base.OnInit(context);
            NetEntityShareInitContext netentityContext = context as NetEntityShareInitContext;
            if (netentityContext != null)
            {
                CreateMonsterContext createContext = netentityContext.localInitContext as CreateMonsterContext;
                if (createContext != null)
                {
                    m_skins = createContext.dicSkinData;
                    m_displayId = createContext.displayID;
                    m_isPlayer = createContext.isPlayer;
                    if (GameIni.Instance.enableDebug)
                    {
                        Debug.Log($"SetSkin:  CreatureViewId:{m_displayId}");
                    }
                }
            }
        }

        protected override void OnReset()
        {
            OnResUnLoaded();
            m_skins = null;
            m_isPlayer = true;
            skelAnimation = null;
            skelGraphic = null;

            base.OnReset();
         
          
        }

        private SkeletonAnimation skelAnimation;
        private SkeletonGraphic skelGraphic;

        private ISpineSkin spineClothGraphic;
        private uint equipHandle = 0;
        private void OnResLoaded()
        {
            if (!m_isPlayer)
                return;
            //todo 从池中取
            if (skelGraphic != null)
            {
                spineClothGraphic = SpineObjectPoolFacade.Instance().Pop<SpineMixSkinGraphic>();// new SpineMixSkinGraphic();
                spineClothGraphic.Init(skelGraphic);
            }
            else if (skelAnimation != null)
            {
                spineClothGraphic = SpineObjectPoolFacade.Instance().Pop<SpineMixSkinAnimation>(); //new SpineMixSkinAnimation();
                spineClothGraphic.Init(skelAnimation);
            }
            EquipSkin(m_skins);
            //LoadSkinTest();
            //test
            //Dictionary<int, int> equipskins = new Dictionary<int, int>();
            //for (int i = (int)EnDiscipleProp.DISCIPLE_PROP_SKIN_UPPER; i <= (int)EnDiscipleProp.DISCIPLE_PROP_SKIN_MOUTH; i++)
            //{
            //    equipskins.Add(i, 101);
            //}
            //EquipSkin(equipskins);
        }
        private void OnResUnLoaded()
        {
            if (spineClothGraphic != null)
            {
                SpineObjectPoolFacade.Instance().Push(spineClothGraphic);
                spineClothGraphic = null;
            }
            skelGraphic = null;
            skelAnimation = null;
            //LoadSkinTest();
        }

        public void EquipSkin(Dictionary<int, int> equipskins)
        {
            /*
            var viewCfg = GameGlobal.GameScheme.CreatureView_0(m_displayId);
            if (viewCfg == null)
            {
                Debug.LogError($"无效的CreatureView配置 Id:{m_displayId}");
                return;
            }*/

           //equipskins.Clear();
            //equipskins.Add((int)SkinPartType.Hair, 903);
            //equipskins.Add((int)SkinPartType.Body, 206);
            //equipskins.Add((int)SkinPartType.SkinUpper, 201);
            //equipskins.Add((int)SkinPartType.SkinLower, 802);
            //equipskins.Add((int)SkinPartType.Mouth, 207);
            //equipskins.Add((int)SkinPartType.Eye, 804);

            //equipskins.Add((int)SkinPartType.Suit, 1003);
            //equipskins.Add((int)SkinPartType.Hair, 1004);
            //equipskins.Add((int)SkinPartType.Eye, 1005);
            //equipskins.Add((int)SkinPartType.Body, 1007);
            //equipskins.Add((int)SkinPartType.Mouth, 1008);
            ////if (null != equipskins)
            //{
            //    foreach (var kv in equipskins)
            //    {
            //        if (kv.Value == 0)
            //            continue;
            //        EquipAsset(kv.Key, kv.Value);
            //    }
            //}

            //拿官方资源测试的时候，整体换装加在后面的时候前面的子换装才生效
            //但是拿我们的资源测试的时候，我们的整体换装要加在前面
            //不等于1和0时候,先添加整体换装
            /*
            if (SkinHelper.IsWholeSkin(equipskins))
            {
                if (viewCfg.iSkinType != 0)
                {
                    if (viewCfg.iSkinParamWhole == 0)
                    {
                        Debug.LogError($"CreatureView的整体皮肤配置错误 Id:{m_displayId}");
                        return;
                    }
                    spineClothGraphic.EquipAsset((int)SkinPartType.Whole, viewCfg.iSkinParamWhole);
                }
            }
            else
            {
                if (null != equipskins)
                {
                    for (int i = 0; i < SkinHelper.DrawSkinOrders.Count; i++)
                    {
                        int skinType = SkinHelper.DrawSkinOrders[i];
                        if (equipskins.ContainsKey(skinType) && equipskins[skinType] != 0)
                        {
                            spineClothGraphic.EquipAsset(skinType, equipskins[skinType]);
                        }
                    }
                }
            }
            spineClothGraphic.Apply();
            */
        }
       

        #region testcode
     
        //private void LoadSkinTest()
        //{
        //    spineClothGraphic.Reset(EquipType.Gun);
        //    spineClothGraphic.Equip(new EquipAsset()
        //    {
        //        equipType = EquipType.Gun,
        //        skin = "yueru_shangyi"
        //    });
        //    spineClothGraphic.Equip(new EquipAsset()
        //    {
        //        equipType = EquipType.Goggles,
        //        skin = "yueru_shenti"
        //    });
        //    spineClothGraphic.Equip(new EquipAsset()
        //    {
        //        equipType = EquipType.Goggles,
        //        skin = "yueru_toufa"
        //    });
        //    spineClothGraphic.Equip(new EquipAsset()
        //    {
        //        equipType = EquipType.Goggles,
        //        skin = "yueru_wuguan"
        //    });
        //    spineClothGraphic.Equip(new EquipAsset()
        //    {
        //        equipType = EquipType.Goggles,
        //        skin = "yueru_xiayi"
        //    });

        //    spineClothGraphic.Apply();
        //}
        #endregion




        private SpineComponent spineComponent;
        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            base.OnReceiveEntityMessage(id, data);

            IPrefabResource res = data as IPrefabResource;

            switch (id)
            {
                case EntityMessageID.ResLoaded:
                    {
                        SpineComponent component = res?.gameObject.GetComponentInChildren<SpineComponent>();
                        skelAnimation = component.skeAni;
                        skelGraphic = component.skeGra;
                        spineComponent = component;
                        OnResLoaded();
                    }
                    break;
                case EntityMessageID.ResUnloaded:
                    {
                        //这里有可能不是自己创建的皮肤， 就不要清理了， 靠spineClothGraphic 内部清理
                        /*
                        if (skelAnimation != null)
                        {
                            skelAnimation.Skeleton.Skin.Clear(false);
                            skelAnimation = null;
                        }
                        if (skelGraphic != null)
                        {
                            skelGraphic.Skeleton.Skin.Clear(false);
                            skelGraphic = null;
                        }
                        */

                        OnResUnLoaded();
                    }
                    break;
                default:
                    break;
            }
        }
        #region 替换attachment
        //private List<EquipHook> m_equipables;
        //public List<EquipHook> equipHooks
        //{
        //    get
        //    {
        //        if (m_equipables == null)
        //        {
        //            m_equipables = new List<EquipHook>();
        //            EquipHook equipHook = new EquipHook()
        //            {
        //                type = EquipType.Gun,
        //                slot = "gun",
        //                templateSkin = "base",
        //                templateAttachment = "gun",
        //            };
        //            m_equipables.Add(equipHook);
        //            equipHook = new EquipHook()
        //            {
        //                type = EquipType.Goggles,
        //                slot = "goggles",
        //                templateSkin = "base",
        //                templateAttachment = "goggles",
        //            };
        //            m_equipables.Add(equipHook);
        //        }
        //        return m_equipables;
        //    }
        //}
        #endregion

    }
}
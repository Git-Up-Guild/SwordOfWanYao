using Spine.Unity;
using Spine;
using UnityEngine;
using System.Collections.Generic;
using gamepol;
using XClient.Common;
using System.Text;
using XGame.Ini;

namespace XGame.Entity.Part
{
    public static class SkinHelper
    {

        //Disciple的属性转换为装备部位
        public static SkinPartType PropDataToEquipType(int data)
        {
            EnDiscipleProp propdata = (EnDiscipleProp)data;
            switch (propdata)
            {
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_UPPER:
                    return SkinPartType.SkinUpper;
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_LOWER:
                    return SkinPartType.SkinLower;
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_OUTFIT:
                    return SkinPartType.Suit;
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_HAIR:
                    return SkinPartType.Hair;
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_EYE:
                    return SkinPartType.Eye;
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_WEAPON:
                    return SkinPartType.Weapon;
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_BODY:
                    return SkinPartType.Body;
                case EnDiscipleProp.DISCIPLE_PROP_SKIN_MOUTH:
                    return SkinPartType.Mouth;
                default:
                    Debug.LogError($"无效的皮肤部位枚举{propdata}");
                    return SkinPartType.Invalid;                 
            }
        }
        
        //public static SkinPartType BentPropDataToEquipType(int data)
        //{
        //    EnBatEntityProp propdata = (EnBatEntityProp)data;
        //    switch (propdata)
        //    {
        //        case EnBatEntityProp.BENT_PROP_SKIN_UPPER:
        //            return SkinPartType.SkinUpper;
        //        case EnBatEntityProp.BENT_PROP_SKIN_LOWER:
        //            return SkinPartType.SkinLower;
        //        case EnBatEntityProp.BENT_PROP_SKIN_OUTFIT:
        //            return SkinPartType.Suit;
        //        case EnBatEntityProp.BENT_PROP_SKIN_HAIR:
        //            return SkinPartType.Hair;
        //        case EnBatEntityProp.BENT_PROP_SKIN_EYE:
        //            return SkinPartType.Eye;
        //        case EnBatEntityProp.BENT_PROP_SKIN_WEAPON:
        //            return SkinPartType.Weapon;
        //        case EnBatEntityProp.BENT_PROP_SKIN_BODY:
        //            return SkinPartType.Body;
        //        case EnBatEntityProp.BENT_PROP_SKIN_MOUTH:
        //            return SkinPartType.Mouth;
        //        default:
        //            Debug.LogError($"无效的皮肤部位枚举{propdata}");
        //            return SkinPartType.Invalid;                 
        //    }
        //}

        //BattleEntity的属性转换为装备部位
        //public static SkinPartType BattleEntityPropDataToEquipType(int data)
        //{
        //    EnBatEntityProp propdata = (EnBatEntityProp)data;
        //    switch (propdata)
        //    {
        //        case EnBatEntityProp.BENT_PROP_SKIN_UPPER:
        //            return SkinPartType.SkinUpper;
        //        case EnBatEntityProp.BENT_PROP_SKIN_LOWER:
        //            return SkinPartType.SkinLower;
        //        case EnBatEntityProp.BENT_PROP_SKIN_OUTFIT:
        //            return SkinPartType.Suit;
        //        case EnBatEntityProp.BENT_PROP_SKIN_HAIR:
        //            return SkinPartType.Hair;
        //        case EnBatEntityProp.BENT_PROP_SKIN_EYE:
        //            return SkinPartType.Eye;
        //        case EnBatEntityProp.BENT_PROP_SKIN_WEAPON:
        //            return SkinPartType.Weapon;
        //        case EnBatEntityProp.BENT_PROP_SKIN_BODY:
        //            return SkinPartType.Body;
        //        case EnBatEntityProp.BENT_PROP_SKIN_MOUTH:
        //            return SkinPartType.Mouth;
        //        default:
        //            Debug.LogError($"无效的皮肤部位枚举{propdata}");
        //            return SkinPartType.Invalid;
        //    }
        //}

        private static List<int> s_DrawSkinOrders;
        public static List<int> DrawSkinOrders
        {
            get
            {
                if (s_DrawSkinOrders == null)
                {
                    s_DrawSkinOrders = new List<int>
                    {
                        (int)SkinPartType.Weapon,
                        (int)SkinPartType.Hair,
                        (int)SkinPartType.Eye,
                        (int)SkinPartType.Mouth,
                        (int)SkinPartType.SkinUpper,
                        (int)SkinPartType.SkinLower,
                        (int)SkinPartType.Suit,
                        (int)SkinPartType.Body
                    };
                }
                return s_DrawSkinOrders;
            }
        }
        public static bool IsSameSkin(Dictionary<int, int> src, Dictionary<int, int> dst)
        {
            if (src == null || dst == null)
                return false;

            if (src == dst)
            {
#if UNITY_EDITOR
                Debug.LogError(" 两个皮肤引用相同");
                return true;
#endif
            }
            for (int i = (int)SkinPartType.SkinUpper; i <= (int)SkinPartType.Mouth; i++)
            {
                if (src.ContainsKey(i) && dst.ContainsKey(i) && src[i] == dst[i])
                {
                }
                else
                {
                    return false;
                }                
            }
            return true;            
        }

        public static bool IsSameMonsterSkin(Dictionary<int, int> src, Dictionary<int, int> dst)
        {
            if (src == null || dst == null)
                return false;

            if (src == dst)
            {
#if UNITY_EDITOR
                Debug.LogError(" 两个皮肤引用相同");
                return true;
#endif
            }
            //for (int i = (int)EnBatEntityProp.BENT_PROP_SKIN_UPPER; i <= (int)EnBatEntityProp.BENT_PROP_SKIN_MOUTH; i++)
            //{
            //    if (src.ContainsKey(i) && dst.ContainsKey(i) && src[i] == dst[i])
            //    {
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            return false;
        }
        public static string GetTestSkin(SkinPartType equipType)
        {
            if (equipType == SkinPartType.SkinUpper)
                return "yueru_shangyi";
            else if (equipType == SkinPartType.SkinLower)
                return "yueru_xiayi";
            else if (equipType == SkinPartType.Hair)
                return "yueru_toufa";
            else if (equipType == SkinPartType.Eye)
                return string.Empty;
            else if (equipType == SkinPartType.Weapon)
                return string.Empty;
            else if (equipType == SkinPartType.Body)
                return "yueru_shenti";
            else if (equipType == SkinPartType.Mouth)
                return "yueru_wuguan";
            return string.Empty;
        }

        private static EquipAsset m_tmpEquipAsset = new EquipAsset();
        public static void EquipAsset(this ISpineSkin spineClothGraphic, int part, int skin)
        {
            /*
            var skinCfg = GameGlobal.GameScheme.Skin_0((uint)skin);
            SkinPartType equipType = (SkinPartType)part;
            if (skinCfg != null)
            {
                m_tmpEquipAsset.partType = equipType;
                m_tmpEquipAsset.skin = skinCfg.szSkin;
                //string str = SkinHelper.GetTestSkin(equipType);
                //m_tmpEquipAsset.skin = str; 
                if (!string.IsNullOrEmpty(m_tmpEquipAsset.skin))
                    spineClothGraphic.Equip(m_tmpEquipAsset);
            }
            else
            {
                Debug.LogError($"无效的皮肤数据 部位枚举:{equipType} 部位id:{part} 皮肤id:{skin}");
            }
            */
        }

        //如果皮肤没有数据, 表示是整体换装
        public static bool IsWholeSkin(Dictionary<int, int> skins)
        {
            if (skins == null)
                return true;
            foreach (var kv in skins)
            {
                if (kv.Value != 0)
                    return false;
            }
            return true;
        }
        private static StringBuilder m_sb = new StringBuilder();
        public static void DebugSkin(Dictionary<int, int> skins)
        {
            if (GameIni.Instance.enableDebug)
            {
                m_sb.Clear();
                m_sb.Append("皮肤数据");
                if (skins == null)
                {
                    m_sb.Append("skins is null");
                }
                else
                {
                    foreach (var kv in skins)
                    {
                        m_sb.Append($"type:{(SkinPartType)kv.Key} value:{kv.Value}");
                    }
                }
                Debug.Log(m_sb.ToString());
            }
        }
    }
}
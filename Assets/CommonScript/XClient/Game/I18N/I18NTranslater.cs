#if SUPPORT_I18N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XClient.Common;
using XGame.I18N;

namespace XClient.Game.I18N
{
    class I18NTranslater : ITextTranslater
    {
        public string DoTranslater(II18NManager manager, string strKey, ELanguage language)
        {
            var scheme = GameGlobal.Instance.GetModule(DModuleID.MODULE_ID_SCHEME) as ISchemeModule;
            if(scheme == null)
                return strKey;

            var cfg = scheme.GetCgamescheme().I18NBrief_0(strKey);
            if (cfg == null)
            {
#if UNITY_EDITOR
                return "[I18NErr0]" + strKey;
#endif
                return strKey;
            }

            string text = ReadI18NText(cfg, language);
            if (string.IsNullOrEmpty(text) && manager.Config.UseDefLangTextWhenNotExist && language != manager.Config.DefualtLanguage)
            {
                text = ReadI18NText(cfg, manager.Config.DefualtLanguage);
            }

            if (string.IsNullOrEmpty(text))
            {
#if UNITY_EDITOR
                return "[I18NErr1]" + strKey;
#endif
                return strKey;
            }

            return text;
        }

        public string DoTranslater(II18NManager manager, int idKey, ELanguage language)
        {
            var scheme = GameGlobal.Instance.GetModule(DModuleID.MODULE_ID_SCHEME) as ISchemeModule;
            if (scheme == null)
                return string.Empty;

            var cfg = scheme.GetCgamescheme().I18N_0(idKey);
            if (cfg == null)
            {
                Debug.LogError("读取I18N配置失败，idKey=" + idKey);
                return string.Empty;
            }

            string text = ReadI18NText(cfg, language);
            if (string.IsNullOrEmpty(text) && manager.Config.UseDefLangTextWhenNotExist && language != manager.Config.DefualtLanguage)
            {
                text = ReadI18NText(cfg, manager.Config.DefualtLanguage);
            }

            if (string.IsNullOrEmpty(text))
            {
#if UNITY_EDITOR
                return $"[I18NErr1]idKey";
#endif
            }

            return text;
        }

        private string ReadI18NText(object o, ELanguage lang)
        {
            var cfg = o as cfg_I18N;
            var cfg2 = o as cfg_I18NBrief;

            switch (lang)
            {
                case ELanguage.ZH:
                    return cfg != null ? cfg.zh : cfg2.zh;
                case ELanguage.EN:
                    return cfg != null ? cfg.en : cfg2.en;
                case ELanguage.HK:
                    return cfg != null ? cfg.hk : cfg2.hk;
                case ELanguage.JP:
                    return cfg != null ? cfg.jp : cfg2.jp;
                case ELanguage.KO:
                    return cfg != null ? cfg.ko : cfg2.ko;
                case ELanguage.PI:
                    return cfg != null ? cfg.pi : cfg2.pi;
                case ELanguage.PO:
                    return cfg != null ? cfg.po : cfg2.po;
                case ELanguage.TH:
                    return cfg != null ? cfg.th : cfg2.th;
                case ELanguage.VI:
                    return cfg != null ? cfg.vi : cfg2.vi;
                case ELanguage.ES:
                    return cfg != null ? cfg.es : cfg2.es;
                default:
                    return string.Empty;
            }
        }

    }
}

#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.I18N;

namespace XGame.Update
{

    //配置基础项
    [Serializable]
    public class LanguageCfg
    {
        public ELanguage langType = ELanguage.EN;
        public List<string> m_listI18NConfig = new List<string>();
    }

    [CreateAssetMenu]
    public class UpdateI18NConfig : ScriptableObject
    {

        [SerializeField]
        public List<string> m_listSrcConfig = new List<string>();

        //[SerializeField]
       // public List<string> m_listDstConfig = new List<string>();


        [SerializeField]
        public List<LanguageCfg> m_listLanguageConfig = new List<LanguageCfg>();


        private Dictionary<string, string> m_dicUpdateI18N = new Dictionary<string, string>();

        //默认语言
        public ELanguage curLangType = ELanguage.ZH;

        //是否中文
       // private bool m_isChineseLanguage = true;
        //初始化
        public void Init()
        {

            //获取当前语言
            int saveLang = PlayerPrefs.GetInt(I18NManager.SAVE_DATA_NAME, -1);

            //没有保存语言的,取系统语言判断
            if (saveLang == -1)
            {
                //默认是英文
                curLangType = I18NManager.SystemLanguageToGameLanguage();

                /*

                string languageStr = Application.systemLanguage.ToString();
                Debug.Log("当前手机语言: " + languageStr);

                if (languageStr.CompareTo("ChineseTraditional") == 0)
                {
                    curLangType = ELanguage.HK;
                }
                else if(languageStr.CompareTo("ChineseSimplified") == 0||languageStr.CompareTo("Chinese") == 0)
                {
                    curLangType = ELanguage.ZH;
                }else
                {
                    //默认其他语言是英文
                    curLangType = ELanguage.EN;
                }
                */

            }else
            {
                curLangType = (ELanguage)saveLang;
            }


            List<string> listBackupDstConfig = null;
            List<string> listDstConfig = null;
            //查找当前语言
            int nCount = m_listLanguageConfig.Count;
            for(int i=0;i<nCount;++i)
            {
                if(curLangType== m_listLanguageConfig[i].langType)
                {
                    listDstConfig = m_listLanguageConfig[i].m_listI18NConfig;
                    
                    //英文作为第一后备语言
                    if(curLangType== ELanguage.EN)
                    {
                        listBackupDstConfig = m_listLanguageConfig[i].m_listI18NConfig;
                    }
                    break;
                }
            }

            //没有找到的语言，优先使用英文，再没有就使用中文
            if(null== listDstConfig)
            {
                listDstConfig = listBackupDstConfig != null ? listBackupDstConfig : m_listSrcConfig;
            }


            m_dicUpdateI18N.Clear();
            if(m_listSrcConfig.Count!= listDstConfig.Count)
            {
                Debug.LogError("m_listSrcConfig.Count!= m_listDstConfig.Count 请检查国际化配置");
                return;
            }

            for(int i=0;i< m_listSrcConfig.Count;++i)
            {
                if(m_dicUpdateI18N.ContainsKey(m_listSrcConfig[i])==false)
                {
                    m_dicUpdateI18N.Add(m_listSrcConfig[i], listDstConfig[i]);
                }
            }

        }

        //获取一个语言转换字符串
        public string GetLangString(string src)
        {
            if(string.IsNullOrEmpty(src))
            {
                return src;
            }

            string dst = null;
            if(m_dicUpdateI18N.TryGetValue(src,out dst))
            {
                if(string.IsNullOrEmpty(dst)==false)
                {
                    return dst;
                }
                
            }
            return src;
        }

    }


}


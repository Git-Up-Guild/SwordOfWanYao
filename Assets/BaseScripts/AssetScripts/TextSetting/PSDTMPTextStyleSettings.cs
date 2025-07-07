/*****************************************************
** 文 件 名：PSDTMPTextStyleSettings
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/1/7 19:00:10
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XGame.Attr;

/// <summary>
/// TextMeshPro风格配置
/// </summary>
public class PSDTMPTextStyleSettings : ScriptableObject
{
    [System.Serializable]
    public class StyleSetting
    {
        //风格名称
        public string name;

        //描述
        public string desc;

        //材质
        public Material fontMat;

        //渐变色
        public TMP_ColorGradient colorGrandient;
    }

    [Label("SDF字体资源")]
    public TMP_FontAsset sdfFont;

    [Label("字体名称")]
    public string fontName;

    [Header("普通文本风格列表")]
    [Label("普通", ".name")]
    public List<StyleSetting> normalStyles;

    [Header("标题文本风格列表")]
    [Label("标题", ".name")]
    public List<StyleSetting> titleStyles;

    [Header("数字文本风格列表")]
    [Label("数字", ".name")]
    public List<StyleSetting> numberStyles;

    [Header("其它风格列表")]
    [Label("其它", ".name")]
    public List<StyleSetting> styles;

    //风格字典
    private Dictionary<string, StyleSetting> m_dicStyleSettings = new Dictionary<string, StyleSetting>();

    private void Awake()
    {
        BuildStyleDictionary();
    }

    private void BuildStyleDictionary()
    {
        m_dicStyleSettings.Clear();

        foreach (var s in normalStyles)
            if(!m_dicStyleSettings.ContainsKey(s.name))
                m_dicStyleSettings.Add(s.name, s);

        foreach (var s in titleStyles)
            if (!m_dicStyleSettings.ContainsKey(s.name))
                m_dicStyleSettings.Add(s.name, s);

        foreach (var s in numberStyles)
            if (!m_dicStyleSettings.ContainsKey(s.name))
                m_dicStyleSettings.Add(s.name, s);

        foreach (var s in styles)
            if (!m_dicStyleSettings.ContainsKey(s.name))
                m_dicStyleSettings.Add(s.name, s);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildStyleDictionary();
    }
#endif

    /// <summary>
    /// 获取所有的风格
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, StyleSetting> GetAllStyles()
    {
        return m_dicStyleSettings;
    }

    /// <summary>
    /// 获取风格配置
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public StyleSetting GetStyle(string name)
    {
        StyleSetting style;
        if (m_dicStyleSettings.TryGetValue(name, out style))
            return style;
        return null;
    }
}

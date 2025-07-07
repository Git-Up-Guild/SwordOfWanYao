#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;


//创建一个GUI类
public class SampleGUI : ShaderGUI
{
    public GUIStyle style = new GUIStyle();
    static bool Foldout(bool display, string title)
    {
        var style = new GUIStyle("ShurikenModuleTitle");
        style.font = new GUIStyle(EditorStyles.boldLabel).font;
        style.border = new RectOffset(15, 7, 4, 4);
        style.fixedHeight = 22;
        style.contentOffset = new Vector2(20f, -2f);
        style.fontSize = 11;
        style.normal.textColor = new Color(0.7f, 0.8f, 0.9f);




        var rect = GUILayoutUtility.GetRect(16f, 25f, style);
        GUI.Box(rect, title, style);

        var e = Event.current;

        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            display = !display;
            e.Use();
        }

        return display;
    }

    static bool _Function_Foldout = false;
    static bool _Base_Foldout = false;
    static bool _Common_Foldout = true;
    static bool _Main_Foldout = true;
    static bool _Tips_Foldout = false;
    static bool _Mask_Foldout = true;
    static bool _Distort_Foldout = true;
    static bool _Dissolve_Foldout = true;
    static bool _FNL_Foldout = true;

    MaterialEditor m_MaterialEditor;

    MaterialProperty BlendMode = null;
    MaterialProperty CullMode = null;

    MaterialProperty MainTex = null;
    MaterialProperty MainColor = null;
    MaterialProperty MainTexAR = null;
    MaterialProperty MainTexUSpeed = null;
    MaterialProperty MainTexVSpeed = null;
    MaterialProperty CustomMainTex = null;

    MaterialProperty FMaskTex = null;
    MaterialProperty MaskTex = null;
    MaterialProperty MaskTexAR = null;
    MaterialProperty MaskTexUSpeed = null;
    MaterialProperty MaskTexVSpeed = null;

    MaterialProperty FDistortTex = null;
    MaterialProperty DistortTex = null;
    MaterialProperty DistortTexAR = null;
    MaterialProperty DistortTexUSpeed = null;
    MaterialProperty DistortTexVSpeed = null;
    MaterialProperty DistortFactor = null;
    MaterialProperty DistortMainTex = null;
    MaterialProperty DistortMaskTex = null;
    MaterialProperty DistortDissolveTex = null;

    MaterialProperty FDissolveTex = null;
    MaterialProperty DissolveTex = null;
    MaterialProperty DissolveTexAR = null;
    MaterialProperty DissolveTexUSpeed = null;
    MaterialProperty DissolveTexVSpeed = null;
    MaterialProperty DissolveFactor = null;
    MaterialProperty DissolveColor = null;
    MaterialProperty CustomDissolve = null;
    MaterialProperty DissolveSoft = null;
    MaterialProperty DissolveWide = null;

    MaterialProperty FFnl = null;
    MaterialProperty FnlColor = null;
    MaterialProperty FnlScale = null;
    MaterialProperty FnlPower = null;
    MaterialProperty ReFnl = null;

    MaterialProperty MainAlpha = null;
    MaterialProperty FDepth = null;
    MaterialProperty DepthFade = null;

    public void FindProperties(MaterialProperty[] props)
    {
        BlendMode = FindProperty("_BlendMode", props);
        CullMode = FindProperty("_CullMode", props);

        MainTex = FindProperty("_MainTex", props);
        MainColor = FindProperty("_MainColor", props);
        MainTexAR = FindProperty("_MainTexAR", props);
        MainTexUSpeed = FindProperty("_MainTexUSpeed", props);
        MainTexVSpeed = FindProperty("_MainTexVSpeed", props);
        CustomMainTex = FindProperty("_CustomMainTex", props);

        FMaskTex = FindProperty("_FMaskTex", props);
        MaskTex = FindProperty("_MaskTex", props);
        MaskTexAR = FindProperty("_MaskTexAR", props);
        MaskTexUSpeed = FindProperty("_MaskTexUSpeed", props);
        MaskTexVSpeed = FindProperty("_MaskTexVSpeed", props);

        FDistortTex = FindProperty("_FDistortTex", props);
        DistortTex = FindProperty("_DistortTex", props);
        DistortTexAR = FindProperty("_DistortTexAR", props);
        DistortTexUSpeed = FindProperty("_DistortTexUSpeed", props);
        DistortTexVSpeed = FindProperty("_DistortTexVSpeed", props);
        DistortFactor = FindProperty("_DistortFactor", props);
        DistortMainTex = FindProperty("_DistortMainTex", props);
        DistortMaskTex = FindProperty("_DistortMaskTex", props);
        DistortDissolveTex = FindProperty("_DistortDissolveTex", props);

        FDissolveTex = FindProperty("_FDissolveTex", props);
        DissolveTex = FindProperty("_DissolveTex", props);
        DissolveTexAR = FindProperty("_DissolveTexAR", props);
        DissolveTexUSpeed = FindProperty("_DissolveTexUSpeed", props);
        DissolveTexVSpeed = FindProperty("_DissolveTexVSpeed", props);
        DissolveFactor = FindProperty("_DissolveFactor", props);
        DissolveColor = FindProperty("_DissolveColor", props);
        CustomDissolve = FindProperty("_CustomDissolve", props);
        DissolveSoft = FindProperty("_DissolveSoft", props);
        DissolveWide = FindProperty("_DissolveWide", props);

        FFnl = FindProperty("_FFnl", props);
        FnlColor = FindProperty("_FnlColor", props);
        FnlScale = FindProperty("_FnlScale", props);
        FnlPower = FindProperty("_FnlPower", props);
        ReFnl = FindProperty("_ReFnl", props);

        MainAlpha = FindProperty("_MainAlpha", props);
        FDepth = FindProperty("_FDepth", props);
        DepthFade = FindProperty("_DepthFade", props);
    }


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {




        FindProperties(props);

        m_MaterialEditor = materialEditor;

        Material material = materialEditor.target as Material;

        //
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Function_Foldout = Foldout(_Function_Foldout, "功能定制");

        if (_Function_Foldout)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.ShaderProperty(FMaskTex, "遮罩模块");


            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(FDistortTex, "UV扭曲模块");
            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(FDissolveTex, "溶解模块");
            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(FFnl, "菲涅尔模块");
            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(FDepth, "软粒子模块");
            GUILayout.Space(5);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        //
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Base_Foldout = Foldout(_Base_Foldout, "基础设置");

        if (_Base_Foldout)
        {
            EditorGUI.indentLevel++;

            GUILayout.Space(5);
            m_MaterialEditor.ShaderProperty(BlendMode, "叠加模式");
            if (material.GetFloat("_BlendMode") == 0)
            {
                material.SetFloat("_Scr", 5);
                material.SetFloat("_Dst", 10);
            }
            else
            {
                material.SetFloat("_Scr", 1);
                material.SetFloat("_Dst", 1);
            }
            GUILayout.Space(5);

            m_MaterialEditor.ShaderProperty(CullMode, "剔除模式");
            GUILayout.Space(10);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

        //
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        _Main_Foldout = Foldout(_Main_Foldout, "主贴图");

        if (_Main_Foldout)
        {
            EditorGUI.indentLevel++;

     

     
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("主贴图"), MainTex, MainColor);

            GUILayout.Space(5);

            if (MainTex.textureValue != null ) { 
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            m_MaterialEditor.TextureScaleOffsetProperty(MainTex);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
            m_MaterialEditor.ShaderProperty(MainTexAR, "R为透明通道");

            GUILayout.Space(5);
              m_MaterialEditor.ShaderProperty(CustomMainTex, "自定义数据");

            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            m_MaterialEditor.ShaderProperty(MainTexUSpeed, "U流动");
            m_MaterialEditor.ShaderProperty(MainTexVSpeed, "V流动");
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
            }



            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();


        //
        


     
      


        if (material.GetFloat("_FMaskTex") == 1) {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _Mask_Foldout = Foldout(_Mask_Foldout, "遮罩图");

        if (_Mask_Foldout)
        {
            EditorGUI.indentLevel++;
        
            m_MaterialEditor.TexturePropertySingleLine(new GUIContent("遮罩图"), MaskTex);

            GUILayout.Space(5);

            if (MaskTex.textureValue != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.TextureScaleOffsetProperty(MaskTex);
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);

                m_MaterialEditor.ShaderProperty(MaskTexAR, "R为遮罩通道");
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_MaterialEditor.ShaderProperty(MaskTexUSpeed, "U流动");
                m_MaterialEditor.ShaderProperty(MaskTexVSpeed, "V流动");
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }



            EditorGUI.indentLevel--;
        }
            EditorGUILayout.EndVertical();

        }
    


        //
        


   
       
       
  


        if (material.GetFloat("_FDistortTex") == 1)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _Distort_Foldout = Foldout(_Distort_Foldout, "UV扭曲图");

            if (_Distort_Foldout)
            {
                EditorGUI.indentLevel++;

                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("UV扭曲图"), DistortTex);

                GUILayout.Space(5);

                if (DistortTex.textureValue != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.TextureScaleOffsetProperty(DistortTex);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);

                    m_MaterialEditor.ShaderProperty(DistortTexAR, "R为遮罩通道");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(DistortFactor, "扭曲强度");
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(DistortTexUSpeed, "U流动");
                    m_MaterialEditor.ShaderProperty(DistortTexVSpeed, "V流动");
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);

                    m_MaterialEditor.ShaderProperty(DistortMainTex, "扭曲主贴图");
                    GUILayout.Space(5);
                    if (material.GetFloat("_FMaskTex") == 1) {
                        m_MaterialEditor.ShaderProperty(DistortMaskTex, "扭曲遮罩图");
                    GUILayout.Space(5);
                    }

                    if (material.GetFloat("_FDissolveTex") == 1) {
                        m_MaterialEditor.ShaderProperty(DistortDissolveTex, "扭曲溶解图");
                    GUILayout.Space(5);
                    }
                }



                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }


        //









        if (material.GetFloat("_FDissolveTex") == 1)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _Dissolve_Foldout = Foldout(_Dissolve_Foldout, "溶解图");

            if (_Dissolve_Foldout)
            {
                EditorGUI.indentLevel++;

                m_MaterialEditor.TexturePropertySingleLine(new GUIContent("UV扭曲图"), DissolveTex,DissolveColor);

                GUILayout.Space(5);

                if (DissolveTex.textureValue != null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.TextureScaleOffsetProperty(DissolveTex);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);

                    m_MaterialEditor.ShaderProperty(DissolveTexAR, "R为遮罩通道");
                    GUILayout.Space(5);

                    m_MaterialEditor.ShaderProperty(CustomDissolve, "自定义数据");
                    GUILayout.Space(5);
                    if (material.GetFloat("_CustomDissolve") == 0) {
                        m_MaterialEditor.ShaderProperty(DissolveFactor, "溶解程度");
                    GUILayout.Space(5);
                    }
                    m_MaterialEditor.ShaderProperty(DissolveSoft, "软化程度");
                    GUILayout.Space(5);
                    m_MaterialEditor.ShaderProperty(DissolveWide, "溶解宽度");
                    GUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    m_MaterialEditor.ShaderProperty(DissolveTexUSpeed, "U流动");
                    m_MaterialEditor.ShaderProperty(DissolveTexVSpeed, "V流动");
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);

                  
                }



                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        //







        if (material.GetFloat("_FFnl") == 1)
        {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _FNL_Foldout = Foldout(_FNL_Foldout, "菲涅尔");

            if (_FNL_Foldout)
            {
                EditorGUI.indentLevel++;

                m_MaterialEditor.ShaderProperty(ReFnl, "反向菲涅尔");
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(FnlScale, "菲涅尔强度");
                GUILayout.Space(5);
                m_MaterialEditor.ShaderProperty(FnlPower, "菲涅尔锐化");
                GUILayout.Space(5);
                if (material.GetFloat("_ReFnl") == 0) {
                    m_MaterialEditor.ShaderProperty(FnlColor, "菲涅尔颜色");
                    GUILayout.Space(5);
                }


                    EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

        }


        //

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        _Common_Foldout = Foldout(_Common_Foldout, "综合设置");

        if (_Common_Foldout)
        {
            EditorGUI.indentLevel++;
            m_MaterialEditor.ShaderProperty(MainAlpha, "总透明度");
            GUILayout.Space(5);
            if (material.GetFloat("_FDepth") == 1)
            {
                m_MaterialEditor.ShaderProperty(DepthFade, "软粒子");
                GUILayout.Space(5);

            }


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUI_Common(material);




            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();

      

        //
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        _Tips_Foldout = Foldout(_Tips_Foldout, "说明");

        if (_Tips_Foldout)
        {
            EditorGUI.indentLevel++;

            style.fontSize = 12;
            style.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            style.wordWrap = true;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(" 1.开启自定义数据时请先添加uv2，再添加custom1.xyzw,再添加custom2.xyzw", style);

            GUILayout.Space(5); GUILayout.Label(" 2.custom1.xy控制主贴图uv偏移", style);

            GUILayout.Space(5); GUILayout.Label(" 3.custom1.z控制溶解程度", style);

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            GUILayout.Label(" 本材质由坏熊猫制作。", style);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }


    void GUI_Common(Material material)
    {
       
        EditorGUI.BeginChangeCheck();
        {
            MaterialProperty[] props = { };
            base.OnGUI(m_MaterialEditor, props);
        }

    }

}
#endif
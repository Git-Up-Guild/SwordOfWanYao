//using System;
//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;

///// <summary>
///// 流光材质UI
///// </summary>
//public class DissolveMaterialEditor : MaterialEditor
//{

//    private int activeGlowModeIdx = 0;
//    private int activeGlowMultiplierIdx = 0;

//    private List<string> GlowModeList = new List<string>();
//    private List<string> activeGlowMultiplierList = new List<string>();

//    /// <summary>
//    /// Key为显示的名字，Value为shader里的KeyWorld
//    /// </summary>
//    private Dictionary<string, string> GlowModeKeyWorldMap = new Dictionary<string, string>();

//    /// <summary>
//    /// Key为显示的名字，Value为shader里的KeyWorld
//    /// </summary>
//    private Dictionary<string, string> GlowGlowMultiplierKeyWorldMap = new Dictionary<string, string>();


//    Material m_Material;
//    private void Init()
//    {
//        GlowModeList.Add("Base Texture");
//        GlowModeKeyWorldMap.Add("Base Texture", "GLOW_MAINTEX");

//        GlowModeList.Add("Main Color");
//        GlowModeKeyWorldMap.Add("Main Color", "GLOW_MAINCOLOR");

//        GlowModeList.Add("Glow Texture");
//        GlowModeKeyWorldMap.Add("Glow Texture", "GLOW_GLOWTEX");

//        GlowModeList.Add("Glow Color");
//        GlowModeKeyWorldMap.Add("Glow Color", "GLOW_GLOWCOLOR");

//        GlowModeList.Add("Vertex Color");
//        GlowModeKeyWorldMap.Add("Vertex Color", "GLOW_VERTEXCOLOR");


//        activeGlowMultiplierList.Add("None");
//        GlowGlowMultiplierKeyWorldMap.Add("None", "NO_MULTIPLY");

//        activeGlowMultiplierList.Add("Glow Color");
//        GlowGlowMultiplierKeyWorldMap.Add("Glow Color", "MULTIPLY_GLOWCOLOR");

//        activeGlowMultiplierList.Add("Vertex Color");
//        GlowGlowMultiplierKeyWorldMap.Add("Vertex Color", "MULTIPLY_VERT");

//        activeGlowMultiplierList.Add("Vertex Alpha");
//        GlowGlowMultiplierKeyWorldMap.Add("Vertex Alpha", "MULTIPLY_VERT_ALPHA");

//        activeGlowMultiplierList.Add("Base Texture (A)");
//        GlowGlowMultiplierKeyWorldMap.Add("Base Texture (A)", "MULTIPLY_MAINTEX_ALPHA");
//    }

//    public override void OnEnable()
//    {
//        base.OnEnable();
//        Init();
//        m_Material = target as Material;
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        m_Material.EnableKeyword("GLOW_DISSLOVE");
//        m_Material.DisableKeyword("GLOW_MAINTEX");
//        m_Material.DisableKeyword("GLOW_MAINCOLOR");
//        m_Material.DisableKeyword("GLOW_GLOWTEX");
//        m_Material.DisableKeyword("GLOW_ILLUMTEX");
//        m_Material.DisableKeyword("GLOW_GLOWCOLOR");
//        m_Material.DisableKeyword("GLOW_VERTEXCOLOR");


//        m_Material.EnableKeyword("NO_MULTIPLY");
//        m_Material.DisableKeyword("MULTIPLY_GLOWCOLOR");
//        m_Material.DisableKeyword("MULTIPLY_VERT");
//        m_Material.DisableKeyword("MULTIPLY_ILLUMTEX_ALPHA");
//        m_Material.DisableKeyword("MULTIPLY_MAINTEX_ALPHA");
//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(m_Material);
//        }
//    }

//}
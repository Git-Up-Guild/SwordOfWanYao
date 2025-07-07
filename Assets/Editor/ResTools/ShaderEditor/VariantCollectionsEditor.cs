/*******************************************************************
** 文件名: VariantCollectionsEditor.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:     谌安
** 日  期:    2018/12/13
** 版  本:    1.0
** 描  述:    Variant批量收集工具
** 应  用:    

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.ShaderVariantCollection;

public class VariantCollectionsEditor : Editor
{

    #region SVC class info
    //ShaderVariant 所有信息
    public class SVCNode
    {
        public Dictionary<string, SVCLODNode> m_LODInfo;

    }

    //ShaderVariant LOD信息
    public class SVCLODNode
    {
        //key:shader_feature multi_compile
        public List<SVCLODPASSNode> m_variantLODInfo;
    }

    //ShaderVariant LOD PASS节点
    public class SVCLODPASSNode
    {
        public UnityEngine.Rendering.PassType m_passType = UnityEngine.Rendering.PassType.ScriptableRenderPipeline;

        public List<List<string>> m_shaderFeature;
        public List<List<string>> m_multi_compile;

        public List<string> m_skipVariants;

        //解释变体信息
        public void AppendShaderFeature(string contents)
        {
            if (m_shaderFeature == null)
            {
                m_shaderFeature = new List<List<string>>();
            }

            contents = contents.Replace("#pragma", "");
            contents = contents.Trim();

            contents = contents.Replace("shader_feature_local", "");
            contents = contents.Trim();

            contents = contents.Replace("shader_feature", "");
            contents = contents.Trim();
            //split all keywords/
            SplitContents(m_shaderFeature, contents);
        }

        //解释变体信息
        public void AppendMultiCompile(string contents)
        {
            if (m_multi_compile == null)
            {
                m_multi_compile = new List<List<string>>();
            }

            contents = contents.Replace("#pragma", "");
            contents = contents.Trim();

            contents = contents.Replace("multi_compile_local", "");
            contents = contents.Trim();

            //处理系统定义
            if (contents.StartsWith("multi_compile_"))
            {
                List<string> tmpSystemVaniant = new List<string>();
                GetSystemVaniantByKey(contents, tmpSystemVaniant);

                if (tmpSystemVaniant.Count > 0)
                {
                    foreach (string t in tmpSystemVaniant)
                    {
                        m_multi_compile.Add(new List<string>() { "_", t });
                    }
                }
                return;
            }

            contents = contents.Replace("multi_compile", "");
            contents = contents.Trim();

            SplitContents(m_multi_compile, contents);
        }

        private void SplitContents(List<List<string>> data, string content)
        {
            List<string> tmpList = null;
            //#pragma shader_feature _NORMALMAP		//法线贴图

            #region 去除注释信息
            if (content.Contains("//"))
            {
                string[] remove = content.Split('/');

                content = remove[0];
            }
            #endregion

            string[] contents = content.Split(' ');

            for (int i = 0; i < contents.Length; i++)
            {
                string tmp = contents[i];

                if (string.IsNullOrEmpty(contents[i]))
                    continue;

                tmp = tmp.Trim();

                if (tmpList == null)
                {
                    tmpList = new List<string>();

                    data.Add(tmpList);
                }
                tmpList.Add(tmp);
            }
        }

        public void AppendSkipVariants(string skipVariants)
        {
            if (m_skipVariants == null)
            {
                m_skipVariants = new List<string>();
            }

            skipVariants = skipVariants.Replace("#pragma", "");
            skipVariants = skipVariants.Trim();

            skipVariants = skipVariants.Replace("skip_variants", "");
            skipVariants = skipVariants.Trim();

            string[] contents = skipVariants.Split(' ');

            for (int i = 0; i < contents.Length; i++)
            {
                string tmp = contents[i];

                if (string.IsNullOrEmpty(contents[i]))
                    continue;

                tmp = tmp.Trim();

                m_skipVariants.Add(tmp);
            }
        }

        #region 获取系统key

        private void GetSystemVaniantByKey(string key, List<string> systemVaniant)
        {
            if (key.Equals("multi_compile_fog"))
            {
                AppendVaniant(systemVaniant, "FOG_LINEAR");
                AppendVaniant(systemVaniant, "FOG_EXP");
                AppendVaniant(systemVaniant, "FOG_EXP2");
            }
            else if (key.Equals("multi_compile_instancing"))
            {
                AppendVaniant(systemVaniant, "INSTANCING_ON");
            }
        }

        private void AppendVaniant(List<string> systemVaniant,string vanaint)
        {
            if (!systemVaniant.Contains(vanaint) )
            {
                if (m_skipVariants != null && m_skipVariants.Contains(vanaint))
                    return;
                //检查目前的multi_compile和shader_feature 是否已经包含，必免重复/

                bool isAppend = true;

                if (m_multi_compile != null && m_multi_compile.Count > 0)
                {
                    foreach (List<string> t in m_multi_compile)
                    {
                        if (t != null)
                        {
                            if (t.Contains(vanaint))
                            {
                                isAppend = false;
                                break;
                            }
                        }
                    }
                }

                if (isAppend && m_shaderFeature != null && m_shaderFeature.Count > 0)
                {
                    foreach (List<string> t in m_shaderFeature)
                    {
                        if (t != null)
                        {
                            if (t.Contains(vanaint))
                            {
                                isAppend = false;
                                break;
                            }
                        }
                    }
                }

                if (isAppend)
                {
                    systemVaniant.Add(vanaint);
                }
            }
        }

        #endregion
    }
    #endregion

    #region 系统pass节点
    public class SystemPASSNode
    {
        public UnityEngine.Rendering.PassType m_passType = UnityEngine.Rendering.PassType.ScriptableRenderPipeline;
        public Shader m_shader;
        public string[] m_keywords;
    }

    #endregion


    //variant收集根目录
    private static string s_VCPath = "G_Artist/Shader/Shadervariant/SVC";
    private static List<string> s_VCPathfilter = new List<string>();
    //variant文件名
    private static string s_VCName = "start.shadervariants";    
    private static bool s_CollectApart = false;
    //Material存放路径 /Effect/Chenghao
    private static string[] s_MatSearchPaths = new string[] {
        "G_Artist",
        "G_Resources",
        "Game",
    };
    private static List<string> s_MatSearchShaders = new List<string>();

    private static List<Material> s_allMaterial = new List<Material>();

    private static Dictionary<string, SVCNode> s_SVCDict = new Dictionary<string, SVCNode>();

    private static ShaderVariantCollection s_SVC = null;

    private static List<string> s_Variants = new List<string>();

    private static int s_systemShader = 0;

    [MenuItem("XGame/Res Tools/Shader/Variant Collections (Apart)")]
    public static void CollectVariantApart()
    {
        s_CollectApart = true;

        //收集的总的SVC
        s_VCPath = "G_Resources/Game/Shadervariant";
        s_VCPathfilter.Clear();
        s_MatSearchShaders.Clear();
        s_VCName = "GameShadervariant.shadervariants";
        VariantCollections();

        //------------------------------------------------------------分界线//------------------------------------------------------------
        //这个是美术自己搞的,按理说应该放到和我们同一个目录里面去,现在只是单独收集个变体防止真机报错
        /*
        s_VCPath = "G_Artist/Shader";
        s_VCPathfilter.Clear();
        s_MatSearchShaders.Clear();
        s_VCName = "EffectsShadervariants.shadervariants";
        VariantCollections();
        */

    }

    public static void VariantCollections()
    {
        s_SVCDict.Clear();

        ShowProgress("Collect Materials...", 0, 1);
        FindAllMaterial();
        ShowProgress("Collect Materials Finish! Materials count:" + s_allMaterial.Count, 1, 1);

        GenVariantCollectionByPath();

        GenShaderVariantByList();

        //手动配置部分系统变体/
        if (!s_CollectApart)
        {
            AppendSVCBySystem();
        }

        /*
        string configPath = Path.Combine("Assets/G_Resources/Game/ScriptableObjects",
        Path.GetFileNameWithoutExtension(s_VCName) + ".asset");

        LighadEngine.ToolkitsEditor.EditorHelper.MakeSureDirectoryExist(Path.GetDirectoryName(configPath));

        var holder = AssetDatabase.LoadAssetAtPath<ShaderVariantCollectionHolder>(configPath);
        if (holder == null)
        {
            holder = ScriptableObject.CreateInstance<ShaderVariantCollectionHolder>();
            AssetDatabase.CreateAsset(holder, configPath);
        }

        holder.svc = s_SVC;
        EditorUtility.SetDirty(holder);
        */


        // holder = ScriptableObject.CreateInstance<ShaderVariantCollectionHolder>();

        /*
        string vcShaderPath = "Assets/" + s_VCPath + "/" + s_VCName;
        if(File.Exists(Application.dataPath+"/"+vcShaderPath))
        {
            File.Delete(vcShaderPath);
        }

        AssetDatabase.CreateAsset(s_SVC, vcShaderPath);
        */

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    #region 查找全部Material
    private static void FindAllMaterial()
    {
        s_allMaterial.Clear();
        List<string> filePaths = new List<string>();
        for (int i = 0; i < s_MatSearchPaths.Length; i++)
        {        
            filePaths.AddRange(Directory.GetFiles(Path.Combine("Assets", s_MatSearchPaths[i]), "*.mat", SearchOption.AllDirectories));
        }

        Material tmpMat;
        for (int i = 0; i < filePaths.Count; i++)
        {
            string path = filePaths[i];
            tmpMat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (tmpMat != null)
            {
                s_allMaterial.Add(tmpMat);
            }
        }
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }

    #endregion

    #region 获得变体实例
    private static void GenVariantCollectionByPath()
    {
        string pathVC = Path.Combine(Path.Combine(Application.dataPath, s_VCPath), s_VCName);
        /*if (!File.Exists(pathVC))
        {
            Debug.LogFormat("Shader Variant Collections Not Found: {0}",pathVC);
            return;
        }*/
        string path = GetRelativeAssetsPath(pathVC);
        s_SVC = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path);
        if (s_SVC == null)
        {
            s_SVC = new ShaderVariantCollection();
            AssetDatabase.CreateAsset(s_SVC, path);
        }
        s_SVC.Clear();
    }
    #endregion

    #region 生成变体信息

    private static void GenShaderVariantByList()
    {
       
        for (int i = 0; i < s_allMaterial.Count; i++)
        {
            ShowProgress("Handle Material...", i, s_allMaterial.Count);

            Material mat = s_allMaterial[i];

            if (mat != null)
            {
                if (mat.shader != null)
                {
                    GenVariantByMaterial(mat);
                }
            }
        }

        Debug.Log(s_allMaterial.Count + "  finish");
    }

    // 清理掉注释信息
    private static string PreProcessScript(string text)
    {
        StringBuilder sb = new StringBuilder();
        int cur = 0;
        int pos = text.IndexOf('/');
        char[] NewLine = new char[] { '\r', '\n' };
        while(pos >= 0 && pos <text.Length-1)
        {
            char ch = text[pos + 1];
            if (ch == '*') // 多行注释
            {
                if (pos > cur)
                {
                    sb.Append(text, cur, pos - cur);
                }
                int endPos = text.IndexOf('*', pos + 2);
                while(endPos >= pos + 2 && endPos<text.Length-1 && text[endPos+1] != '/')
                {
                    endPos = text.IndexOf('*', endPos + 1);
                }
                if (endPos < pos+2 || endPos >= text.Length - 1)
                {
                    // 语法错误了
                    return null;
                }
                cur = endPos + 2;
                pos = text.IndexOf('/', cur);
            }else if (ch == '/') // 单行注释
            {
                if (pos > cur)
                {
                    sb.Append(text, cur, pos - cur);
                 }
                int endPos = text.IndexOfAny(NewLine, pos + 2);
                cur = (endPos >= pos + 2) ? endPos : text.Length;
                pos = text.IndexOf('/', cur);
            }
            else // 不是注释
            {
                pos = text.IndexOf('/', pos + 1);
            }
        }
        if (cur < text.Length)
        {
            sb.Append(text, cur, text.Length - cur);
        }
        return sb.ToString();
    }

    //[MenuItem("UnitTest/ScriptPreprocess")]
    //public static void spp_test()
    //{
    //    foreach (string path in Directory.GetFiles(Path.Combine(EditorPathUtil.serverBinPath, "UITexture/Shader")))
    //    {
    //        string text = PreProcessScript(File.ReadAllText(path));
    //        if (text != null)
    //        {
    //            File.WriteAllText(Path.Combine(Path.Combine(Path.GetDirectoryName(path), "Result"), "pp_" + Path.GetFileName(path)), text);
    //        }
    //    }
    //}

    //通过svc路径才收集
    private static bool IsPassSVCPath(string path)
    {
        if (!path.Contains(s_VCPath))
        {
            return true;
        }
        return false;
    }

    //通过svc过滤路径的不收集
    private static bool IsPassSVCPathFilter(string path) {
        if (s_VCPathfilter.Count <= 0) return false;

        foreach (string filterPath in s_VCPathfilter)
        {
            if (path.Contains(filterPath))
            {
                return true;
            }
        }
        return false;
    }

    //通过s_MatSearchShaders里面的shader才收集
    private static bool IsPassSearchShaderFilter(Shader path)
    {
        if (s_MatSearchShaders.Count <= 0) return false;

        foreach (string filterPath in s_MatSearchShaders)
        {
            if (path.name == filterPath)
            {
                return false;
            }
        }
        return true;
    }

     
    private static void GenVariantByMaterial(Material mat)
    {
        Shader shader = mat.shader;

        string path = AssetDatabase.GetAssetPath(shader);
        if (!s_CollectApart) return;

        //if (IsPassSVCPath(path)) return;
        if (IsPassSVCPathFilter(path)) return;
        if (IsPassSearchShaderFilter(shader)) return;

        path = path.Replace("Assets/", "");

        Debug.Log(path);

        path = Application.dataPath + "/" + path;


        SVCNode svcNode = null;

        if (File.Exists(path))
        {
            string contents = File.ReadAllText(path);//读取相应路径文件的全部内容
            contents = PreProcessScript(contents);
            try
            {
                svcNode = ShaderTextToList(contents, shader.name, mat);
            }catch(Exception ex)
            {
                Debug.LogErrorFormat("{0} : 解释Shader文件错误。", path);
                Debug.LogException(ex);
            }
        }

        s_Variants.Clear();

        if (svcNode != null)
        {
            CombinationVariant(svcNode, s_Variants, mat);

            if(s_Variants.Count==0)
            {
                
            }
        }

       
    }

    #region 通过文本获得shader variant信息
    private static SVCNode ShaderTextToList(string shaderContents,string shaderPath,Material tMat)
    {
        SVCNode tInfo = null;
        if (s_SVCDict.ContainsKey(shaderPath))
        {
            s_SVCDict.TryGetValue(shaderPath, out tInfo);
            return tInfo;
        }

        if (string.IsNullOrEmpty(shaderContents))
        {
            return tInfo;
        }

        tInfo = new SVCNode();

        s_SVCDict.Add(shaderPath, tInfo);
        
        tInfo.m_LODInfo = new Dictionary<string, SVCLODNode>();

        SVCLODNode newLodInfo = null;

        SVCLODPASSNode passNode = null;

        int subShaderRefCount = 0;

        string tmpSave = "";

        string[] shaderDesc = shaderContents.Split('\n');


        for (int i = 0; i < shaderDesc.Length; i++)
        {
            string tmp = shaderDesc[i];
            tmp = tmp.Replace("\t", "");
            tmp = tmp.Replace("\r", "");
            tmp = tmp.Trim();

            if (tmp.Contains("SubShader"))
            {
                subShaderRefCount = int.MaxValue;
                if (tInfo.m_LODInfo == null)
                {
                    tInfo.m_LODInfo = new Dictionary<string, SVCLODNode>();
                }
            }

            if (tmp.StartsWith("LOD"))
            {
                newLodInfo = new SVCLODNode();

                if (tInfo.m_LODInfo.ContainsKey(tmp))
                {
                    Debug.LogWarning("The shader have duplicate lod defined! Shader:" + shaderPath, tMat.shader);
                }
                tInfo.m_LODInfo[tmp] = newLodInfo;
                newLodInfo.m_variantLODInfo = new List<SVCLODPASSNode>();
            }

            #region pass区间处理

            if (tmp.ToLower().Contains("pass"))
            {
                #region 部分无LOAD Shader

                if (newLodInfo == null)
                {
                    newLodInfo = new SVCLODNode();

                    tInfo.m_LODInfo.Add("LOD "+i, newLodInfo);

                    newLodInfo.m_variantLODInfo = new List<SVCLODPASSNode>();
                }
                #endregion

                passNode = new SVCLODPASSNode();
                newLodInfo.m_variantLODInfo.Add(passNode);
            }
            if (passNode !=null) {
                if (tmp.Contains("LightMode"))
                {
                    passNode.m_passType = GenPassTypeByStr(tmp);
                }

                if (tmp.Contains("multi_compile"))
                {
                    passNode.AppendMultiCompile(tmp);
                }

                if (tmp.Contains("shader_feature"))
                {
                    passNode.AppendShaderFeature(tmp);
                }

                if (tmp.Contains("skip_variants"))
                {
                    passNode.AppendSkipVariants(tmp);
                }
            }
             
            string szLower = tmp.ToLower();
            if (szLower.Contains("fallback")) {
                string[] szLowers = szLower.Split(' ');
                for (int n = 0; n < szLowers.Length; n++) {
                    string splitLower = szLowers[n].Trim();
                    if (splitLower.Contains("diffuse") || splitLower.Contains("vertexlit")) {
                        Debug.LogWarning("Not Allowed Shader Fallback", tMat.shader);
                    }
                }
            }
            #endregion

            #region 对subShader区间进行判断{++ }-- 当值为0代表本subShader结束
            if (tmp.Contains("{"))
            {
                if (subShaderRefCount == int.MaxValue)
                {
                    subShaderRefCount = 1;
                }
                else
                {
                    subShaderRefCount++;
                }
            }

            if (tmp.Contains("}"))
            {
                subShaderRefCount--;
            }

            if (newLodInfo != null)
            {
                tmpSave += tmp +"\n";
                if (subShaderRefCount <= 0)
                {
                    Debug.Log(tmpSave);
                    tmpSave = "";
                    
                    newLodInfo = null;
                }
            }
            #endregion
        }
        return tInfo;
    }

    #region 单个pass中的LightModel type
    private static UnityEngine.Rendering.PassType GenPassTypeByStr(string info)
    {
        /*
        Normal = 0,
        Vertex = 1,
        VertexLM = 2,
        VertexLMRGBM = 3,
        ForwardBase = 4,
        ForwardAdd = 5,
        LightPrePassBase = 6,
        LightPrePassFinal = 7,
        ShadowCaster = 8,
        Deferred = 10,
        Meta = 11,
        MotionVectors = 12,
        ScriptableRenderPipeline = 13,
        ScriptableRenderPipelineDefaultUnlit = 14
        */
        if (info.Contains("ForwardBase"))
        {
            return UnityEngine.Rendering.PassType.ForwardBase;
        }
        else if (info.Contains("ForwardAdd"))
        {
            return UnityEngine.Rendering.PassType.ForwardAdd;
        }
        else if (info.Contains("Vertex"))
        {
            return UnityEngine.Rendering.PassType.Vertex;
        }
        else if (info.Contains("LightPrePassBase"))
        {
            return UnityEngine.Rendering.PassType.LightPrePassBase;
        }
        else if (info.Contains("LightPrePassFinal"))
        {
            return UnityEngine.Rendering.PassType.LightPrePassFinal;
        }
        else if (info.Contains("ShadowCaster"))
        {
            return UnityEngine.Rendering.PassType.ShadowCaster;
        }
        else if (info.Contains("Deferred"))
        {
            return UnityEngine.Rendering.PassType.Deferred;
        }
        else if (info.Contains("Meta"))
        {
            return UnityEngine.Rendering.PassType.Meta;
        }
        else if (info.Contains("MotionVectors"))
        {
            return UnityEngine.Rendering.PassType.MotionVectors;
        }
        else if (info.Contains("UniversalForward"))
        {
            return UnityEngine.Rendering.PassType.ScriptableRenderPipeline;
        }
        return UnityEngine.Rendering.PassType.ScriptableRenderPipeline;
    }
    #endregion

    #endregion

    #endregion

    #region 组合变体

    //组合变体/
    public static void CombinationVariant(SVCNode node,List<string> variants, Material mat)
    {
        foreach (KeyValuePair<string, SVCLODNode> svcLODNode in node.m_LODInfo)
        {
            if (svcLODNode.Value.m_variantLODInfo == null)
                continue;

            foreach (SVCLODPASSNode svcPassNode in svcLODNode.Value.m_variantLODInfo)
            {
                Debug.Log(svcLODNode.Key);
                string variantContents = "";
                //有shader没有变体 Hair_Alpha 
                if (svcPassNode != null)
                {
                    AddSVCByMat(svcPassNode, mat, ref variantContents);
                }
            }
        }
    }
    
    private static void SVCPassCombination(SVCLODPASSNode passNode, Material mat,ref string variantContents)
    {
        List<List<string>> shaderVariant = new List<List<string>>();

        #region 移除不存在mat中的keywords
        if (passNode.m_shaderFeature != null)
        {
            List<string> matKeywords = new List<string>(mat.shaderKeywords);
            for (int i = 0; i < passNode.m_shaderFeature.Count; i++)
            {
                List<string> result = null;
                List<string> nextNode = passNode.m_shaderFeature[i];

                for (int j = 0; j < nextNode.Count; j++)
                {
                    string s = nextNode[j];

                    if (matKeywords.Contains(s))
                    {
                        if (result == null)
                        {
                            result = new List<string>();
                        }
                        result.Add(s);
                    }
                }

                if (result != null && result.Count > 0)
                {
                    shaderVariant.Add(result);
                }
            }
        }
        //附加multi_compile
        if (passNode.m_multi_compile != null)
        {
            shaderVariant.AddRange(passNode.m_multi_compile);
        }
        #endregion

        List<string[]> tmp = new List<string[]>();

        for (int i = 0; i < shaderVariant.Count; i++)
        {
            tmp.Add(shaderVariant[i].ToArray());
        }

        if (tmp.Count <= 0) return;

        string[] combinationResult;
        combinationResult = CombinationOperation(tmp);

        if (combinationResult != null)
        {
            for (int i = 0; i < combinationResult.Length; i++)
            {
                ShaderVariantCollection.ShaderVariant s1 = new ShaderVariantCollection.ShaderVariant();
                s1.shader = mat.shader;
                if (!string.IsNullOrEmpty(combinationResult[i]))
                {
                    string[] tmpKeywords = combinationResult[i].Split(' ');
                    if (tmpKeywords != null && tmpKeywords.Length > 0)
                    {
                        s1.keywords = tmpKeywords;
                    }
                }
                s1.passType = passNode.m_passType;
                if (s1.passType != PassType.Meta)
                {
                    s_SVC.Add(s1);
                }               
            }
        }
        else if(mat.shader != null)
        {
            ShaderVariantCollection.ShaderVariant s1 = new ShaderVariantCollection.ShaderVariant();
            s1.shader = mat.shader;
            s1.passType = passNode.m_passType;
            if (s1.passType != PassType.Meta)
            {
                s_SVC.Add(s1);
            }
        }

       
        if(combinationResult.Length>32)
        {
            Debug.LogError("超標 mat = " + mat.name + ", shader=" + mat.shader.name + ", variantCount: " + combinationResult.Length);
        }

        Debug.Log("mat = "+mat.name+", shader="+ mat.shader.name+ ", variantCount: "+combinationResult.Length);
    }


    private static void AddSVCByMat(SVCLODPASSNode passNode, Material mat, ref string variantContents)
    {
        //List<List<string>> shaderVariant = new List<List<string>>();

        ShaderVariantCollection.ShaderVariant s1 = new ShaderVariantCollection.ShaderVariant();
        s1.shader = mat.shader;
        s1.passType = passNode.m_passType;
        s1.keywords = mat.shaderKeywords;
        if (s1.passType != PassType.Meta)
        {
            s_SVC.Add(s1);
        }

  





   }

    //组合运算
    private static string[] CombinationOperation(List<string[]> al)
    {
        if (al.Count == 0)
            return null;
        int size = 1;
        for (int i = 0; i < al.Count; i++)
        {
            size = size * al[i].Length;
        }
        string[] str = new string[size];
        for (int j = 0; j < size; j++)
        {
            for (int m = 0; m < al.Count; m++)
            {
                string curStr = al[m][(j * CombinationOperationEx(al, m) / size) % al[m].Length];

                bool isAppend = !curStr.Equals("_");

                if (isAppend)
                {
                    isAppend = !curStr.StartsWith("__");
                }

                if (!string.IsNullOrEmpty(curStr) && isAppend)
                {
                    if (!string.IsNullOrEmpty(str[j]))
                    {
                        str[j] += " ";
                    }
                    str[j] = str[j] + curStr;
                }
            }
            //Debug.Log(str[j]);
        }
        return str;
    }
    private static int CombinationOperationEx(List<string[]> al, int m)
    {
        int result = 1;
        for (int i = 0; i < al.Count; i++)
        {
            if (i <= m)
            {
                result = result * al[i].Length;
            }
            else
            {
                break;
            }
        }
        return result;
    }

    #endregion

    #region 将系统变体加入SVC 中

    private static void AppendSVCBySystem()
    {
        s_systemShader = 1;

        List<SystemPASSNode> tmpSystem = new List<SystemPASSNode>();

        GenSystemPass("Hidden/BlitCopy", tmpSystem);

        //GenSystemPass("Standard", tmpSystem);

        GenSystemPass("Skybox/Cubemap", tmpSystem);

        GenSystemPass("Particles/Additive", tmpSystem);

        GenSystemPass("Particles/Alpha Blended", tmpSystem);

        //GenSystemPass("Hidden/Internal-GUITextureClip", tmpSystem);

        //GenSystemPass("Hidden/Internal-GUITextureClipText", tmpSystem);

        //GenSystemPass("Hidden/Internal-GUITexture", tmpSystem);

        //GenSystemPass("Hidden/Internal-GUITextureBlit", tmpSystem);

        GenSystemPass("UI/Default", tmpSystem);

        GenSystemPass("Hidden/CubeBlur", tmpSystem);

        GenSystemPass("Hidden/CubeCopy", tmpSystem);

        foreach (SystemPASSNode t in tmpSystem)
        {
            ShaderVariantCollection.ShaderVariant s1 = new ShaderVariantCollection.ShaderVariant();
            s1.shader = t.m_shader;
            s1.keywords = t.m_keywords;
            s1.passType = t.m_passType;
            s_SVC.Add(s1);
        }
    }

    private static void GenSystemPass(string key, List<SystemPASSNode> systemNode)
    {
        ShowProgress("Handle System Shader...", s_systemShader, 13);
        Shader tmp =  Shader.Find(key);

        if (key.Equals("Hidden/BlitCopy"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal,null));
        }
        else if (key.Equals("Standard"))
        {
            //systemNode.Add(InstanceVariantNode(tmp, PassType.VertexLM, SplitSystemKeywords("FOG_EXP2")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL LIGHTPROBE_SH")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL LIGHTPROBE_SH SHADOWS_SCREEN" )));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_LINEAR LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL DIRLIGHTMAP_COMBINED FOG_LINEAR LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_EXP LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL DIRLIGHTMAP_COMBINED FOG_EXP LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_EXP2")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_EXP2 LIGHTMAP_ON")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL DIRLIGHTMAP_COMBINED FOG_EXP2 LIGHTMAP_ON")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_EXP2 LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_EXP2 LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SCREEN SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL DIRLIGHTMAP_COMBINED FOG_EXP2 LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL DIRLIGHTMAP_COMBINED FOG_EXP2 LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_EXP2 LIGHTPROBE_SH")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardBase, SplitSystemKeywords("DIRECTIONAL FOG_EXP2 LIGHTMAP_SHADOW_MIXING LIGHTPROBE_SH SHADOWS_SCREEN")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardAdd, SplitSystemKeywords("DIRECTIONAL")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardAdd, SplitSystemKeywords("DIRECTIONAL SHADOWS_SCREEN")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ForwardAdd, SplitSystemKeywords("DIRECTIONAL FOG_EXP2")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.ShadowCaster, SplitSystemKeywords("DIRECTIONAL FOG_EXP2")));
            //systemNode.Add(InstanceVariantNode(tmp, PassType.Meta, new string[] { }));
        }
        else if (key.Equals("Skybox/Cubemap"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }
        else if (key.Equals("Particles/Additive"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, SplitSystemKeywords("FOG_LINEAR" )));
        }
        else if (key.Equals("Particles/Alpha Blended"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, SplitSystemKeywords("FOG_LINEAR" )));
        }
        else if (key.Equals("Hidden/Internal-GUITextureClip"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }
        else if (key.Equals("Hidden/Internal-GUITextureClipText"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }
        else if (key.Equals("Hidden/Internal-GUITexture"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }
        else if (key.Equals("Hidden/Internal-GUITextureBlit"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }
        else if (key.Equals("Hidden/BlitCopy"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }
        else if (key.Equals("UI/Default"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, SplitSystemKeywords("UNITY_UI_CLIP_RECT")));
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, SplitSystemKeywords("UNITY_UI_ALPHACLIP")));
        }
        else if (key.Equals("Hidden/CubeBlur"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }
        else if (key.Equals("Hidden/CubeCopy"))
        {
            systemNode.Add(InstanceVariantNode(tmp, PassType.Normal, null));
        }

        s_systemShader++;
    }

    public static SystemPASSNode InstanceVariantNode(Shader tShader,PassType tPassType,string[] keywords)
    {
        SystemPASSNode node = new SystemPASSNode();

        node.m_shader = tShader;
        node.m_passType = tPassType;
        node.m_keywords = keywords;

        return node;
    }

    public static string[] SplitSystemKeywords(string key)
    {
        return key.Split(' ');
    }

    #endregion

    private static void ShowProgress(string msg, int progress, int total)
    {
        EditorUtility.DisplayProgressBar("VariantCollectionsEditor", string.Format("{0}...{1}/{2}", msg, progress, total), progress * 1.0f / total);
    }
}


/*******************************************************************
** 文件名: SpineAssets.cs
** 版  权:    (C) 深圳冰川网络有限公司 
** 创建人:     许德纪
** 日  期:    2024/11/30
** 版  本:    1.0
** 描  述:    Spine 资源同步工具
** 应  用:    

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/



using Spine.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using XClient.Scripts;
using XGame.I18N;
using XGame.UI;

namespace XGameEditor
{
    public class SpineAssets
    {
        [MenuItem("XGame/资源工具/同步spine加载路径")]
        public static void SynSpineLoadPath()
        {
            string[] filePaths = Directory.GetFiles("Assets", "*.asset", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; ++i)
            {
                SkeletonDataAsset sd = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(filePaths[i]);
                if (sd)
                {
                    if (sd.skeletonJSON)
                    {
                        sd.skeletonJsonPath = AssetDatabase.GetAssetPath(sd.skeletonJSON);
                        sd.skeletonJsonPath = sd.skeletonJsonPath.Replace("Assets/", "");
                        EditorUtility.SetDirty(sd);

                    }
                } else
                {
                    SpineAtlasAsset sa = AssetDatabase.LoadAssetAtPath<SpineAtlasAsset>(filePaths[i]);
                    if (sa && sa.atlasFile)
                    {
                        sa.atlasPath = AssetDatabase.GetAssetPath(sa.atlasFile);
                        sa.atlasPath = sa.atlasPath.Replace("Assets/", "");
                        EditorUtility.SetDirty(sa);
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }



        [MenuItem("XGame/资源工具/增加Spine索引组件")]
        public static void AddSpineComponent()
        {
            string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; ++i)
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(filePaths[i]);
                if (go)
                {
                    SkeletonAnimation sa = go.GetComponentInChildren<SkeletonAnimation>();
                    SkeletonGraphic sg = go.GetComponentInChildren<SkeletonGraphic>();
                    if (sa || sg)
                    {
                        bool bChange = false;

                        SpineComponent sc = go.GetComponent<SpineComponent>();
                        if (sc == null)
                        {
                            sc = go.AddComponent<SpineComponent>();
                            bChange = true;
                        }

                        if (sc.skeAni != sa || sc.skeGra != sg)
                        {
                            sc.skeAni = sa;
                            sc.skeGra = sg;
                            bChange = true;
                        }

                        if (bChange)
                        {
                            PrefabUtility.SavePrefabAsset(go);
                        }



                    }

                }

            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem("XGame/资源工具/设置材质的GPUInstance")]
        public static void EnableGPUInstance()
        {
            string[] filePaths = Directory.GetFiles("Assets", "*.mat", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; ++i)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(filePaths[i]);
                if(null!=m)
                {
                    m.enableInstancing = true;
                }
               
            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem("XGame/资源工具/增加SpineTimeLine组件和移除Missd对象")]
        public static void RemoveTimeLineUnrefrenceObect()
        {
            bool bChage = false;
            string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; ++i)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePaths[i]);
                if (prefab)
                {

                   

                    var director = prefab.GetComponent<PlayableDirector>();
                    if (director == null)
                    {
                        continue;
                    }
                    var timelineAsset = director.playableAsset as TimelineAsset;
                    if (timelineAsset == null)
                    {
                        continue;
                    }

      


                    Dictionary<UnityEngine.Object, UnityEngine.Object> bindings = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

                    foreach (var pb in director.playableAsset.outputs)
                    {
                       
                        var key = pb.sourceObject;
                        //director.ClearGenericBinding(key);
                        if (key==null)
                        {
                            continue;
                        }
                        var value = director.GetGenericBinding(key);
                        if (!bindings.ContainsKey(key)&& key!=null)
                        {
                            bindings.Add(key, value);
                        }else
                        {
                            if (key != null&&value != null)
                            {
                                bindings[key] = value;
                            }
                        }

                       
                    }


                    GameObject.DestroyImmediate(director,true);

                    var spineTimeLine = prefab.GetComponent<SpineTimeLine>();
                    if(null== spineTimeLine)
                    {
                        prefab.AddComponent<SpineTimeLine>();
                    }

                    director = prefab.AddComponent<PlayableDirector>();
                   
                    director.playableAsset = timelineAsset;
                    director.playOnAwake = false;
                    foreach (var pair in bindings)
                    {
                        director.SetGenericBinding(pair.Key, pair.Value);
                    }
                  
                    PrefabUtility.SavePrefabAsset(prefab);
     
                    bChage = true;


                }

            }

            if(bChage)
            {
                AssetDatabase.SaveAssets();
            }
            
        }

        [MenuItem("XGame/资源工具/替换GraphicRaycaster")]
        public static void RePlaceGraphicRaycaster()
        {
            bool bChage = false;
            bool bSave = false;
            string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; ++i)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePaths[i]);
                if (prefab)
                {


                    bSave = false;
                    GraphicRaycaster[] aryGR = prefab.GetComponentsInChildren<GraphicRaycaster>();
                    int count = aryGR.Length;
                    if (count> 0)
                    {
                        for (int j = 0; j < count; ++j)
                        {
                            var gr = aryGR[j];
                            GameObject go = gr.gameObject;
                            GameObject.DestroyImmediate(gr, true);
                            gr = go.AddComponent<GraphicRaycasterEx>();
                            bSave = true;

                        }
                    }

                    //清除多余的 GraphicRaycasterEx
                    GraphicRaycaster[] aryGREx = prefab.GetComponentsInChildren<GraphicRaycasterEx>();
                    for (int j = 0; j < aryGREx.Length; ++j)
                    {
                        var gr = aryGREx[j];
                        if(null!= gr)
                        {
                            GameObject go = gr.gameObject;
                            if(null!= go)
                            {
                                //全部删除，再增加，恢复层次
                                GraphicRaycasterEx[] curGR = go.GetComponents<GraphicRaycasterEx>();
                                for(int k=1;k< curGR.Length;++k)
                                {
                                    GameObject.DestroyImmediate(curGR[k], true);
                                    bSave = true;
                                }

                               
                            }
                        }
                        
                    }




                    if(bSave)
                    {
                        PrefabUtility.SavePrefabAsset(prefab);
                    }

                   

                    bChage = true;


                }

            }

            if (bChage)
            {
                AssetDatabase.SaveAssets();
            }

        }



        [MenuItem("XGame/资源工具/隐藏ScrollBarImage")]
        public static void HideScrollBarImage()
        {
            string[] BarsName = { "Scrollbar Horizontal", "Scrollbar Vertical" };
            bool bChage = false;
            string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; ++i)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePaths[i]);
                if (prefab)
                {


                    bChage = false;
                    int nLen = BarsName.Length;
                    for(int j=0;j<nLen;++j)
                    {
                        HideGameObjectBar(prefab.transform, BarsName[j],ref bChage);
                    }


                    Debug.Log("prefab name:" + prefab.name);

                    if(bChage)
                    {
                        PrefabUtility.SavePrefabAsset(prefab);
                    }
                   

                   


                }

            }

           AssetDatabase.SaveAssets();

        }

        private static void HideGameObjectBar(Transform go,string barName,ref bool bChage)
        {
            Transform cur = go.transform;
            if(cur.name== barName)
            {

                bChage = true;

                //设置缩放为0 
                cur.localScale =  Vector3.zero;

                //隐藏image组件
                Image img = cur.GetComponent<Image>();
                if(img!=null)
                {
                    img.enabled = false;
                }

                //隐藏handle image脚本
                Transform Handle = cur.Find("Handle");
                if(null!= Handle)
                {
                    img = Handle.GetComponent<Image>();
                    if (img != null)
                    {
                        img.enabled = false;
                    }
                }
            }

            //处理子对象
            int nCount = go.transform.childCount;
            for(int i=0;i<nCount;++i)
            {
                HideGameObjectBar(cur.GetChild(i), barName,ref bChage);
            }
          
        }



        [MenuItem("XGame/资源工具/禁用RaycastTarget")]
        public static void DisableRaycastTarget()
        {
            List<Graphic> listGrahics = new List<Graphic>();
            string[] filePaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; ++i)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePaths[i]);

                bool bChange = false;

                //字体
                 prefab.GetComponentsInChildren<Graphic>(listGrahics);
                for(int j=0;j< listGrahics.Count;++j)
                {

                    if(listGrahics[j] as Image|| listGrahics[j] as TextMeshPro || listGrahics[j] as LocalizeTextMeshPro || listGrahics[j] as Text|| listGrahics[j] as RawImage)
                    {
                        if (listGrahics[j] as Image)
                        {
                            GameObject go = listGrahics[j].gameObject;
                            if (go.GetComponent<Button>() != null ||
                                go.GetComponent<PressButton>() != null
                               // go.GetComponent<ToggleButton>() != null 
                             
                                )
                            {
                                continue;
                            }

                        }
                        if (listGrahics[j].raycastTarget)
                        {
                            listGrahics[j].raycastTarget = false;
                            bChange = true;


                        }
                    }

                   
                    
                }

                if(bChange)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
                
            }

            AssetDatabase.SaveAssets();

        }

    }
}

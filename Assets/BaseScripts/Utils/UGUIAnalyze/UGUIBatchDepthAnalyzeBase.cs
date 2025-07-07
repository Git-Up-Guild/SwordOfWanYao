/************************************************************************************
* Copyright (c) 2020 All Rights Reserved.
*命名空间：Assets.Scripts.Scripts.Analyze.UGUIBatchDepthAnalyze
*文件名： UGUIBatchDepthAnalyzeBase.cs
*创建人： 敖贤文
*创建时间：2020/7/30 18:24:53 
*描述
*=====================================================================
*修改标记
*修改时间：
*修改人：
*描述：
************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UGUIBatchDepthAnalyzeBase
{
    //记录UI物体数据
    protected Dictionary<Transform, GameObjectData> dict = new Dictionary<Transform, GameObjectData>();

    //记录UI层级（不是深度哦，就是普通的我们认识的层级）
    protected Dictionary<Transform, int> orderDict = new Dictionary<Transform, int>();

    
    /// <summary>
    /// UI物体数据
    /// </summary>
    public class GameObjectData
    {
        public GameObject go; //物体
        public int materialID;//材质ID
        public int depth;     //深度
        public int batchID;   //合批ID
        public int textureID; //贴图ID
    }
    /// <summary>
    /// 获得数据之前
    /// </summary>
    protected virtual void BeforeBake(GameObject root)
    {

    }
    /// <summary>
    /// 获得GameobjectData数据
    /// root - 例如UIMonitorWindow ，这个window要有canvas
    /// </summary>
    public Dictionary<Transform, GameObjectData> GetBakeData(GameObject root)
    {
        BeforeBake(root);
        HandleBakeData(root);
        AfterBake(root);
        return dict;
    }
    protected virtual void HandleBakeData(GameObject root)
    {

    }
    protected virtual void AfterBake(GameObject root)
    {

    }
    protected void CheckCanvasBatchDepthMateri(Canvas canvas, Dictionary<Transform, GameObjectData> dic)
    {
        //搜索整个Canvas的子物体们的合批、深度、材质(深 度)        
        if (canvas != null)
        {
            //开始分析            
            AnalyzeUI(canvas.transform, dic);
        }
    }
    /// <summary>
    /// 深度优先级遍历 空物体不渲染，没有render，所以深度永远是0
    /// 深度看是否能合批，和合批用上一个低级的，不可判断是否相交，不相交还是一样
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="dict"></param>
    private void AnalyzeUI(Transform trans, Dictionary<Transform, GameObjectData> dict)
    {
        Transform[] childs = GetChildren(trans);
        if (!IsNeedRenderer(trans))
        {
            //自己不需要渲染，但是孩子需要渲染 2020/08/07
            //继续深入
            if (childs != null && childs.Length > 0)
            {
                for (int i = 0; i < childs.Length; i++)
                {
                    AnalyzeUI(childs[i], dict); //递归
                }
            }//end new
            return;
        }
        Graphic renderer = trans.GetComponent<Graphic>();
        //根据子物体和它自身的信息获取depth
        //1. 无子物体时，深度为0
        //2. 有1个子物体且相交时，可Batch时，深度为子物体深度；不可Batch时，深度为子物体深度+1
        //3. 有n个子物体且相交时，根据(2.)规则计算所有depth_i, 取max(depth_1, depth_2, ... depth_x);作为层级   

        //物体数据
        GameObjectData gameObjectData = new GameObjectData()
        {
            go = trans.gameObject
        };
        if (renderer != null)
        {
            gameObjectData.materialID = renderer.material.GetInstanceID();
            gameObjectData.textureID = renderer.mainTexture.GetNativeTexturePtr().ToInt32();
        }
        //深度计算开始
        //1.没有下层物体情况
        //获取下层物体列表
        List<Transform> downObjArr = new List<Transform>();
        int curLayer = 0;
        if (orderDict.TryGetValue(trans, out curLayer))
        {
            foreach (var v in orderDict)
            {
                if (v.Value < curLayer)
                {
                    downObjArr.Add(v.Key);
                }
            }
        }
        else
        {
            Debug.LogError("获取不到当前层级" + trans);
            return;
        }
        //1.没有下层物体时 canvas
        if (downObjArr.Count == 0)
        {
            gameObjectData.depth = 0;
        }
        //第2.3种情况有子物体 canvas下的各种物体,image
        if (downObjArr != null && downObjArr.Count > 0)
        {
            //2. 有1个下层物体且相交时，可Batch时，深度为下层物体深度；不可Batch时，深度为下层物体深度+1 更高级的与更低级的进行比较，看更高级是否与更低级相交，相交才处理判断
            if (downObjArr.Count == 1)
            {
                if (IsNeedRenderer(downObjArr[0]))
                {
                    //相交时
                    if (downObjArr[0].GetComponent<RectTransform>().Overlaps(trans.GetComponent<RectTransform>()))
                    {
                        gameObjectData.depth = CalculateDepth(trans, downObjArr[0], dict);
                    }
                }
            }
            else
            {
                //3.有n个下层物体且相交时，根据(2.)规则计算所有depth_i, 取max(depth_1, depth_2, ... depth_x);作为层级   
                List<int> depthList = new List<int>();
                for (int i = 0; i < downObjArr.Count; i++)
                {
                    if (IsNeedRenderer(downObjArr[i]))
                    {
                        //相交时
                        if (trans.GetComponent<RectTransform>().Overlaps(downObjArr[i].GetComponent<RectTransform>()))
                        {
                            int count = CalculateDepth(trans, downObjArr[i], dict);
                            depthList.Add(count);
                        }
                    }
                }
                depthList.Sort((a, b) =>
                {
                    return a == b ? 0 : a > b ? -1 : 1;
                });
                if (depthList.Count >= 1)
                {
                    gameObjectData.depth = depthList[0];//取最大的depth
                }
            }
        }
        Canvas canvas = trans.GetComponent<Canvas>();
        if (canvas == null)
        {
            dict.Add(trans, gameObjectData);
        }
        //继续深入
        if (childs != null && childs.Length > 0)
        {
            for (int i = 0; i < childs.Length; i++)
            {
                AnalyzeUI(childs[i], dict); //递归
            }
        }
    }
    //计算深度，根据判断是否合批来计算highObj的深度返回出去 , lowObj是下层物体, dict是一个存储数据的字典
    private int CalculateDepth(Transform highObj, Transform lowObj, Dictionary<Transform, GameObjectData> dict)
    {
        GameObjectData data;
        dict.TryGetValue(lowObj.transform, out data);
        if (data != null)
        {
            if (IsCanBatch(highObj, lowObj))
            {
                return data.depth;//depth为子物体的深度
            }
            else
            {
                return data.depth + 1; //depth为子物体的深度 + 1            
            }
        }
        //data 等于空 代表是highObj自己上面是Canvas不是其他,自己就用和canvas一样的0，第一个
        if (lowObj.GetComponent<Canvas>() == null&&IsNeedRenderer(lowObj)) //判断lowObj是否可以渲染，不可渲染，直接返回0
        {
            Debug.LogError(highObj + "," + lowObj + ":居然没有深度,导致highObj的深度为-1");
            return -1;
        }
        else
        {
            return 0;//Canvas本身就没深度为0
        }
    }

    /// <summary>
    /// 判断2个物体是否可合批 (根据实际情况可能需要改动，因为暂且无法确保包含所有情况 本人水平差）
    /// 1、比较材质(材质本身、材质上的贴图）
    /// 2、若比较材质相同时，继续比较图片的贴图（非材质贴图）即Image的贴图
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    private bool IsCanBatch(Transform t1, Transform t2)
    {
        //检查材质是否相同来判断是否可以Batch
        bool isCan = false;
        //比较材质
        Graphic g1 = t1.GetComponent<Graphic>();
        Graphic g2 = t2.GetComponent<Graphic>();
        if (g1 == null || g2 == null)
        {
            //Debug.Log(t1 + "," + t2 + "Graphic其中一个为空 可合批");
            isCan = true;
        }
        else
        {
            Material m1 = g1.material;
            Material m2 = g2.material;
            if (m1 == null || m2 == null)
            {
                //Debug.Log(t1 + "," + t2 + "Material其中一个为空 可合批"); //这种可能是不正确的，可能材质其中一个为空代表不能合批
                isCan = true;
            }
            else
            {
                if (m1.Equals(m2))
                {
                    //比较纹理
                    Texture tex1 = m1.mainTexture;
                    Texture tex2 = m2.mainTexture;
                    if (tex1 == null && tex2 != null || tex1 != null && tex2 == null)//其中一个为空，另一个不为空，则表示材质实际不同
                    {
                        //Debug.Log(t1 + "," + t2 + "材质的纹理其中一个为空 不可合批！");
                        isCan = false;
                    }
                    if (tex1 == null && tex2 == null)
                    {
                        //Debug.Log(t1 + "," + t2 + "材质的纹理都为空 可合批！");
                        isCan = true;
                    }
                    if (tex1 != null && tex2 != null)
                    {
                        if (tex1.Equals(tex2))
                        {
                            //Debug.Log(t1 + "," + t2 + "材质的纹理相同 可合批！");
                            isCan = true;
                        }
                        else
                        {
                            //Debug.Log(t1 + "," + t2 + "材质的纹理不相同 不可合批！");
                            isCan = false;
                        }
                    }
                }
            }
            //当材质认为一样时（材质一样，材质贴图一样时），继续考虑Graphic的图片情况(即是否属于同一个图集)
            if (isCan)
            {
                //考虑完材质 考虑图片的纹理图
                Texture mainTex1 = g1.mainTexture;
                Texture mainTex2 = g2.mainTexture;
                if (mainTex1 == null && mainTex2 != null) { /*Debug.Log(t1 + "," + t2 + "图片贴图其中一个为空 不可合批！");*/ isCan = false; }
                if (mainTex1 != null && mainTex2 == null) { /*Debug.Log(t1 + "," + t2 + "图片贴图其中一个为空 不可合批！");*/ isCan = false; }
                if (mainTex2 == null && mainTex2 == null) { /*Debug.Log(t1 + "," + t2 + "图片贴图2个都为空，可以合批！！");*/ isCan = true; }

                if (mainTex1 != null && mainTex2 != null)
                {
                    if (mainTex1.Equals(mainTex2))
                    {
                        //Debug.Log(t1 + "," + t2 + "图片贴图相同 可合批!！");
                        isCan = true;
                    }
                    else
                    {
                        //Debug.Log(t1 + "," + t2 + "图片贴图不相同 不可合批!！");
                        isCan = false;
                    }
                }
            }
        }
        return isCan;
    }
    /// <summary>
    /// 获取其下所有子物体
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    private Transform[] GetChildren(Transform trans)
    {
        if (trans == null || trans.childCount <= 0)
        {
            return null;
        }
        Transform[] childs = new Transform[trans.childCount];
        for (int i = 0; i < trans.childCount; i++)
        {
            childs[i] = trans.GetChild(i);
        }
        return childs;
    }

    /// <summary>
    /// 获取UI层级队列
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="trans"></param>
    /// <param name="order"></param>
    protected void GetUIOrder(Dictionary<Transform, int> dict, Transform trans, ref int order)
    {
        if (trans == null) return;
        dict.Add(trans, order++);
        if (trans.childCount > 0)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                GetUIOrder(dict, trans.GetChild(i), ref order);
            }
        }
    }
    /// <summary>
    /// 是否需要渲染
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    private bool IsNeedRenderer(Transform trans)
    {
        //TODO 不渲染的情况都要排除... 还差考虑组件的 Mask CanvasGroup 等情况
        Graphic renderer = trans.GetComponent<Graphic>();
        if (trans == null || !trans.gameObject.activeInHierarchy || (renderer != null && renderer.color.a == 0))
        {
            return false;
        }
        return true;
    }

}

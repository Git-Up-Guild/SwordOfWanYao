/************************************************************************************
* Copyright (c) 2020 All Rights Reserved.
*命名空间：Assets.Scripts.Scripts.Analyze.UGUIBatchDepthAnalyze
*文件名： UGUIBatchDepthRunningAnalyze.cs
*创建人： 敖贤文
*创建时间：2020/7/30 18:19:33 
*描述
*=====================================================================
*修改标记
*修改时间：
*修改人：
*描述：
************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class UGUIBatchDepthRunningAnalyze : UGUIBatchDepthAnalyzeBase
{
    private bool isAddCanvas = false;
    protected override void BeforeBake(GameObject root)
    {
        base.BeforeBake(root);
        //检查是否有canvas
        if (root != null)
        {
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                isAddCanvas = true;
                canvas = root.AddComponent<Canvas>();
            }
        }
    }
    protected override void HandleBakeData(GameObject root)
    {
        if (root != null)
        {
            base.HandleBakeData(root);
            int order = 0;
            orderDict.Clear();
            GetUIOrder(orderDict, root.transform, ref order);
            //重置开始深度计算标记
            //计算UI深度、材质ID、贴图ID
            if (dict != null)
            {
                dict.Clear();
            }
            CheckCanvasBatchDepthMateri(root.GetComponent<Canvas>(), dict);
            //排序UI物体数据
            List<GameObjectData> list = new List<GameObjectData>();
            foreach (var v in dict.Values)
            {
                list.Add(v);
            }
            //从小到大排序
            list.Sort((a, b) =>
            {
                //1.根据深度
                if (a.depth != b.depth)
                {
                    return a.depth < b.depth ? -1 : 1;
                }
                //2.根据材质
                if (a.materialID != b.materialID)
                {
                    return a.materialID < b.materialID ? -1 : 1;
                }
                //3.根据贴图
                if (a.textureID != b.textureID)
                {
                    return a.textureID < b.textureID ? -1 : 1;
                }
                //4.根据UI层级
                int orderA = 0;
                int orderB = 0;
                orderDict.TryGetValue(a.go.transform, out orderA);
                orderDict.TryGetValue(b.go.transform, out orderB);
                return orderA < orderB ? -1 : 1;
            });
            //计算UI 合批ID
            int batchID = 0;
            if (list.Count > 0)
            {
                list[0].batchID = batchID;
                for (int i = 1; i < list.Count; i++)
                {
                    //按顺序判断材质和贴图是否一样 一样的则为一个合批ID
                    //相邻层 材质ID和贴图ID相同时 为一个批次，即可合批！ （待测试是否正确）
                    GameObjectData data = list[i];
                    GameObjectData lastData = list[i - 1];
                    //深度代表不能合批，而且相交
                    if (data.depth != lastData.depth || data.materialID != lastData.materialID || data.textureID != lastData.textureID)
                    {
                        batchID++;
                    }
                    data.batchID = batchID;
                }
            }
            //打印排序后的物体数据情况
            for (int i = 0; i < list.Count; i++)
            {
                Debug.Log(list[i].go + "," + list[i].depth + "," + +list[i].materialID + "," + list[i].textureID + "," + orderDict[list[i].go.transform] + "," + list[i].batchID);
            }
        }
    }
    protected override void AfterBake(GameObject root)
    {
        if (root != null)
        {
            base.AfterBake(root);
            if (isAddCanvas)
            {
                isAddCanvas = false;
                UnityEngine.Object.Destroy(root.GetComponent<Canvas>());
            }
        }
    }
}

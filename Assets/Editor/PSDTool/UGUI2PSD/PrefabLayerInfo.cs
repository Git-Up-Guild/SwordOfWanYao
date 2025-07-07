using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI2PSD
{
    public class PrefabLayerInfo
    {
        private Transform m_trans;
        private List<PrefabLayerBase> prefabLayers;

        public PrefabLayerInfo()
        {
            prefabLayers = new List<PrefabLayerBase>();
        }

        public PrefabLayerInfo(Transform prefab) : this()
        {
            m_trans = prefab;
            AnalysisPrefab(m_trans);    //分析预制体
            CreateExtendLayerInfo();    //生成额外信息层
        }

        public List<PrefabLayerBase> GetPrefabLayerList()
        {
            return prefabLayers;
        }

        public ExtendLayer ExtendLayer { get; private set; }

        public string GetPrefabName()
        {
            return m_trans.name;
        }

        private void AnalysisPrefab(Transform tran)
        {
            int count = tran.childCount;
            if (count > 0)
            {
                //先添加一个组开头
                PrefabLayerBase groupStartLayer = PrefabLayerFactory.CreateLayer(EPrefabLayerType.GroupStart, tran);
                prefabLayers.Add(groupStartLayer);

                //处理子类层信息
                for (int i = count - 1; i >= 0; i--)
                {
                    Transform temp = tran.GetChild(i);
                    AnalysisPrefab(temp);   //递归处理所有层信息
                }

                //判断组root是否有图片或者文本（因为ps里面一个图片下面不能有图片，所以要加组来实现，作为第一个子obj）
                bool isHas = HasTextOrImageComp(tran);
                if (isHas)
                {
                    //如果有的话，要添加自己的layer
                    PrefabLayerBase selfLayer = AutoAddLayer(tran);
                    prefabLayers.Add(selfLayer);
                }

                //最后要添加组结尾
                PrefabLayerBase groupEndLayer = PrefabLayerFactory.CreateLayer(EPrefabLayerType.GroupEnd, tran);
                prefabLayers.Add(groupEndLayer);
            }
            else
            {
                //表示没有子级了，就直接添加自己即可
                bool isHas = HasTextOrImageComp(tran);
                if (isHas)
                {
                    PrefabLayerBase selfLayer = AutoAddLayer(tran);
                    prefabLayers.Add(selfLayer);
                }
                else
                {
                    //表示是一个空的子物体
                    //那么把它当成一个组来处理吧
                    PrefabLayerBase start = PrefabLayerFactory.CreateLayer(EPrefabLayerType.GroupStart, tran);
                    PrefabLayerBase end = PrefabLayerFactory.CreateLayer(EPrefabLayerType.GroupEnd, tran);
                    prefabLayers.Add(start);
                    prefabLayers.Add(end);
                }
            }
        }

        private void CreateExtendLayerInfo()
        {
            PrefabLayerBase extLayer = PrefabLayerFactory.CreateLayer(EPrefabLayerType.Extend, m_trans);
            if (extLayer.IsResolveSuccess)
                prefabLayers.Insert(1, extLayer);   //将预制体信息插入第一位(根节点下最外层)

            ExtendLayer = extLayer as ExtendLayer;
        }

        private bool HasTextOrImageComp(Transform tran)
        {
            return tran.GetComponent<Text>() != null || tran.GetComponent<Image>() != null;
        }

        private PrefabLayerBase AutoAddLayer(Transform tran)
        {
            PrefabLayerBase result = null;
            if (tran.GetComponent<Text>())
            {
                result = PrefabLayerFactory.CreateLayer(EPrefabLayerType.Text, tran);
            }
            if (tran.GetComponent<Image>())
            {
                result = PrefabLayerFactory.CreateLayer(EPrefabLayerType.Image, tran);
            }
            return result;
        }
    }
}
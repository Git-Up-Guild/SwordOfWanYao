using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UGUI2PSD
{
    public class ExtendLayer : PrefabLayerBase
    {
        public override EPrefabLayerType LayerType => EPrefabLayerType.Extend;

        public string Key { get; private set; }
        public byte[] PrefabData { get; private set; }

        public ExtendLayer(Transform tran) : base(tran)
        {
            Visible = false;    //扩展层不显示
            Key = UGUI2PSDTool.UserLayerName;   //用户自定义层
            PrefabData = new byte[0];
            ResolveLayerInfo();
            ReCalculateSuitableRect();
        }


        public override void ResolveLayerInfo()
        {
            string assetPath = AssetDatabase.GetAssetPath(trans.gameObject);
            if (assetPath.Contains("Assets/"))
            {
                string fullPath = Application.dataPath + "/../" + assetPath;
                PrefabData = File.ReadAllBytes(fullPath);
                Print($"{assetPath} - 字节数：{PrefabData.Length}");
            }
            else
            {
                IsResolveSuccess = false;
                Debug.LogError($"{trans.name}不是预制体，无法对其生成扩展层信息！");
                return;
            }
        }

        //重新计算合适的rect，避免通道数据过大，影响生成和解析效率
        private void ReCalculateSuitableRect()
        {
            int count = PrefabData.Length;
            int perChannelLength = Mathf.CeilToInt(count * 0.25f); //4通道，ARGB
            int width = Mathf.CeilToInt(Mathf.Sqrt(perChannelLength));
            PsRect = new Rect(0, 0, width, width);
        }
    }
}

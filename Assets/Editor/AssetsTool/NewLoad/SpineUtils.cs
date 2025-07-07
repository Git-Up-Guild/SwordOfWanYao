
using Spine.Unity;
using Spine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XGameEditor.NewLoad
{
    public class SpineUtils : Editor
    {
        [MenuItem("Assets/XGame/导出所有皮肤名称")]
        static void ExportSkin()
        {
            // 获取当前选择的对象
            Object selectedObject = Selection.activeObject;

            // 检查所选对象是否是 SkeletonData
            if (selectedObject is SkeletonDataAsset skeletonDataAsset)
            {
                // 获取 SkeletonData
                SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);

                // 导出皮肤到文本文件
                ExportSkinsToTextFile(skeletonData);
            }
            else
            {
                Debug.LogError("Please select a valid SkeletonDataAsset.");
            }
        }
        static void ExportSkinsToTextFile(SkeletonData skeletonData)
        {
            string filePath = "Assets/ExportedSkins.txt";  // 文本文件的路径

            // 打开文件流（如果文件不存在，会自动创建）
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                // 写入文件头
                //writer.WriteLine("List of Skins:");

                List<string> strings = new List<string>();
                // 遍历所有皮肤并将其名称写入文件
                foreach (var skin in skeletonData.Skins)
                {
                    strings.Add(skin.Name);
                    //writer.WriteLine(skin.Name);
                }
                strings.Sort();
                for (int i = 0; i < strings.Count; i++)
                {
                    writer.WriteLine(strings[i]);
                }
            }

            // 刷新 AssetDatabase，确保文件显示在 Unity 编辑器中
            AssetDatabase.Refresh();

            Debug.Log($"Skins exported to {filePath}");
        }
    }
}
using PhotoshopFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using PhotoshopFile.Text;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

namespace UGUI2PSD
{
    public static class UGUI2PSDTool
    {
        public const string GroupEndPostfix = "_End";
        public const string UserLayerName = "UserLayer";
        public const string SettingPath = "Assets/XGameEditor/Editor/PSDTool/PSD2UGUI/Resources/UGUI2PSD_Setting.asset";
        public const string TextPatternPath = "/XGameEditor/Editor/PSDTool/PSD2UGUI/Resources/TextPattern.txt";  //文本层样板数据

        private const string TempFileName = "UGUI2PSD_Temp.prefab";
        //组件替换匹配器
        private static Type[] ReplaceMatcher = new Type[] {
            /*typeof(RectTransform),*/ typeof(Image), typeof(Text), typeof(Outline)
        };

        //传入数据到channel
        public static void SaveDataToChannel(Layer layer, byte[] allData)
        {
            int index = 0;
            int sum = 0;
            int channelCount = layer.Channels.Count;
            for (int i = 0; i < channelCount; i++)
            {
                Channel channel = layer.Channels[i];
                int dataLength = channel.ImageData.Length;
                for (int j = 0; j < dataLength; j++)
                {
                    if (index < allData.Length - 1)
                    {
                        channel.ImageData[j] = allData[index++];
                    }
                    else
                    {
                        channel.ImageData[j] = 0;
                    }
                }
                sum += dataLength;
            }

            int remainCount = allData.Length - sum;
            if (remainCount > 0)
            {
                Debug.LogError($"还有{remainCount}个字节未写入，将会有数据丢失！");
            }
        }

        //从channel取出数据
        public static byte[] GetDataFromChannel(Layer layer)
        {
            List<byte> readData = new List<byte>();

            int channelCount = layer.Channels.Count;
            for (int i = 0; i < channelCount; i++)
            {
                Channel channel = layer.Channels[i];
                if (channel.ImageData == null)
                    Debug.LogError("channel.ImageData == null");
                int dataLength = channel.ImageData.Length;
                for (int j = 0; j < dataLength; j++)
                {
                    byte data = channel.ImageData[j];
                    if (data != 0)
                        readData.Add(data);
                }
            }

            return readData.ToArray();
        }

        //设置物体组件信息（根据psd的用户层数据，即预制体信息数据来设置）
        public static void UpdateGameObjectByPsdLayer(GameObject root, Layer userLayer)
        {
            if (root)
            {
                //保留最原始物体，不修改，万一psd导出那边还要处理
                //先处理原物体
                ProcessSourceGameObject(root.transform);

                //然后开始生成保存的预制体
                byte[] ImageData = GetDataFromChannel(userLayer);
                //创建临时文件
                string filePath = Application.dataPath + "/" + TempFileName;
                File.WriteAllBytes(filePath, ImageData);
                //强制刷新unity
                AssetDatabase.Refresh();

                //然后加载这个文件资源的obj
                string relativePath = "Assets/" + TempFileName;
                GameObject tempObj = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
                if (tempObj)
                {
                    //Debug.Log("<color=blue>加载tempObj成功！</color>");

                    //加载物体到界面显示
                    GameObject go = GameObject.Instantiate(tempObj, root.transform.parent);
                    if (go)
                    {
                        go.name = root.name + "_New";
                        go.transform.localPosition = Vector3.zero;
                        go.transform.localEulerAngles = Vector3.zero;
                        go.transform.localScale = Vector3.one;
                        //PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                        //给tempObj增删更新组件
                        UpdateComponents(root, go);

                        //删除root物体
                        GameObject.DestroyImmediate(root);
                    }
                }
                else
                {
                    Debug.LogError("加载资源失败：" + relativePath);
                }

                //删除临时文件
                File.Delete(filePath);
                AssetDatabase.Refresh();
            }
        }

        //先加工处理psd直接导出的物体（主要是层级）
        private static void ProcessSourceGameObject(Transform srcTran)
        {
            List<GameObject> waitDeleteObjList = new List<GameObject>();
            foreach (Transform item in srcTran.transform)
            {
                if (item.name == item.parent.name)
                {
                    waitDeleteObjList.Add(item.gameObject);
                    //将子物体的相关组件挂载到父物体上
                    MergeComponentsValues(item.transform, item.parent);
                }
                else
                {
                    ProcessSourceGameObject(item);
                }
            }

            //删除多余物体
            foreach (var item in waitDeleteObjList)
            {
                GameObject.DestroyImmediate(item);
            }
        }

        //复制组件(包括所有子类的)
        private static void UpdateComponents(GameObject newObj, GameObject oldObj)
        {
            //方法1：递归调用每一层物体的组件，动态添加，但是物体间的相互引用关系不好处理，现不采用此种
            //方法2：由于新物体是在旧物体的基础上修改的，所以对旧物体的信息修改（大致3种情况：物体增删、图片引用修改、文本样式修改）即可达到目的，物体间引用关系等不会改变，这里采用此种方法
            //先合并根节点的信息
            MergeComponentsValues(newObj.transform, oldObj.transform);
            //再遍历合并下面的子物体的
            MergeNewObjToOldObj(newObj, oldObj);
        }

        //合并新物体信息到旧物体
        private static void MergeNewObjToOldObj(GameObject newObj, GameObject oldObj)
        {
            //方法1：递归调用每一层物体的组件，动态添加，但是物体间的相互引用关系不好处理，现不采用此种
            //方法2：由于新物体是在旧物体的基础上修改的，所以对旧物体的信息修改（大致3种情况：物体增删、图片引用修改、文本样式修改）即可达到目的，物体间引用关系等不会改变，这里采用此种方法
            List<GameObject> waitDeleteObjList = new List<GameObject>();
            foreach (Transform oldChild in oldObj.transform)
            {
                Transform newChild = newObj.transform.Find(oldChild.name);  //新的预制体是否还有保留此物体
                if (newChild)
                {
                    //如果有的话，合并组件数值
                    MergeComponentsValues(newChild, oldChild);

                    //然后递归判断子物体
                    MergeNewObjToOldObj(newChild.gameObject, oldChild.gameObject);
                }
                else
                {
                    waitDeleteObjList.Add(oldChild.gameObject);
                }
            }

            //检查新预制体是否有添加物体
            foreach (Transform item in newObj.transform)
            {
                bool isHas = oldObj.transform.Find(item.name) != null;  //旧预制体是否有此物体
                if (!isHas)
                {
                    //没有就复制一个加过去
                    GameObject newChild = GameObject.Instantiate(item.gameObject);
                    newChild.transform.SetParent(oldObj.transform);
                    newChild.transform.localPosition = item.transform.localPosition;
                    newChild.transform.localRotation = item.transform.localRotation;
                    newChild.transform.localScale = item.transform.localScale;
                    newChild.name = item.name;
                }
            }

            //删除多余物体
            foreach (var item in waitDeleteObjList)
            {
                GameObject.DestroyImmediate(item);
            }

            //检查子类个数是否一致
            int oldObjChildCount = oldObj.transform.childCount;
            int newObjChildCount = newObj.transform.childCount;
            if (oldObjChildCount != newObjChildCount)
            {
                Debug.LogError($"合并后子类数量不对, newObj: {newObj}, oldObj: {oldObj}");
            }

            //最后更新子物体的顺序
            foreach (Transform item in newObj.transform)
            {
                int index = item.GetSiblingIndex();
                Transform old = oldObj.transform.Find(item.name);
                if (old)
                {
                    old.SetSiblingIndex(index);
                }
            }
        }

        //

        //合并组件信息
        private static void MergeComponentsValues(Transform srcTran, Transform destTran)
        {
            //批量替换（如果两个物体的指定种类的组件不一样（包括一个有一个没有的情况），则会替换）
            ComponentUtility.ReplaceComponentsIfDifferent(srcTran.gameObject, destTran.gameObject, IsDesiredComponent);

            //同步位置
            destTran.position = srcTran.position;

            #region 单个替换
            /*Component[] components = destTran.GetComponents<Component>();
            foreach (Component comp in components)
            {
                Type compType = comp.GetType();
                if (compType == typeof(RectTransform))  //位置信息
                {
                    RectTransform rectTran = comp as RectTransform;

                }
                else if (compType == typeof(Image))     //图片
                {

                }
                else if (compType == typeof(Text))      //文本
                {

                }
                else if (compType == typeof(Outline))     //描边
                {

                }
            }*/
            #endregion
        }

        //是否是期望替换得组件
        private static bool IsDesiredComponent(Component comp)
        {
            Type compType = comp.GetType();
            bool isDesired = Array.IndexOf(ReplaceMatcher, compType) != -1;
            //Debug.Log($"物体：{comp.gameObject.name} - 组件：{compType}，是否替换：{isDesired}");
            return isDesired;
        }

        private static void MergeComponent(Component src, Component dest)
        {
            ComponentUtility.CopyComponent(src);
            ComponentUtility.PasteComponentValues(dest);
        }


        //****************************相关测试************************************

        [MenuItem("XGame/PSD工具/反向生成相关测试/测试加载预制体")]
        public static void TestLoad()
        {
            string relativePath = "Assets/" + TempFileName;
            string newPath = "Assets/Test.prefab";
            File.Copy(relativePath, newPath, true);
            //强制刷新unity
            AssetDatabase.Refresh();

            GameObject tempObj = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
            if (tempObj)
            {
                int childCount = tempObj.transform.childCount;
                Debug.Log(childCount);
            }
            Debug.Log(tempObj);
        }


        [MenuItem("XGame/PSD工具/反向生成相关测试/组件替换测试")]
        public static void TestReplaseComp()
        {
            GameObject A = GameObject.Find("A");
            GameObject B = GameObject.Find("B");

            MergeComponentsValues(A.transform, B.transform);
        }

        [MenuItem("XGame/PSD工具/创建预制体导出psd配置")]
        public static void CreateUgui2PsdSetting()
        {
            Ugui2Psd_Setting setting = AssetDatabase.LoadAssetAtPath<Ugui2Psd_Setting>(SettingPath);
            if (setting == null)
            {
                setting = Ugui2Psd_Setting.CreateInstance<Ugui2Psd_Setting>();
                setting.OutputBasePath = Application.dataPath + "/../";

                string fullFilePath = Application.dataPath + "/../" + SettingPath;
                string dirPath = Path.GetDirectoryName(fullFilePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                AssetDatabase.CreateAsset(setting, SettingPath);
            }
            Selection.activeObject = setting;
        }

        [MenuItem("XGame/PSD工具/预制体导出PSD")]
        public static void CreateNewPSD()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                try
                {
                    CreatePsdFile cPsdFile = new CreatePsdFile(go.transform);
                }
                catch (TdTaParseException e)
                {
                    Debug.LogError(e.Message);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace XGameEditor
{
      #if !UNITY_PACKING

    /// 作者：_Walker__
    /// 链接：https://www.jianshu.com/p/632869a87848
    public class ModelMatTool : AssetPostprocessor
    {
        private static bool m_NeedWaiting = false;

        /// <summary>
        /// 是否启用删除操作
        /// OnPostprocessModel回调在模型导入的时候就会调到，
        /// 通过这个标记位保证只在调用脚本函数的时候执行。
        /// </summary>
        private static bool m_EnableDelete = false;

        /// <summary>
        /// 批量删除模型上的材质
        /// </summary>
        public static IEnumerator DelModelMats(Object[] models)
        {
            foreach (Object obj in models)
            {
                while (m_NeedWaiting) yield return null;
                DelModelMat(obj as GameObject);
            }
        }

        /// <summary>
        /// 删除模型上绑定的材质
        /// </summary>
        /// <param name="model">模型对象</param>
        public static void DelModelMat(GameObject model)
        {
            if (null == model) return;
            string assetPath = AssetDatabase.GetAssetPath(model);
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (null == importer) return;
            m_EnableDelete = true;
            m_NeedWaiting = true;

            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            AssetDatabase.ImportAsset(assetPath);
        }

        private void OnPostprocessModel(GameObject model)
        {
            if (null == model)
            {
                return;
            }

            if (!m_EnableDelete)
            {
                return;
            }
            m_EnableDelete = false;

            Renderer[] renders = model.GetComponentsInChildren<Renderer>();
            if (null == renders) return;
            foreach (Renderer render in renders)
            {
                render.sharedMaterial = new Material(Shader.Find("Mobile/Diffuse"));
                render.sharedMaterials = new Material[render.sharedMaterials.Length];
            }
            m_NeedWaiting = false;
        }

        protected virtual Material OnAssignMaterialModel(Material previousMaterial, Renderer renderer)
        {
            var materialPath = "Assets/G_Artist/Effect/Materials/Default_Material.mat";

            if (AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)))
            {
                return AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            }

            Debug.LogErrorFormat("renderer[{0}] assign default material failed, assign to {1}", renderer.name, previousMaterial.name);

            return previousMaterial;
        }

        [MenuItem("Assets/XGame/删除选中模型的默认材质")]
        static void DelSelectedModelMat()
        {
            Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            if (null == objs) return;
            //EditorCoroutine.Start(DelModelMats(objs));

            foreach (Object obj in objs)
            {
                DelModelMat(obj as GameObject);
            }
        }
    }
#endif
}
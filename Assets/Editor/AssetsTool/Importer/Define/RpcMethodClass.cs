using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XGameEditor.AssetImportTool
{
    public class RpcMethodClass
    {
        private static bool isDebug = true;
        private static void Debuger(string msg)
        {
            if (isDebug) Debug.Log(msg);
        }

        //检测路径
        public static string CheckPath(AssetImporter importer)
        {
            if (importer) return importer.assetPath;
            else return null;
        }

        //通用检查属性或字段值
        public static object CommonCheckProperty(AssetImporter importer, string propName)
        {
            if (importer)
            {
                return AssetImportHelper.GetPropertyOrFieldValue(importer, propName);
            }
            return null;
        }

        //检查参数值，透传
        public static object CheckParam(AssetImporter importer, string param)
        {
            if (importer)
            {
                return param;
            }
            return null;
        }

        //检查模型顶点数
        public static int CheckVertex(AssetImporter importer)
        {
            int minVerts = 100000;
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(importer.assetPath);
            if (go)
            {
                MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter f in filters)
                {
                    if (f.sharedMesh.vertexCount < minVerts)
                        minVerts = f.sharedMesh.vertexCount;
                }
            }
            Debuger($"{importer.assetPath} >> 最小Mesh顶点数：{minVerts}");
            return minVerts;
        }

        //检查模型是否被用作碰撞器了
        public static bool CheckCollider(AssetImporter importer)
        {
            return false;
        }

        //参与光照烘焙，并且非自己制作光照贴图UV时返回True
        public static bool CheckJoinBakeAndSystemUv(AssetImporter importer)
        {
            return false;
        }

        //检测模型是否使用了法线贴图
        public static bool CheckUseNormals(AssetImporter importer)
        {
            bool isUse = ((ModelImporter)importer).importNormals != ModelImporterNormals.None;
            Debuger("检测模型是否使用了法线贴图: " + isUse);
            return isUse;
        }

        //检测模型是否带蒙皮
        public static bool CheckHasSkinnedMesh(AssetImporter importer)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(importer.assetPath);
            if (go)
            {
                SkinnedMeshRenderer[] filters = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                bool bHas = filters.Length > 0;
                Debuger("检测模型是否带蒙皮: " + bHas);
                return bHas;
            }
            return false;
        }

        //检测是否自己的骨架不通用，或者自己作为了通用骨架根，则返回True
        public static bool CheckSelfBone(AssetImporter importer)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(importer.assetPath);
            if (go)
            {
                Animator animator = go.GetComponent<Animator>();
                bool bUse = animator != null;
                Debuger("自己作为了通用骨架根: " + bUse);
                return bUse;
            }
            return false;
        }

        //检测带蒙皮模型是否使用通用骨架
        public static bool CheckUseCommonBone(AssetImporter importer)
        {
            bool isSkinnedMesh = CheckHasSkinnedMesh(importer);
            if (isSkinnedMesh)
            {
                ModelImporter modelImporter = importer as ModelImporter;
                if (modelImporter)
                {
                    string[] depends = AssetDatabase.GetDependencies(importer.assetPath, false);
                    bool bUse = depends.Length > 0;
                    Debuger("检测带蒙皮模型是否使用通用骨架: " + bUse);
                    return bUse;
                }
            }
            return false;
        }

        //检测模型是否带有动画
        public static bool CheckFbxHasAni(AssetImporter importer)
        {
            ModelImporter modelImporter = importer as ModelImporter;
            if (modelImporter)
            {
                bool bHas = modelImporter.clipAnimations.Length > 0;
                Debuger("检测模型是否带有动画: " + bHas);
                return bHas;
            }
            return false;
        }


        //检测Texture是否带有透明通道
        public static bool CheckHasAlphaChannel(AssetImporter importer)
        {
            TextureImporter textureImporter = importer as TextureImporter;
            if (textureImporter)
            {
                bool bHas = textureImporter.DoesSourceTextureHaveAlpha();
                Debuger("检测Texture是否带有透明通道：" + bHas);
                return bHas;
            }
            return false;
        }


        //检测Texture的alpha通道是否表示透明程度
        public static bool CheckAlphaIsTransparency(AssetImporter importer)
        {
            Debuger("透明通道不表示透明度");
            return false;
        }

        //检查声音长度
        public static float CheckAudioLength(AssetImporter importer)
        {
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(importer.assetPath);
            if (audioClip)
            {
                Debuger("音效长度：" + audioClip.length);
                return audioClip.length;
            }
            return 0f;
        }

    }
}

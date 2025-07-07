using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace XGameEditor.ResTools
{
    public class TextureForamtHelper : EditorWindow
    {
        [MenuItem("XGame/Res Tools/Texture/Foramt")]
        public static void ShowWindow()
        {
            TextureForamtHelper resVerifier = EditorWindow.GetWindow(typeof(TextureForamtHelper)) as TextureForamtHelper;
            resVerifier.Show();
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Set Ios Textures From Select Assets"))
            {
                SetIosForamt();
            }
            GUILayout.EndVertical();
        }

        public void SetIosForamt()
        {
            ShowProgress("Collect textures...", 0, 1);
            List<string> assetPaths = GetAssetPathsFromSelection();
            for (int i = 0; i < assetPaths.Count; i++)
            {
                ShowProgress("Set texture...", i, assetPaths.Count);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPaths[i]);
                if (obj is Texture)
                {
                    SetIosTexture(obj as Texture);
                }
            }
            OnSetFinished();
        }

        private void SetIosTexture(Texture tex)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;

            int iosPlatformMaxTextureSize = 0;
            TextureImporterFormat iosPlatformTextureFmt;
            int iosPlatformCompressionQuality = 0;
            bool iosOverride = true;
            if (!importer.GetPlatformTextureSettings("iPhone", out iosPlatformMaxTextureSize, out iosPlatformTextureFmt, out iosPlatformCompressionQuality))
            {
                iosOverride = false;
            }

            bool isSave = false;

            if (iosPlatformTextureFmt != TextureImporterFormat.ASTC_4x4)
            {
                iosPlatformTextureFmt = TextureImporterFormat.ASTC_4x4;

                if (iosPlatformMaxTextureSize == 0)
                    iosPlatformMaxTextureSize = 1024;

                isSave = true;
            }

            if (isSave)
            {
                importer.SetPlatformTextureSettings("iPhone", iosPlatformMaxTextureSize, iosPlatformTextureFmt);

                importer.SaveAndReimport();
            }
        }

        private void OnSetFinished()
        {
            Debug.Log("设置结束！");
            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

        private List<string> GetAssetPathsFromSelection()
        {
            List<string> paths = new List<string>();
            List<string> selectPaths = new List<string>();
            string[] assetGUIDs = Selection.assetGUIDs;
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                selectPaths.Add(AssetDatabase.GUIDToAssetPath(assetGUIDs[i]));
            }
            for (int i = 0; i < selectPaths.Count; i++)
            {
                if (Directory.Exists(selectPaths[i]))
                {
                    paths.AddRange(Directory.GetFiles(selectPaths[i], "*", System.IO.SearchOption.AllDirectories));
                }
                else
                {
                    paths.Add(selectPaths[i]);
                }
            }
            for (int i = paths.Count - 1; i >= 0; i--)
            {
                if (paths[i].EndsWith(".meta"))
                {
                    paths.RemoveAt(i);
                }
            }
            return paths;
        }

        private void ShowProgress(string msg, int progress, int total)
        {
            EditorUtility.DisplayProgressBar(string.Format("ResourceVerifier", progress, total), string.Format("{0}...{1}/{2}", msg, progress, total), progress * 1.0f / total);
        }
    }
}
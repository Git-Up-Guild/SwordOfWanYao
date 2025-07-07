/*******************************************************************
** 文件名: TextureImportHelper.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 2015 - Speed
** 创建人:     郑秀程
** 日  期:    2015/5/28
** 版  本:    1.0
** 描  述:    批量修改贴图压缩格式
** 应  用:    

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
namespace XGameEditor.ResTools
{
    public class TextureImportHelper : EditorWindow
    {
        public TextureImporterFormat targetForamt = TextureImporterFormat.AutomaticCompressed;


        [MenuItem("XGame/Res Tools/Texture/TextureImportHelper")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(TextureImportHelper));
        }



        void OnGUI()
        {
            targetForamt = (TextureImporterFormat)EditorGUILayout.EnumPopup("Target Format", targetForamt);

            if (GUILayout.Button("Change Texture Format For Select Hierarchy"))
            {
                ChangeTextureFormatForSelectHierarchy();
            }

        }

        public void ChangeTextureFormatForSelectHierarchy()
        {
            MeshRenderer[] mrs = Selection.GetFiltered(typeof(MeshRenderer), SelectionMode.Deep).Select(obj => (MeshRenderer)obj).ToArray();
            string tmpPath;
            TextureImporter tmpTextureImporter;
            foreach (var mr in mrs)
            {
                foreach (var mat in mr.sharedMaterials)
                {
                    tmpPath = AssetDatabase.GetAssetPath(mat.mainTexture);
                    tmpTextureImporter = AssetImporter.GetAtPath(tmpPath) as TextureImporter;
                    tmpTextureImporter.textureFormat = targetForamt;
                    AssetDatabase.ImportAsset(tmpPath);
                }
            }
        }
    }
}
/*******************************************************************
** 文件名:	GAsset2BundleRecordsEditor.cs
** 版  权:	(C) 
** 创建人:	郑秀程 
** 日  期:	2021/4/20
** 版  本:	1.0
** 描  述:
** 应  用: 
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using XGame.Asset.Load;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace XGameEditor
{
    [CustomEditor(typeof(NewGAsset2BundleRecords))]
    public class GAsset2BundleRecordsEditor : Editor
    {
        private NewGAsset2BundleRecords records;
        private string filterName;

        [SerializeField]
        private List<EditorA2BRecord> recordsTmp;
        private SerializedObject tempSerialObj;
        private SerializedProperty tempRecoredsProperty;
        private bool isFilterMode;

        private void OnEnable()
        {
            records = target as NewGAsset2BundleRecords;
            recordsTmp = new List<EditorA2BRecord>();
        }

        private void OnDisable()
        {
            if (tempRecoredsProperty != null)
                tempRecoredsProperty.Dispose();
            tempRecoredsProperty = null;
            if (tempSerialObj != null)
                tempSerialObj.Dispose();
            tempSerialObj = null;

            isFilterMode = false;
        }

        public override void OnInspectorGUI()
        {
            bool bDoFilter = false;
            bool newRecords = false;
            EditorGUILayout.BeginHorizontal();
            string newFilterName = EditorGUILayout.TextField("要查找的资源名称：", filterName);
            if(GUILayout.Button("查询", GUILayout.Width(60)))
            {
                if(!string.IsNullOrEmpty(newFilterName))
                {
                    isFilterMode = true;
                    bDoFilter = true;
                }
                else
                    isFilterMode = false;
            }
            if (GUILayout.Button("清除", GUILayout.Width(60)))
            {
                newFilterName = null;
                isFilterMode = false;
            }
            EditorGUILayout.EndHorizontal();

            filterName = newFilterName;

            if (bDoFilter)
            {
                recordsTmp.Clear();
                newRecords = true;
                foreach (var r in records.allRecord.RecordList)
                {
                    if (r.assetName.ToLower().Contains(filterName.ToLower()))
                    {
                        recordsTmp.Add(r);
                    }
                }
            }


            if (!isFilterMode)
                base.OnInspectorGUI();
            else
            {
                if(newRecords)
                {
                    if (tempSerialObj != null)
                        tempSerialObj.Dispose();

                    if (tempRecoredsProperty != null)
                        tempRecoredsProperty.Dispose();

                    tempSerialObj = null;
                    tempRecoredsProperty = null;
                }

                if(tempSerialObj == null)
                    tempSerialObj = new SerializedObject(this);

                if(tempRecoredsProperty == null)
                    tempRecoredsProperty = tempSerialObj.FindProperty("recordsTmp");

                EditorGUILayout.PropertyField(tempRecoredsProperty);
            }
        }    
    }
}
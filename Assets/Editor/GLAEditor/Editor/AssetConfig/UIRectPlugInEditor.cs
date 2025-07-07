//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using XClient.UI;

//[CustomEditor(typeof(UIResPlugIn))]
//public class UIRectPlugInEditor : Editor
//{
//	int m_nIndex = -1;
//	string strIndex = "";


//	public override void OnInspectorGUI()
//	{
//		DrawDefaultInspector();

//		UIResPlugIn uiResPlugIn = target as UIResPlugIn;

//		if (GUILayout.Button("AddConfig"))
//		{
//			uiResPlugIn.SaveNode(uiResPlugIn.ExportNode());
//		}

//		CreateInputBox();

//		if (GUILayout.Button("UpdateConfig"))
//		{
//			uiResPlugIn.SaveNode(uiResPlugIn.ExportNode(), m_nIndex);
//		}

//		if (GUILayout.Button("DeleteConfig"))
//		{
//			uiResPlugIn.DeleteNode(m_nIndex);
//		}
//	}

//	private void CreateInputBox()
//	{
//		EditorGUILayout.BeginVertical("Box");

//		EditorGUILayout.BeginHorizontal("Box");

//		EditorGUILayout.LabelField("Input Index", GUILayout.Width(150));

//		m_nIndex = PlayerPrefs.GetInt("UIRectPlugInEditor_Index");
//		string strOldIndex = m_nIndex.ToString();

//		strIndex = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strIndex);

//		EditorGUILayout.EndHorizontal();

//		if (!strIndex.Equals(strOldIndex))
//		{
//			try
//			{
//				int nTmp = 0;
//				if (System.Int32.TryParse(strIndex, out nTmp))
//				{
//					UIResPlugIn uiResPlugIn = target as UIResPlugIn;
//					SUIResNode sNode = uiResPlugIn.GetNode(nTmp);
//					if (sNode != null)
//					{
//						uiResPlugIn.ImputNode(sNode, false);
//					}
//					m_nIndex = nTmp;
//					EditorUtility.SetDirty(target);
//					PlayerPrefs.SetInt("UIRectPlugInEditor_Index", m_nIndex);
//				}
//			}
//			catch (System.Exception e)
//			{ Debug.Log(e); }

//		}
//		m_nIndex = EditorGUILayout.IntField("Index", m_nIndex);
//		EditorGUILayout.EndVertical();
//	}

//}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XClient.UI;

//[CustomEditor(typeof(CommonWndModel))]
//public class CommonWndModelEditor : Editor {
//	Dictionary<string, string> dicOldParent = new Dictionary<string, string>();
//	Dictionary<string, string> dicParentStr = new Dictionary<string, string>();

//	string strDeleteIndex = "";

//	public override void OnInspectorGUI()
//	{
//		CommonWndModel commonWndModel = target as CommonWndModel;

//		CreateBox(ref commonWndModel.enMainFunctionBtn, "En Main Function Btn");

//		CreateBox(ref commonWndModel.enDefFunctionBtn, "En Def Function Btn");

//		DrawNodeConfigView(commonWndModel);

//		if (GUILayout.Button("AddConfig"))
//		{
//			CommonWndModelConfig asyncWindowConfig = EditorWindow.GetWindow(typeof(CommonWndModelConfig)) as CommonWndModelConfig;

//			asyncWindowConfig.InitConfig(target as CommonWndModel);
//			asyncWindowConfig.Show();
//		}

//		if (GUILayout.Button("UpdateConfig"))
//		{
//			CommonWndModelConfig asyncWindowConfig = EditorWindow.GetWindow(typeof(CommonWndModelConfig)) as CommonWndModelConfig;

//			asyncWindowConfig.InitConfig(target as CommonWndModel, true);
//			asyncWindowConfig.Show();
//		}

//		if (GUILayout.Button("SaveConfig"))
//		{
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//		}

//		EditorGUILayout.BeginVertical("Box");
//		EditorGUILayout.LabelField("Input Delete Index", GUILayout.Width(200));
//		strDeleteIndex = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strDeleteIndex);
//		int nDeleteIndex = -1;
//		if (!System.Int32.TryParse(strDeleteIndex, out nDeleteIndex))
//		{
//			nDeleteIndex = -1;
//		}
//		EditorGUILayout.EndVertical();

//		if (GUILayout.Button("DelConfig"))
//		{
//			if (nDeleteIndex >= 0 && nDeleteIndex <= commonWndModel.lstModel.Count)
//			{
//				commonWndModel.lstModel.RemoveAt(nDeleteIndex);
//			}
//		}

//		DrawDefaultInspector();
//	}

//	private void CreateBox(ref XClient.Common.EnFunctionBtn functionBtn, string fucntionName)
//	{
//		EditorGUILayout.BeginVertical("Box");

//		EditorGUILayout.BeginHorizontal("Box");

//		EditorGUILayout.LabelField("Input Enum(" + fucntionName + ")", GUILayout.Width(200));

//		string strOldParent = "";
//		dicOldParent.TryGetValue(fucntionName, out strOldParent);

//		string strParentStr = "";
//		if (!dicParentStr.TryGetValue(fucntionName, out strParentStr))
//		{
//			strParentStr = "";
//		}
//		strParentStr = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strParentStr);

//		EditorGUILayout.EndHorizontal();

//		XClient.Common.EnFunctionBtn tmp = XClient.Common.EnFunctionBtn.FunctionBtn_None;

//		if (!strParentStr.Equals(strOldParent))
//		{
//			try
//			{
//				tmp = (XClient.Common.EnFunctionBtn)System.Enum.Parse(typeof(XClient.Common.EnFunctionBtn), strParentStr);
//				functionBtn = tmp;
//				dicParentStr[fucntionName] = strParentStr;
//			}
//			catch (System.Exception e)
//			{ Debug.Log(e); }

//			dicOldParent[fucntionName] = strParentStr;
//		}
//		functionBtn = (XClient.Common.EnFunctionBtn)EditorGUILayout.EnumPopup(fucntionName, functionBtn);
//		int nFunctionBtn = (int)functionBtn;
//		nFunctionBtn = EditorGUILayout.IntField("ToInt", nFunctionBtn);
//		EditorGUILayout.EndVertical();
//	}

//	//通用路径配置Item绘制
//	private void DrawNodeConfigView(CommonWndModel commonWndModel)
//	{
//		if (commonWndModel.lstModel == null)
//		{
//			return;
//		}
//		EditorGUILayout.BeginVertical("Box");
//		int nCount = commonWndModel.lstModel.Count;
//		nCount = EditorGUILayout.IntField("Model List Count", nCount);

//		for (int i = 0; i < commonWndModel.lstModel.Count; i++)
//		{
//			CommonWndModel.SModel sModel = commonWndModel.lstModel[i];

//			EditorGUILayout.BeginVertical("Box");

//			EditorGUILayout.LabelField("Model " + i + "=========================================");

//			EditorGUILayout.BeginVertical("Box");
//			XClient.Common.EnFunctionBtn eBtn = (XClient.Common.EnFunctionBtn)sModel.eBtn;
//			eBtn = (XClient.Common.EnFunctionBtn)EditorGUILayout.EnumPopup ("E Btn", eBtn);
//			int nBtn = (int)eBtn;
//			nBtn = EditorGUILayout.IntField("ToInt", nBtn);
//			EditorGUILayout.EndVertical();

//			EditorGUILayout.BeginVertical("Box");
//			XClient.Common.WindowModel eModel = (XClient.Common.WindowModel)sModel.eModel;
//			eModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("E Model", eModel);
//			int nModel = (int)eModel;
//			nModel = EditorGUILayout.IntField("ToInt", nModel);

//			EditorGUILayout.EndVertical();

//			if (sModel.eSubModel.Count > 0)
//			{
//				EditorGUILayout.BeginVertical("Box");
//				int nSubCount = sModel.eSubModel.Count;
//				nSubCount = EditorGUILayout.IntField("Sub Model List Count", nSubCount);

//				EditorGUILayout.BeginVertical("Box");
//				EditorGUILayout.LabelField("Sub Model " + i + "-----------------------------------");
//				for (int j = 0; j < nSubCount; j ++)
//				{
//					EditorGUILayout.BeginVertical("Box");
//					XClient.Common.WindowModel eSubModel = (XClient.Common.WindowModel)sModel.eSubModel[j];
//					eSubModel = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("E Sub Model", eSubModel);
//					int nSubModel = (int)eSubModel;
//					nSubModel = EditorGUILayout.IntField("ToInt", nSubModel);
//					EditorGUILayout.EndVertical();
//				}
//				EditorGUILayout.EndVertical();

//				EditorGUILayout.EndVertical();
//			}

//			EditorGUILayout.EndVertical();

//		}
//		EditorGUILayout.EndVertical();
//	}
//}

//public class CommonWndModelConfig : EditorWindow
//{
//	Dictionary<string, string> dicOldParent = new Dictionary<string, string>();
//	Dictionary<string, string> dicParentStr = new Dictionary<string, string>();

//	public XClient.Common.EnFunctionBtn eBtn;       // 按钮
//	public XClient.Common.WindowModel eModel;       // 主窗口ID

//	List<XClient.Common.WindowModel> eSubModel = new List<XClient.Common.WindowModel>();    // 附加窗口

//	CommonWndModel m_commonWndModel;
//	public int m_nIndex = -1;
//	bool m_bUpdate;
//	int m_subNode;
//	int m_oldNode;

//	string strDeleteIndex = "";

//	public void InitConfig(CommonWndModel commonWndModel, bool bUpdate = false)
//	{
//		m_commonWndModel = commonWndModel;
//		m_bUpdate = bUpdate;
//	}

//	public void OnGUI()
//	{
//		if (m_bUpdate)
//		{
//			CreateInputBox();
//		}

//		CreateBtnBox(ref eBtn, "E Btn");

//		CreateModeleBox(ref eModel, "E Model");

//		CreateSubNode();

//		if (!m_bUpdate)
//		{
//			if (GUILayout.Button("AddNode"))
//			{
//				CommonWndModel.SModel sModel = new CommonWndModel.SModel();
//				sModel.eBtn = (int)eBtn;
//				sModel.eModel = (int)eModel;
//				sModel.eSubModel = new List<int>();
//				for (int i = 0; i < eSubModel.Count; i++)
//				{
//					sModel.eSubModel.Add((int)eSubModel[i]);
//				}
//				m_commonWndModel.lstModel.Add(sModel);
//				//this.Close();
//			}
//		}
//		else
//		{
//			if (GUILayout.Button("UpdateNode"))
//			{
//				if (m_nIndex < 0 || m_nIndex >= m_commonWndModel.lstModel.Count)
//				{
//					Debug.LogError("UpdateNode Error m_nIndex=" + m_nIndex + " nCount=" + m_commonWndModel.lstModel.Count);
//					return;
//				}
//				CommonWndModel.SModel sModel = m_commonWndModel.lstModel[m_nIndex];
//				sModel.eBtn = (int)eBtn;
//				sModel.eModel = (int)eModel;
//				sModel.eSubModel.Clear();
//				for (int i = 0; i < eSubModel.Count; i++)
//				{
//					sModel.eSubModel.Add((int)eSubModel[i]);
//				}
//				this.Close();
//			}
//		}

//		EditorGUILayout.BeginVertical("Box");
//		EditorGUILayout.LabelField("Input Delete Sub Index", GUILayout.Width(200));
//		strDeleteIndex = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strDeleteIndex);
//		int nDeleteIndex = -1;
//		if(!System.Int32.TryParse(strDeleteIndex, out nDeleteIndex))
//		{
//			nDeleteIndex = -1;
//		}
//		EditorGUILayout.EndVertical();

//		if (GUILayout.Button("DelConfig"))
//		{
//			CommonWndModel.SModel sModel = m_commonWndModel.lstModel[m_nIndex];
//			if (nDeleteIndex >= 0 && nDeleteIndex <= sModel.eSubModel.Count)
//			{
//				sModel.eSubModel.RemoveAt(nDeleteIndex);
//				UpdateSubNode(m_subNode - 1);
//			}
//		}
//	}

//	private void UpdateSubNode(int nCount)
//	{
//		m_subNode = nCount;
//		if (m_oldNode != m_subNode)
//		{
//			m_oldNode = m_subNode;

//			#region 移除超出的
//			if (eSubModel.Count > m_subNode)
//			{
//				eSubModel.RemoveRange(m_subNode, eSubModel.Count - m_subNode);
//			}
//			#endregion
//		}

//		DrawNodeConfigView(m_subNode);
//	}

//	private void CreateSubNode()
//	{
//		EditorGUILayout.BeginHorizontal("Box");

//		EditorGUILayout.LabelField("Set SubNode Count", GUILayout.Width(150));
//		m_subNode = EditorGUILayout.IntField(m_subNode);

//		EditorGUILayout.EndHorizontal();

//		if (m_oldNode != m_subNode)
//		{
//			m_oldNode = m_subNode;

//			#region 移除超出的
//			if (eSubModel.Count > m_subNode)
//			{
//				eSubModel.RemoveRange(m_subNode, eSubModel.Count - m_subNode);
//			}
//			#endregion
//		}

//		DrawNodeConfigView(m_subNode);
//	}

//	private List<string> curModelstr = new List<string>();
//	private List<string> oldModelStr = new List<string>();
	
//	private void DrawNodeConfigView(int count)
//	{
//		for (int i = 0; i < count; i++)
//		{
//			if (eSubModel.Count <= i)
//			{
//				eSubModel.Add(XClient.Common.WindowModel.None);
//				curModelstr.Add("");
//				oldModelStr.Add("");
//			}

//			EditorGUILayout.BeginVertical("Box");

//			EditorGUILayout.LabelField("Sub Model " + i + "-----------------------------------");

//			EditorGUILayout.BeginHorizontal("Box");
//			EditorGUILayout.LabelField("Input Sub eSubModel", GUILayout.Width(150));

//			curModelstr[i] = EditorGUI.TextField(EditorGUILayout.GetControlRect(), curModelstr[i]);
//			EditorGUILayout.EndHorizontal();

//			XClient.Common.WindowModel tmp = XClient.Common.WindowModel.None;
//			if (!curModelstr[i].Equals(oldModelStr[i]))
//			{
//				try
//				{
//					tmp = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), curModelstr[i]);
//					eSubModel[i] = tmp;
//				}
//				catch (System.Exception e)
//				{ Debug.Log(e); }

//				oldModelStr[i] = curModelstr[i];
//			}
//			eSubModel[i] = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup("SubModel", eSubModel[i]);

//			EditorGUILayout.EndVertical();
//		}
//	}

//	private void UpdateSubModel(int nIndex)
//	{
//		if (m_commonWndModel == null || nIndex < 0 || nIndex >= m_commonWndModel.lstModel.Count)
//		{
//			return;
//		}
//		m_nIndex = nIndex;
//		CommonWndModel.SModel sModel = m_commonWndModel.lstModel[m_nIndex];
//		eBtn = (XClient.Common.EnFunctionBtn)sModel.eBtn;
//		eModel = (XClient.Common.WindowModel)sModel.eModel;

//		eSubModel.Clear();
//		curModelstr.Clear();
//		oldModelStr.Clear();
//		for (int i = 0; i < sModel.eSubModel.Count; i++)
//		{
//			eSubModel.Add((XClient.Common.WindowModel)sModel.eSubModel[i]);
//			curModelstr.Add(eSubModel[i].ToString());
//			oldModelStr.Add(eSubModel[i].ToString());
//		}

//		UpdateSubNode(eSubModel.Count);
//	}

//	private void CreateInputBox()
//	{
//		EditorGUILayout.BeginVertical("Box");

//		EditorGUILayout.BeginHorizontal("Box");

//		EditorGUILayout.LabelField("Input N Index", GUILayout.Width(150));

//		string strOldIndex = m_nIndex.ToString();

//		string strIndex = "";

//		strIndex = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strIndex);

//		EditorGUILayout.EndHorizontal();

//		if (!strIndex.Equals(strOldIndex))
//		{
//			try
//			{
//				int tmp = System.Int32.Parse(strIndex);
//				UpdateSubModel(tmp);
//			}
//			catch (System.Exception e)
//			{ Debug.Log(e); }
			
//		}
//		m_nIndex = EditorGUILayout.IntField("N Index", m_nIndex);
//		EditorGUILayout.EndVertical();
//	}

//	private void CreateBtnBox(ref XClient.Common.EnFunctionBtn functionBtn, string fucntionName)
//	{
//		EditorGUILayout.BeginVertical("Box");

//		EditorGUILayout.BeginHorizontal("Box");

//		EditorGUILayout.LabelField("Input Enum(" + fucntionName + ")", GUILayout.Width(150));

//		string strOldParent = "";
//		dicOldParent.TryGetValue(fucntionName, out strOldParent);

//		string strParentStr = "";
//		if (!dicParentStr.TryGetValue(fucntionName, out strParentStr))
//		{
//			strParentStr = "";
//		}
//		strParentStr = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strParentStr);

//		EditorGUILayout.EndHorizontal();

//		XClient.Common.EnFunctionBtn tmp = XClient.Common.EnFunctionBtn.FunctionBtn_None;

//		if (!strParentStr.Equals(strOldParent))
//		{
//			try
//			{
//				tmp = (XClient.Common.EnFunctionBtn)System.Enum.Parse(typeof(XClient.Common.EnFunctionBtn), strParentStr);
//				functionBtn = tmp;
//				dicParentStr[fucntionName] = strParentStr;
//			}
//			catch (System.Exception e)
//			{ Debug.Log(e); }
//			dicOldParent[fucntionName] = strParentStr;
//		}
//		functionBtn = (XClient.Common.EnFunctionBtn)EditorGUILayout.EnumPopup(fucntionName, functionBtn);
//		int nFunctionBtn = (int)functionBtn;
//		nFunctionBtn = EditorGUILayout.IntField(fucntionName + " ToInt", nFunctionBtn);
//		EditorGUILayout.EndVertical();
//	}

//	private void CreateModeleBox(ref XClient.Common.WindowModel functionBtn, string fucntionName)
//	{
//		EditorGUILayout.BeginVertical("Box");

//		EditorGUILayout.BeginHorizontal("Box");

//		EditorGUILayout.LabelField("Input Enum(" + fucntionName + ")", GUILayout.Width(150));

//		string strOldParent = "";
//		dicOldParent.TryGetValue(fucntionName, out strOldParent);

//		string strParentStr = "";
//		if (!dicParentStr.TryGetValue(fucntionName, out strParentStr))
//		{
//			strParentStr = "";
//		}
//		strParentStr = EditorGUI.TextField(EditorGUILayout.GetControlRect(), strParentStr);

//		EditorGUILayout.EndHorizontal();

//		XClient.Common.WindowModel tmp = XClient.Common.WindowModel.None;

//		if (!strParentStr.Equals(strOldParent))
//		{
//			try
//			{
//				tmp = (XClient.Common.WindowModel)System.Enum.Parse(typeof(XClient.Common.WindowModel), strParentStr);
//				functionBtn = tmp;
//				dicParentStr[fucntionName] = strParentStr;
//			}
//			catch (System.Exception e)
//			{ Debug.Log(e); }

//			dicOldParent[fucntionName] = strParentStr;
//		}
//		functionBtn = (XClient.Common.WindowModel)EditorGUILayout.EnumPopup(fucntionName, functionBtn);
//		int nFunctionBtn = (int)functionBtn;
//		nFunctionBtn = EditorGUILayout.IntField(fucntionName + " ToInt", nFunctionBtn);

//		EditorGUILayout.EndVertical();
//	}
//}

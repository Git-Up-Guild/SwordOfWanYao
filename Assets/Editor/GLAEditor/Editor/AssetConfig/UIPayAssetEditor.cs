//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using XClient.UI;
//using XGame.Asset.Loader;
//using XGame.Utils;

//[CustomEditor(typeof(UIPayAsset))]
//public class UIPayAssetEditor : Editor
//{
//	public string m_strFolderPanel = "";
//	public override void OnInspectorGUI()
//	{
//		DrawDefaultInspector();

//		EditorGUILayout.BeginVertical("Box");
//		m_strFolderPanel = PlayerPrefs.GetString("UIPayAssetEditor_Path");
//		m_strFolderPanel = EditorGUILayout.TextField("FolderPanel", m_strFolderPanel);
//		EditorGUILayout.EndVertical();

//		if (GUILayout.Button("SetFolderPanel"))
//		{
//			string strFile = UnityEditor.EditorUtility.OpenFolderPanel("选择配置目录", m_strFolderPanel, "");
//			if (string.IsNullOrEmpty(strFile))
//			{
//				return;
//			}
//			m_strFolderPanel = strFile;
//			PlayerPrefs.SetString("UIPayAssetEditor_Path", m_strFolderPanel);
//		}

//		UIPayAsset asset = target as UIPayAsset;
//		if (GUILayout.Button("ExportPayFirstConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			UIPayResFirst[] arr = asset.uiPayRes.PayFirst;
//			string strFile = m_strFolderPanel + "\\PayFirstConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("nIndex,resTitle,resTotal,resNum,resAdL,resAdR,effectTitle,nPowerL,nPowerR,strSkinInfoL[nVocation*2+nSex:nSkinID;...],strSkinInfoR[[nVocation*2+nSex:nSkinID;...]\r\nint,string,string,string,string,string,int,int,int,string,string\r\n");
//			for (int i = 0; i < arr.Length; i ++)
//			{
//				UIPayResFirst res = arr[i];
//				sr.Write(res.nIndex);
//				sr.Write(",");
//				if (res.resTitle != null && !string.IsNullOrEmpty(res.resTitle.path))
//				{
//					sr.Write(res.resTitle.path);
//				}
//				sr.Write(",");
//				if (res.resTotal != null && !string.IsNullOrEmpty(res.resTotal.path))
//				{
//					sr.Write(res.resTotal.path);
//				}
//				sr.Write(",");
//				if (res.resNum != null && !string.IsNullOrEmpty(res.resNum.path))
//				{
//					sr.Write(res.resNum.path);
//				}
//				sr.Write(",");
//				if (res.resAdL != null && !string.IsNullOrEmpty(res.resAdL.path))
//				{
//					sr.Write(res.resAdL.path);
//				}
//				sr.Write(",");
//				if (res.resAdR != null && !string.IsNullOrEmpty(res.resAdR.path))
//				{
//					sr.Write(res.resAdR.path);
//				}
//				sr.Write(",");
//				sr.Write(res.effectTitle);
//				sr.Write(",");
//				sr.Write(res.nPowerL);
//				sr.Write(",");
//				sr.Write(res.nPowerR);
//				sr.Write(",");
//				string strSkinInfo = "";
//				for (int j = 0; j < res.sSkinL.Length; j ++)
//				{
//					if (strSkinInfo != "")
//					{
//						strSkinInfo += ";";
//					}
//					UIPayResFirst.SkinInfo sInfo = res.sSkinL[j];
//					strSkinInfo += sInfo.nVocation.ToString();
//					strSkinInfo += ":";
//					strSkinInfo += sInfo.nSkinID.ToString();

//				}
//				sr.Write(strSkinInfo);
//				sr.Write(",");
//				strSkinInfo = "";
//				for (int j = 0; j < res.sSkinR.Length; j++)
//				{
//					if (strSkinInfo != "")
//					{
//						strSkinInfo += ";";
//					}
//					UIPayResFirst.SkinInfo sInfo = res.sSkinR[j];
//					strSkinInfo += sInfo.nVocation.ToString();
//					strSkinInfo += ":";
//					strSkinInfo += sInfo.nSkinID.ToString();
//				}
//				sr.Write(strSkinInfo);
//				sr.Write("\r\n");
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportPayFirstConf true");
//		}

//		if (GUILayout.Button("ImportPayFirstConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\PayFirstConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}
//			UIPayResFirst[] arr = new UIPayResFirst[nMaxRow - 1];
//			List<string> propList = ListPool<string>.Get();
//			List<string> szStrList = ListPool<string>.Get();
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nCol = 0;
//				UIPayResFirst res = new UIPayResFirst();
//				res.nIndex = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.resTitle = new ResourceRef();
//				res.resTitle.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resTitle.UpdateResourceGuid(true);
//				res.resTotal = new ResourceRef();
//				res.resTotal.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resTotal.UpdateResourceGuid(true);
//				res.resNum = new ResourceRef();
//				res.resNum.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resNum.UpdateResourceGuid(true);
//				res.resAdL = new ResourceRef();
//				res.resAdL.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAdL.UpdateResourceGuid(true);
//				res.resAdR = new ResourceRef();
//				res.resAdR.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAdR.UpdateResourceGuid(true);
//				res.effectTitle = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.nPowerL = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.nPowerR = pCSVReader.GetInt(nRow, nCol++, 0);
//				string strSkinInfo = pCSVReader.GetString(nRow, nCol++, "");
//				if (!string.IsNullOrEmpty(strSkinInfo))
//				{
//					szStrList.Clear();
//				    Api.split(ref szStrList, strSkinInfo, ";");
//					res.sSkinL = new UIPayResFirst.SkinInfo[szStrList.Count];
//					for (int i = 0; i < szStrList.Count; i++)
//					{
//						propList.Clear();
//						XGame.Utils.Api.split(ref propList, szStrList[i], ":");
//						if (propList.Count != 2)
//						{
//							Debug.LogError("ImportPayFirstConf sSkinL propList.Count != 2 nIndex=" + res.nIndex + " nNum=" + (i + 1).ToString());
//							return;
//						}
//						res.sSkinL[i] = new UIPayResFirst.SkinInfo();
//						res.sSkinL[i].nVocation = int.Parse(propList[0].Trim());
//						res.sSkinL[i].nSkinID = int.Parse(propList[1].Trim());
//					}
//				}

//				strSkinInfo = pCSVReader.GetString(nRow, nCol++, "");
//				if (!string.IsNullOrEmpty(strSkinInfo))
//				{
//					szStrList.Clear();
//					XGame.Utils.Api.split(ref szStrList, strSkinInfo, ";");
//					res.sSkinR = new UIPayResFirst.SkinInfo[szStrList.Count];
//					for (int i = 0; i < szStrList.Count; i++)
//					{
//						propList.Clear();
//						XGame.Utils.Api.split(ref propList, szStrList[i], ":");
//						if (propList.Count != 2)
//						{
//							Debug.LogError("ImportPayFirstConf sSkinR propList.Count(" + propList.Count + ") != 2 nIndex=" + res.nIndex + " nNum=" + (i + 1).ToString() + " info=" + szStrList[i]);
//							return;
//						}
//						res.sSkinR[i] = new UIPayResFirst.SkinInfo();
//						res.sSkinR[i].nVocation = int.Parse(propList[0].Trim());
//						res.sSkinR[i].nSkinID = int.Parse(propList[1].Trim());
//					}
//				}
//				arr[nRow - 1] = res;
//			}
//			ListPool<string>.Release(szStrList);
//			ListPool<string>.Release(propList);
//			asset.uiPayRes.PayFirst = arr;
//			for (int i = 0; i < asset.uiPayRes.PayFirst.Length; i++)
//			{
//				UIPayResFirst res = asset.uiPayRes.PayFirst[i];
//				if (res.resTitle != null && !string.IsNullOrEmpty(res.resTitle.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resTitle.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayFirstConf resTitle sp=null idx=" + i + " sp=" + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayFirstConf resTitle idx=" + i + " path=null");
//				}
//				if (res.resTotal != null && !string.IsNullOrEmpty(res.resTotal.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resTotal.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayFirstConf resTotal sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayFirstConf resTotal idx=" + i + " path=null");
//				}
//				if (res.resNum != null && !string.IsNullOrEmpty(res.resNum.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resNum.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayFirstConf resNum sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayFirstConf resNum idx=" + i + " path=null");
//				}
//				if (res.resAdL != null && !string.IsNullOrEmpty(res.resAdL.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAdL.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayFirstConf resAdL sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayFirstConf resAdL idx=" + i + " path=null");
//				}
//				if (res.resAdR != null && !string.IsNullOrEmpty(res.resAdR.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAdR.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayFirstConf resAdR sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayFirstConf resAdR idx=" + i + " path=null");
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportPayFirstConf Succ");
//		}

//		if (GUILayout.Button("ExportPayDailyConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			UIPayResDaily[] arr = asset.uiPayRes.DailyRes;
//			string strFile = m_strFolderPanel + "\\PayDailyConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("nIndex,resAd1,resAd11,resAd12,nSkinID,bHideCrit\r\nint,string,string,string,int,string\r\n");
//			for (int i = 0; i < arr.Length; i++)
//			{
//				UIPayResDaily res = arr[i];
//				sr.Write(res.nIndex);
//				sr.Write(",");
//				if (res.resAd1 != null && !string.IsNullOrEmpty(res.resAd1.path))
//				{
//					sr.Write(res.resAd1.path);
//				}
//				sr.Write(",");
//				if (res.resAd11 != null && !string.IsNullOrEmpty(res.resAd11.path))
//				{
//					sr.Write(res.resAd11.path);
//				}
//				sr.Write(",");
//				if (res.resAd12 != null && !string.IsNullOrEmpty(res.resAd12.path))
//				{
//					sr.Write(res.resAd12.path);
//				}
//				sr.Write(",");
//				sr.Write(res.nSkinID);
//				sr.Write(",");
//				sr.Write(res.bHideCrit);
//				sr.Write("\r\n");
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportPayDailyConf true");
//		}

//		if (GUILayout.Button("ImportPayDailyConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\PayDailyConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}
//			UIPayResDaily[] arr = new UIPayResDaily[nMaxRow - 1];
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nCol = 0;
//				UIPayResDaily res = new UIPayResDaily();
//				res.nIndex = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.resAd1 = new ResourceRef();
//				res.resAd1.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAd1.UpdateResourceGuid(true);
//				res.resAd11 = new ResourceRef();
//				res.resAd11.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAd11.UpdateResourceGuid(true);
//				res.resAd12 = new ResourceRef();
//				res.resAd12.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAd12.UpdateResourceGuid(true);
//				res.nSkinID = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.bHideCrit = System.Boolean.Parse(pCSVReader.GetString(nRow, nCol++, ""));
//				arr[nRow - 1] = res;
//			}
//			asset.uiPayRes.DailyRes = arr;
//			for (int i = 0; i < asset.uiPayRes.DailyRes.Length; i++)
//			{
//				UIPayResDaily res = asset.uiPayRes.DailyRes[i];
//				if (res.resAd1 != null && !string.IsNullOrEmpty(res.resAd1.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAd1.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayDailyConf resAd1 sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayDailyConf resAd1 idx=" + i + " path=null");
//				}
//				if (res.resAd11 != null && !string.IsNullOrEmpty(res.resAd11.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAd11.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayDailyConf resAd11 sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayDailyConf resAd11 idx=" + i + " path=null");
//				}
//				if (res.resAd12 != null && !string.IsNullOrEmpty(res.resAd12.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAd12.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportPayDailyConf resAd12 sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayDailyConf resAd12 idx=" + i + " path=null");
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportPayDailyConf Succ");
//		}
		
//		if (GUILayout.Button("ExportVipTypeConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			UIVipTypeRes[] arr = asset.uiPayRes.VipType;
//			string strFile = m_strFolderPanel + "\\VipTypeConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("nVipType,strName,strValid,strActive,resBg,resName,arrSkinID[nVocation*2+nSex:nSkinID;...],lstGift[resGiftBg|resImage|strName|nVocation-nGoodsID&nVocation-nGoodsID;...],nEffectID,nSkinkEffect,resRight,resRightPress,resIconBig,resIconSmall\r\n" +
//				"string,string,string,string,string,string,int,string,int,int,string,string,string,string\r\n");
//			for (int i = 0; i < arr.Length; i++)
//			{
//				UIVipTypeRes res = arr[i];
//				sr.Write(res.nVipType.ToString());
//				sr.Write(",");
//				sr.Write(res.strName);
//				sr.Write(",");
//				sr.Write(res.strValid);
//				sr.Write(",");
//				sr.Write(res.strActive);
//				sr.Write(",");
//				if (res.resBg != null && !string.IsNullOrEmpty(res.resBg.path))
//				{
//					sr.Write(res.resBg.path);
//				}
//				sr.Write(",");
//				if (res.resName != null && !string.IsNullOrEmpty(res.resName.path))
//				{
//					sr.Write(res.resName.path);
//				}
//				sr.Write(",");
//				string strSkinInfo = "";
//				for (int j = 0; j < res.arrSkinID.Length; j ++)
//				{
//					if (strSkinInfo != "")
//					{
//						strSkinInfo += ";";
//					}
//					strSkinInfo += res.arrSkinID[j].nVocation;
//					strSkinInfo += ":";
//					strSkinInfo += res.arrSkinID[j].nSkinID;
//				}

//				sr.Write(strSkinInfo);
//				sr.Write(",");
//				string strGift = "";
//				for (int j = 0; j < res.lstGift.Count; j ++)
//				{
//					if (strGift != "")
//					{
//						strGift += ";";
//					}
//					if (res.lstGift[j].resGiftBg != null && !string.IsNullOrEmpty(res.lstGift[j].resGiftBg.path))
//					{
//						strGift += res.lstGift[j].resGiftBg.path;
//					}
//					strGift += "|";
//					if (res.lstGift[j].resImage != null && !string.IsNullOrEmpty(res.lstGift[j].resImage.path))
//					{
//						strGift += res.lstGift[j].resImage.path;
//					}
//					strGift += "|";
//					strGift += res.lstGift[j].strName;
//					strGift += "|";
//					string strTips = "";
//					for (int k = 0; k < res.lstGift[j].arrTips.Length; k ++)
//					{
//						UIVipTypeRes.GoodsTips sTips = res.lstGift[j].arrTips[k];
//						if (sTips == null)
//						{
//							continue;
//						}
//						if (strTips != "")
//						{
//							strTips += "&";
//						}
//						strTips += sTips.nVocation.ToString();
//						strTips += "-";
//						strTips += sTips.nGoodsID.ToString();
//					}
//					strGift += strTips;
//				}
//				sr.Write(strGift);
//				sr.Write(",");
//				sr.Write(res.nEffectID);
//				sr.Write(",");
//				sr.Write(res.nSkinkEffect);
//				sr.Write(",");
//				if (res.resRight != null && !string.IsNullOrEmpty(res.resRight.path))
//				{
//					sr.Write(res.resRight.path);
//				}
//				sr.Write(",");
//				if (res.resRightPress != null && !string.IsNullOrEmpty(res.resRightPress.path))
//				{
//					sr.Write(res.resRightPress.path);
//				}
//				sr.Write(",");
//				if (res.resIconBig != null && !string.IsNullOrEmpty(res.resIconBig.path))
//				{
//					sr.Write(res.resIconBig.path);
//				}
//				sr.Write(",");
//				if (res.resIconSmall != null && !string.IsNullOrEmpty(res.resIconSmall.path))
//				{
//					sr.Write(res.resIconSmall.path);
//				}
//				sr.Write("\r\n");
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportVipTypeConf true");
//		}

//		if (GUILayout.Button("ImportVipTypeConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\VipTypeConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}
//			UIVipTypeRes[] arr = new UIVipTypeRes[nMaxRow - 1];
//			List<string> propList = ListPool<string>.Get();
//			List<string> szStrList = ListPool<string>.Get();
//			List<List<string>> tipsList = ListPool<List<string>>.Get();
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nCol = 0;
//				UIVipTypeRes res = new UIVipTypeRes();
//				res.nVipType = (XClient.Common.EMVipType)System.Enum.Parse(typeof(XClient.Common.EMVipType), pCSVReader.GetString(nRow, nCol++, ""));
//				res.strName = pCSVReader.GetString(nRow, nCol++, "");
//				res.strValid = pCSVReader.GetString(nRow, nCol++, "");
//				res.strActive = pCSVReader.GetString(nRow, nCol++, "");
//				res.resBg = new ResourceRef();
//				res.resBg.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resBg.UpdateResourceGuid(true);
//				res.resName = new ResourceRef();
//				res.resName.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resName.UpdateResourceGuid(true);
//				string strSkinInfo = pCSVReader.GetString(nRow, nCol++, "");
//				if (!string.IsNullOrEmpty(strSkinInfo))
//				{
//					szStrList.Clear();
//					XGame.Utils.Api.split(ref szStrList, strSkinInfo, ";");
//					res.arrSkinID = new UIVipTypeRes.SkinInfo[szStrList.Count];
//					for (int k = 0; k < szStrList.Count; k++)
//					{
//						propList.Clear();
//						XGame.Utils.Api.split(ref propList, szStrList[k], ":");
//						if (propList.Count < 2)
//						{
//							Debug.LogError("ImportVipTypeConf arrSkinID propList.Count(" + propList.Count + ") < 2 nVipType=" + res.nVipType.ToString() + " nNum=" + (k + 1).ToString() + " info=" + szStrList[k]);
//							return;
//						}
//						UIVipTypeRes.SkinInfo resSkin = res.arrSkinID[k];
//						resSkin.nVocation = System.Int32.Parse(propList[0]);
//						resSkin.nSkinID = System.Int32.Parse(propList[1]);
//					}
//				}
//				string strGift = pCSVReader.GetString(nRow, nCol++, "");
//				if (!string.IsNullOrEmpty(strGift))
//				{
//					szStrList.Clear();
//					XGame.Utils.Api.split(ref szStrList, strGift, ";");
//					res.lstGift = new List<UIVipTypeRes.GiftRes>();
//					for (int i = 0; i < szStrList.Count; i++)
//					{
//						propList.Clear();
//						XGame.Utils.Api.split(ref propList, szStrList[i], "|");
//						if (propList.Count < 2)
//						{
//							Debug.LogError("ImportVipTypeConf lstGift propList.Count(" + propList.Count + ") < 2 nVipType=" + res.nVipType.ToString() + " nNum=" + (i + 1).ToString() + " info=" + szStrList[i]);
//							return;
//						}
//						UIVipTypeRes.GiftRes resGift = new UIVipTypeRes.GiftRes();
//						resGift.resGiftBg = new ResourceRef();
//						resGift.resGiftBg.path = propList[0];
//						resGift.resGiftBg.UpdateResourceGuid(true);
//						resGift.resImage = new ResourceRef();
//						resGift.resImage.path = propList[1];
//						resGift.resImage.UpdateResourceGuid(true);
//						if (propList.Count >= 3)
//						{
//							resGift.strName = propList[2];
//						}
//						if (propList.Count >= 4)
//						{
//							tipsList.Clear();
//							string strTips = propList[3];
//							XGame.Utils.Api.StringToStrList(strTips, '&', '-', ref tipsList);
//							resGift.arrTips = new UIVipTypeRes.GoodsTips[tipsList.Count];
//							for (int m = 0; m < tipsList.Count; m++)
//							{
//								resGift.arrTips[m] = new UIVipTypeRes.GoodsTips();
//								if (tipsList[m].Count < 2)
//								{
//									continue;
//								}
//								resGift.arrTips[m].nVocation = (XClient.Common.EMPERSON_VOCATION)System.Enum.Parse(typeof(XClient.Common.EMPERSON_VOCATION), tipsList[m][0]);
//								resGift.arrTips[m].nGoodsID = System.Int32.Parse(tipsList[m][1]);
//							}
//						}
//						res.lstGift.Add(resGift);
//					}
//				}

//				res.nEffectID = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.nSkinkEffect = pCSVReader.GetInt(nRow, nCol++, 0);

//				res.resRight = new ResourceRef();
//				res.resRight.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resRight.UpdateResourceGuid(true);
//				res.resRightPress = new ResourceRef();
//				res.resRightPress.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resRightPress.UpdateResourceGuid(true);
//				res.resIconBig = new ResourceRef();
//				res.resIconBig.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resIconBig.UpdateResourceGuid(true);
//				res.resIconSmall = new ResourceRef();
//				res.resIconSmall.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resIconSmall.UpdateResourceGuid(true);

//				arr[nRow - 1] = res;
//			}
//			ListPool<string>.Release(propList);
//			ListPool<string>.Release(szStrList);
//			ListPool<List<string>>.Release(tipsList);

//			asset.uiPayRes.VipType = arr;
//			for (int i = 0; i < asset.uiPayRes.VipType.Length; i++)
//			{
//				UIVipTypeRes res = asset.uiPayRes.VipType[i];
//				if (res.resBg != null && !string.IsNullOrEmpty(res.resBg.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resBg.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipTypeConf resBg sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportPayFirstConf resBg idx=" + i + " path=null");
//				}
//				if (res.resName != null && !string.IsNullOrEmpty(res.resName.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resName.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipTypeConf resName sp=null idx=" + i + " sp=" + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipTypeConf resName idx=" + i + " path=null");
//				}
//				if (res.resRight != null && !string.IsNullOrEmpty(res.resRight.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resRight.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipTypeConf resRight sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipTypeConf resRight idx=" + i + " path=null");
//				}
//				if (res.resRightPress != null && !string.IsNullOrEmpty(res.resRightPress.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resRightPress.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipTypeConf resRightPress sp=null idx=" + i + " sp=" + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipTypeConf resRightPress idx=" + i + " path=null");
//				}
//				if (res.resIconBig != null && !string.IsNullOrEmpty(res.resIconBig.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resIconBig.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipTypeConf resIconBig sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipTypeConf resIconBig idx=" + i + " path=null");
//				}
//				if (res.resIconSmall != null && !string.IsNullOrEmpty(res.resIconSmall.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resIconSmall.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipTypeConf resIconSmall sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipTypeConf resIconSmall idx=" + i + " path=null");
//				}

//				for (int j = 0; j < res.lstGift.Count; j++)
//				{
//					UIVipTypeRes.GiftRes resGift = res.lstGift[j];
//					if (resGift.resGiftBg != null && !string.IsNullOrEmpty(resGift.resGiftBg.path))
//					{
//						GResources.LoadAsync<Sprite>(resGift.resGiftBg.path, (path, sp) =>
//						{
//							if (sp == null)
//								Debug.LogError("ImportVipTypeConf resGiftBg sp=null idx=" + i + " gift=" + j  + " path=" + path);
//						});
//					}
//					else
//					{
//						Debug.LogError("ImportVipTypeConf resGiftBg idx=" + i + " gift=" + j + " path=null");
//					}
//					if (resGift.resImage != null && !string.IsNullOrEmpty(resGift.resImage.path))
//					{
//						GResources.LoadAsync<Sprite>(resGift.resImage.path, (path, sp) =>
//						{
//							if (sp == null)
//								Debug.LogError("ImportVipTypeConf resImage sp=null idx=" + i + " gift=" + j  + " path=" + path);
//						});
//					}
//					else
//					{
//						Debug.LogError("ImportVipTypeConf resImage idx=" + i + " gift=" + j + " path=null");
//					}
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportVipTypeConf Succ");
//		}

//		if (GUILayout.Button("ExportVipGiftConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			UIVipGiftRes[] arr = asset.uiPayRes.VipGift;
//			string strFile = m_strFolderPanel + "\\VipGiftConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("nVipLevel,nSkinID,resAd1,resAd2,resAd3\r\nint,int,string,string,string\r\n");
//			for (int i = 0; i < arr.Length; i++)
//			{
//				UIVipGiftRes res = arr[i];
//				sr.Write(res.nVipLevel);
//				sr.Write(",");
//				sr.Write(res.nSkinID);
//				sr.Write(",");
//				if (res.resAd1 != null && !string.IsNullOrEmpty(res.resAd1.path))
//				{
//					sr.Write(res.resAd1.path);
//				}
//				sr.Write(",");
//				if (res.resAd2 != null && !string.IsNullOrEmpty(res.resAd2.path))
//				{
//					sr.Write(res.resAd2.path);
//				}
//				sr.Write(",");
//				if (res.resAd3 != null && !string.IsNullOrEmpty(res.resAd3.path))
//				{
//					sr.Write(res.resAd3.path);
//				}
//				sr.Write("\r\n");
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportVipGiftConf true");
//		}

//		if (GUILayout.Button("ImportVipGiftConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\VipGiftConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}
//			UIVipGiftRes[] arr = new UIVipGiftRes[nMaxRow - 1];
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nCol = 0;
//				UIVipGiftRes res = new UIVipGiftRes();
//				res.nVipLevel = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.nSkinID = pCSVReader.GetInt(nRow, nCol++, 0);
//				res.resAd1 = new ResourceRef();
//				res.resAd1.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAd1.UpdateResourceGuid(true);
//				res.resAd2 = new ResourceRef();
//				res.resAd2.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAd2.UpdateResourceGuid(true);
//				res.resAd3 = new ResourceRef();
//				res.resAd3.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resAd3.UpdateResourceGuid(true);
//				arr[nRow - 1] = res;
//			}
//			asset.uiPayRes.VipGift = arr;
//			for (int i = 0; i < asset.uiPayRes.VipGift.Length; i++)
//			{
//				UIVipGiftRes res = asset.uiPayRes.VipGift[i];
//				if (res.resAd1 != null && !string.IsNullOrEmpty(res.resAd1.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAd1.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipGiftConf resAd1 sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipGiftConf resAd1 idx=" + i + " path=null");
//				}
//				if (res.resAd2 != null && !string.IsNullOrEmpty(res.resAd2.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAd2.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipGiftConf resAd11 sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipGiftConf resAd11 idx=" + i + " path=null");
//				}
//				if (res.resAd3 != null && !string.IsNullOrEmpty(res.resAd3.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resAd3.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportVipGiftConf resAd12 sp=null idx=" + i + " sp=null" + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportVipGiftConf resAd12 idx=" + i + " path=null");
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportVipGiftConf Succ");
//		}

//		if (GUILayout.Button("ExportValueConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			UIValueRes[] arr = asset.uiPayRes.ValueRes;
//			string strFile = m_strFolderPanel + "\\ValueConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("eType,resIcon,vecSize,strName\r\nstring,string,string,string\r\n");
//			for (int i = 0; i < arr.Length; i++)
//			{
//				UIValueRes res = arr[i];
//				sr.Write(res.eType.ToString());
//				sr.Write(",");
//				if (res.resIcon != null && !string.IsNullOrEmpty(res.resIcon.path))
//				{
//					sr.Write(res.resIcon.path);
//				}
//				sr.Write(",");
//				sr.Write(res.vecSize.x + ";" + res.vecSize.y);
//				sr.Write(",");
//				sr.Write(res.strName);
//				sr.Write("\r\n");
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportValueConf true");
//		}

//		if (GUILayout.Button("ImportValueConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\ValueConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}
//			UIValueRes[] arr = new UIValueRes[nMaxRow - 1];

//			List<float> lstVecSize = ListPool<float>.Get();
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nCol = 0;
//				UIValueRes res = new UIValueRes();
//				res.eType = (EMUIValueType)System.Enum.Parse(typeof(EMUIValueType), pCSVReader.GetString(nRow, nCol++, ""));
//				res.resIcon = new ResourceRef();
//				res.resIcon.path = pCSVReader.GetString(nRow, nCol++, "");
//				res.resIcon.UpdateResourceGuid(true);
//				string strVecSize = pCSVReader.GetString(nRow, nCol++, "");
//				if (string.IsNullOrEmpty(strVecSize))
//				{
//					res.vecSize = Vector2.zero;
//				}
//				else
//				{
//					lstVecSize.Clear();
//					XGame.Utils.XGame.Utils.StringUtil.StringToFloatList(strVecSize, ';', ref lstVecSize);
//					if (lstVecSize.Count != 2)
//					{
//						Debug.LogError("ImportValueConf vecSize lstVecSize.Count(" + lstVecSize.Count + ") != 2 eType=" + res.eType.ToString() + " info=" + strVecSize);
//						return;
//					}
//					res.vecSize = new Vector2(lstVecSize[0], lstVecSize[1]);
//				}
//				res.strName = pCSVReader.GetString(nRow, nCol++, "");
//				arr[nRow - 1] = res;
//			}
//			ListPool<float>.Release(lstVecSize);
//			asset.uiPayRes.ValueRes = arr;
//			for (int i = 0; i < asset.uiPayRes.ValueRes.Length; i++)
//			{
//				UIValueRes res = asset.uiPayRes.ValueRes[i];
//				if (res.resIcon != null && !string.IsNullOrEmpty(res.resIcon.path))
//				{
//					GResources.LoadAsync<Sprite>(res.resIcon.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportValueConf resIcon sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportValueConf resIcon idx=" + i + " path=null");
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportValueConf Succ");
//		}
		
//		if (GUILayout.Button("ExportNumConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			SUINumRes[] arr = asset.uiPayRes.NumRes;
//			string strFile = m_strFolderPanel + "\\NumConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("eType,nLen,nNum,resNum\r\nstring,int,int,string\r\n");
//			for (int i = 0; i < arr.Length; i++)
//			{
//				SUINumRes res = arr[i];
//				for (int j = 0; j <res.resNum.Length; j ++)
//				{
//					sr.Write(res.eType.ToString());
//					sr.Write(",");
//					sr.Write(res.resNum.Length);
//					sr.Write(",");
//					sr.Write(j);
//					sr.Write(",");
//					if (res.resNum[j] != null && !string.IsNullOrEmpty(res.resNum[j].path))
//					{
//						sr.Write(res.resNum[j].path);
//					}
//					sr.Write("\r\n");
//				}
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportNumConf true");
//		}

//		if (GUILayout.Button("ImportNumConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\NumConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}
//			List<SUINumRes> lstNum = new List<SUINumRes>();
//			SUINumRes sCur = null;
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nCol = 0;
//				EMNumResType eType = (EMNumResType)System.Enum.Parse(typeof(EMNumResType), pCSVReader.GetString(nRow, nCol++, ""));
//				if (sCur == null || sCur.eType != eType)
//				{
//					sCur = new SUINumRes();
//					sCur.eType = eType;
//					lstNum.Add(sCur);

//					int nLen = pCSVReader.GetInt(nRow, nCol++, 0);
//					sCur.resNum = new ResourceRef[nLen];
//				}
//				else
//				{
//					nCol++;
//				}
				
//				int nNo = pCSVReader.GetInt(nRow, nCol++, 0);
//				sCur.resNum[nNo] = new ResourceRef();
//				sCur.resNum[nNo].path = pCSVReader.GetString(nRow, nCol++, "");
//				sCur.resNum[nNo].UpdateResourceGuid(true);
//			}

//			asset.uiPayRes.NumRes = lstNum.ToArray();
//			for (int i = 0; i < asset.uiPayRes.NumRes.Length; i++)
//			{
//				SUINumRes res = asset.uiPayRes.NumRes[i];
//				for (int j = 0; j < res.resNum.Length; j ++)
//				{
//					if (res.resNum[j] != null && !string.IsNullOrEmpty(res.resNum[j].path))
//					{
//						GResources.LoadAsync<Sprite>(res.resNum[j].path, (path, sp) =>
//						{
//							if (sp == null)
//								Debug.LogError("ImportNumConf res.resNum sp=null idx=" + i + " nNo=" + j + " path=" + path);
//						});
//					}
//					else
//					{
//						Debug.LogError("ImportNumConf res.resNum idx=" + i + " nNo=" + j + " path=null");
//					}
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportNumConf Succ");
//		}

//		if (GUILayout.Button("ExportRankConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			ResourceRef[] arr = asset.uiPayRes.RankRes;
//			string strFile = m_strFolderPanel + "\\RankConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("resRank\r\nstring\r\n");
//			for (int i = 0; i < arr.Length; i++)
//			{
//				ResourceRef res = arr[i];
//				if (res != null && !string.IsNullOrEmpty(res.path))
//				{
//					sr.Write(res.path);
//				}
//				sr.Write("\r\n");
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportRankConf true");
//		}

//		if (GUILayout.Button("ImportRankConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\RankConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}


//			ResourceRef[] arr = new ResourceRef[nMaxRow - 1];
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nNo = nRow - 1;
//				arr[nNo] = new ResourceRef();
//				arr[nNo].path = pCSVReader.GetString(nRow, 0, "");
//				arr[nNo].UpdateResourceGuid(true);
//			}

//			asset.uiPayRes.RankRes = arr;
//			for (int i = 0; i < asset.uiPayRes.RankRes.Length; i++)
//			{
//				ResourceRef res = asset.uiPayRes.RankRes[i];
//				if (res != null && !string.IsNullOrEmpty(res.path))
//				{
//					GResources.LoadAsync<Sprite>(res.path, (path, sp) =>
//					{
//						if (sp == null)
//							Debug.LogError("ImportRankConf res sp=null idx=" + i + " path=" + path);
//					});
//				}
//				else
//				{
//					Debug.LogError("ImportRankConf res idx=" + i + " path=null");
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportRankConf Succ");
//		}

//		if (GUILayout.Button("ExportGrowUpConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			SUIGrowUpRes[] arr = asset.uiPayRes.GrowUpRes;
//			string strFile = m_strFolderPanel + "\\GrowUpConf.csv";
//			bool bIsExist = System.IO.File.Exists(strFile);
//			System.IO.FileStream fs = new System.IO.FileStream(strFile, bIsExist ? System.IO.FileMode.Truncate : System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
//			System.IO.StreamWriter sr = new System.IO.StreamWriter(fs, new System.Text.UTF8Encoding(true));
//			sr.Write("eMode,nSlotCount,nSlotID,resIcon,arrEffect[nBeginLevel:nEndLevel:nEffectID;...],strDes,clDes\r\nstring,int,int,string,string,string,string\r\n");
//			for (int i = 0; i < arr.Length; i++)
//			{
//				SUIGrowUpRes growUp = arr[i];
//				for (int j = 0; j < growUp.SlotRes.Length; j ++)
//				{
//					SUIGrowUpSlotRes sSlotInfo = growUp.SlotRes[j];
//					sr.Write(growUp.eMode);
//					sr.Write(",");
//					sr.Write(growUp.SlotRes.Length);
//					sr.Write(",");
//					sr.Write(sSlotInfo.nSlotID);
//					sr.Write(",");
//					if (sSlotInfo.resIcon != null && !string.IsNullOrEmpty(sSlotInfo.resIcon.path))
//					{
//						sr.Write(sSlotInfo.resIcon.path);
//					}
//					sr.Write(",");
//					string strEffect = "";
//					for (int k = 0; k < sSlotInfo.arrEffect.Length; k ++)
//					{
//						SSlotEffectID sEffect = sSlotInfo.arrEffect[k];
//						if (strEffect != "")
//						{
//							strEffect += ";";
//						}
//						strEffect += sEffect.nBeginLevel;
//						strEffect += ":";
//						strEffect += sEffect.nEndLevel;
//						strEffect += ":";
//						strEffect += sEffect.nEffectID;
//					}
//					sr.Write(strEffect);
//					sr.Write(",");
//					sr.Write(sSlotInfo.strDes);
//					sr.Write(",");
//					int r = Mathf.RoundToInt(sSlotInfo.clDes.r * 255.0f);
//					int g = Mathf.RoundToInt(sSlotInfo.clDes.g * 255.0f);
//					int b = Mathf.RoundToInt(sSlotInfo.clDes.b * 255.0f);
//					int a = Mathf.RoundToInt(sSlotInfo.clDes.a * 255.0f);
//					string hex = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
//					sr.Write(hex);
//					sr.Write("\r\n");
//				}
//			}
//			sr.Close();
//			fs.Close();
//			Debug.LogError("ExportGrowUpConf true");
//		}

//		if (GUILayout.Button("ImportGrowUpConf"))
//		{
//			if (string.IsNullOrEmpty(m_strFolderPanel))
//			{
//				Debug.LogError("请先选择目录");
//				return;
//			}
//			string strFile = m_strFolderPanel + "\\GrowUpConf.csv";
//			XGame.CsvReader.ScpReader pCSVReader = new XGame.CsvReader.ScpReader(strFile, 1);
//			if (pCSVReader == null || !pCSVReader.Loaded())
//			{
//				Debug.LogError("加载配置表失败！ strFile=" + strFile);
//				return;
//			}

//			int nMaxRow = pCSVReader.GetRecordCount();
//			if (nMaxRow <= 1)
//			{
//				Debug.LogError("配置表没有配置项！ strFile=" + strFile);
//				return;
//			}

//			List<SUIGrowUpRes> lstGrowUp = new List<SUIGrowUpRes>();
//			SUIGrowUpRes sGrowUp = null;
//			List<string> propList = ListPool<string>.Get();
//			List<string> szStrList = ListPool<string>.Get();
//			// 读取
//			for (int nRow = 1; nRow < nMaxRow; nRow++)
//			{
//				int nCol = 0;
//				int nMode = pCSVReader.GetInt(nRow, nCol ++, 0);
//				if (sGrowUp == null || sGrowUp.eMode != nMode)
//				{
//					sGrowUp = new SUIGrowUpRes();
//					sGrowUp.eMode = nMode;
//					lstGrowUp.Add(sGrowUp);

//					int nLen = pCSVReader.GetInt(nRow, nCol++, 0);
//					sGrowUp.SlotRes = new SUIGrowUpSlotRes[nLen];
//				}
//				else
//				{
//					nCol++;
//				}
//				int nSlotID = pCSVReader.GetInt(nRow, nCol++, 0);
//				int nNo = nSlotID - 1;
//				sGrowUp.SlotRes[nNo] = new SUIGrowUpSlotRes();
//				SUIGrowUpSlotRes sSlot = sGrowUp.SlotRes[nNo];
//				sSlot.nSlotID = nSlotID;
//				sSlot.resIcon = new ResourceRef();
//				sSlot.resIcon.path = pCSVReader.GetString(nRow, nCol++, "");
//				sSlot.resIcon.UpdateResourceGuid(true);

//				string strEffect = pCSVReader.GetString(nRow, nCol++, "");
//				if (!string.IsNullOrEmpty(strEffect))
//				{
//					szStrList.Clear();
//					XGame.Utils.Api.split(ref szStrList, strEffect, ";");
//					sSlot.arrEffect = new SSlotEffectID[szStrList.Count];
//					for (int i = 0; i < szStrList.Count; i++)
//					{
//						propList.Clear();
//						XGame.Utils.Api.split(ref propList, szStrList[i], ":");
//						if (propList.Count != 3)
//						{
//							Debug.LogError("ImportGrowUpConf sSkinL propList.Count(" + propList.Count + ") != 3 nMode=" + nMode + " nSlotID=" + nSlotID + " nNum=" + (i + 1).ToString() + " Info=" + szStrList[i]);
//							return;
//						}
//						sSlot.arrEffect[i] = new SSlotEffectID();
//						sSlot.arrEffect[i].nBeginLevel = int.Parse(propList[0].Trim());
//						sSlot.arrEffect[i].nEndLevel = int.Parse(propList[1].Trim());
//						sSlot.arrEffect[i].nEffectID = int.Parse(propList[2].Trim());
//					}
//				}
//				sSlot.strDes = pCSVReader.GetString(nRow, nCol++, "");
//				string hex = pCSVReader.GetString(nRow, nCol++, "");
//				byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
//				byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
//				byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
//				byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
//				float r = br / 255f;
//				float g = bg / 255f;
//				float b = bb / 255f;
//				float a = cc / 255f;
//				sSlot.clDes = new Color(r, g, b, a);
//			}
//			ListPool<string>.Release(propList);
//			ListPool<string>.Release(szStrList);


//			asset.uiPayRes.GrowUpRes = lstGrowUp.ToArray();
//			for (int i = 0; i < asset.uiPayRes.GrowUpRes.Length; i++)
//			{
//				SUIGrowUpRes res = asset.uiPayRes.GrowUpRes[i];
//				for (int j = 0; j < res.SlotRes.Length; j ++)
//				{
//					if (res.SlotRes[j].resIcon != null && !string.IsNullOrEmpty(res.SlotRes[j].resIcon.path))
//					{
//						GResources.LoadAsync<Sprite>(res.SlotRes[j].resIcon.path, (path, sp) =>
//						{
//							if (sp == null)
//								Debug.LogError("ImportGrowUpConf resIcon sp=null idx=" + i + " nNo=" + j + " path=" + path);
//						});
//					}
//					else
//					{
//						Debug.LogError("ImportGrowUpConf resIcon idx=" + i + " nNo=" + j + " path=null");
//					}
//				}
//			}
//			EditorUtility.SetDirty(target);
//			AssetDatabase.SaveAssets();
//			Debug.LogError("ImportGrowUpConf Succ");
//		}
//	}
//}

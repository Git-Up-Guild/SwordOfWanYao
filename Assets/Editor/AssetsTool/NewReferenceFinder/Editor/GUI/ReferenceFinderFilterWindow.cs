using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static ReferenceFinder.ReferenceFinderConfig;

namespace ReferenceFinder
{
    public class ReferenceFinderFilterWindow : EditorWindow
    {
        public static class Styles
        {
            public const int buttonWidth = 60;
        }

        private static List<string> typeList = new List<string>();
        private static List<string> folderList = new List<string>();
        private static List<EAssetState> stateList = new List<EAssetState>();

        private Vector2 windowScrollPos;
        private Vector2 typeScrollPos;
        private Vector2 stateScrollPos;
        private Vector2 folderScrollPos;

        private ReferenceFinderWindow mainWindow;
        private SearchField typeSearch;

        private string typeSearchStr = string.Empty;

        public static void OpenWindow(ReferenceFinderWindow mainWindow)
        {
            ReferenceFinderFilterWindow window = GetWindow<ReferenceFinderFilterWindow>();
            window.mainWindow = mainWindow;
            window.wantsMouseMove = false;
            var icon = EditorGUIUtility.FindTexture("FilterByType");
            window.titleContent = new GUIContent("资源引用查找过滤", icon);

            window.Show();
            window.Focus();
            mainWindow.assetTreeView.needUpdateAssetTree = true;
        }

        private void OnEnable()
        {
            typeList = Instance.typeFilterDic.Keys.ToList();
            typeList.Sort();
            folderList = Instance.folderFilterDic.Keys.ToList();
            folderList.Sort();
            stateList = Instance.stateFilterDic.Keys.ToList();
            stateList.Sort();
            typeSearch = new SearchField();
        }

        private void OnDisable()
        {
        }

        private void OnGUI()
        {
            windowScrollPos = GUILayout.BeginScrollView(windowScrollPos);
            {
                GUILayout.BeginHorizontal();
                {
                    ShowTypeList();

                    GUILayout.BeginVertical();
                    {
                        ShowStateList();
                        ShowFolderList();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void ShowTypeList()
        {
            GUILayout.BeginVertical("Box", GUILayout.Width(1));
            {
                ShowTopBar(new GUIContent("类型"), typeList, Instance.typeFilterDic);

                //适应最小宽度
                var rect = EditorGUILayout.GetControlRect();
                typeSearchStr = typeSearch.OnGUI(rect, typeSearchStr);

                typeScrollPos = GUILayout.BeginScrollView(typeScrollPos);
                {
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            foreach (var key in typeList)
                            {
                                if (key.Contains(typeSearchStr))
                                    Instance.typeFilterDic[key].isFilter = GUILayout.Toggle(Instance.typeFilterDic[key].isFilter, key.ToString());
                            }
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            OnSelectChanged();
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        private void ShowStateList()
        {
            GUILayout.BeginVertical("Box");
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("状态", EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EditorGUILayout.Space();
                    if (GUILayout.Button("全选", GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(Styles.buttonWidth)))
                    {
                        foreach (var key in stateList)
                        {
                            Instance.stateFilterDic[key].isFilter = true;
                        }
                        OnSelectChanged();
                    }
                    if (GUILayout.Button("重置", GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(Styles.buttonWidth)))
                    {
                        foreach (var key in stateList)
                        {
                            Instance.stateFilterDic[key].isFilter = false;
                        }
                        OnSelectChanged();
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginVertical();
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        foreach (var key in stateList)
                        {
                            Instance.stateFilterDic[key].isFilter = GUILayout.Toggle(Instance.stateFilterDic[key].isFilter, key.ToString());
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnSelectChanged();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        private void ShowFolderList()
        {
            GUILayout.BeginVertical("Box");
            {
                ShowTopBar(new GUIContent("来源"), folderList, Instance.folderFilterDic);
                ShowList(ref folderScrollPos, folderList, Instance.folderFilterDic);
            }
            GUILayout.EndVertical();
        }

        private void ShowTopBar(GUIContent title, List<string> list, Dictionary<string, stringStruct> dic)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(title, EditorStyles.boldLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.Space();
                if (GUILayout.Button("全选", GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(Styles.buttonWidth)))
                {
                    foreach (var key in list)
                    {
                        dic[key].isFilter = true;
                    }
                    OnSelectChanged();
                }
                if (GUILayout.Button("重置", GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(Styles.buttonWidth)))
                {
                    foreach (var key in list)
                    {
                        dic[key].isFilter = false;
                    }
                    OnSelectChanged();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        private void ShowList(ref Vector2 scrollPos, List<string> list, Dictionary<string, stringStruct> dic)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        foreach (var key in list)
                        {
                            dic[key].isFilter = GUILayout.Toggle(dic[key].isFilter, key.ToString());
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        OnSelectChanged();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }

        private void OnSelectChanged()
        {
            mainWindow.assetTreeView.needUpdateAssetTree = true;
            AssetDatabase.SaveAssets();
        }
    }
}
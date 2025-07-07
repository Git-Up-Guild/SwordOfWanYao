using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ReferenceFinder;
using UnityEngine.UI;
using XGame;
using TMPro;
using XGame.I18N;
using DG.Tweening;
using static DG.Tweening.DOTweenAnimation;
using static UnityEngine.UI.ScrollRect;
using XGame.AssetScript.UI;
using XGame.UI;
using UnityEngine.Events;

using System.Reflection;
using static XGame.UI.Framework.EffList.EffectiveListView;
using XGame.UI.Framework.EffList;

public class ReplaceScrollRectWindow : EditorWindow
{
    //被替换的目录
    string m_replaceFolder = "Assets/G_Resources/Game/Prefab/LuaUI";

    public GameObject m_goReplaceTagert;

    [MenuItem("XGame/资源工具/替换ScrollRect工具")]
    static void ShowWindow()
    {
        GetWindow<ReplaceScrollRectWindow>();
    }

    public ReplaceScrollRectWindow()
    {
        this.titleContent = new GUIContent("替换ScrollRect工具");
        this.minSize = new Vector2(750f, 512f);
    }

    private void OnEnable()
    {
       

    }






    private void OnGUI()
    {



        //单个替换
        m_goReplaceTagert = EditorGUILayout.ObjectField(m_goReplaceTagert, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("单个替换"))
        {
            if (null != m_goReplaceTagert)
            {
                string path = AssetDatabase.GetAssetPath(m_goReplaceTagert);
                ReplaceOnePrefab(path);
            }

        }



        //替换的UI目录
        DrawFolderEdit("替换目录", ref m_replaceFolder);


        if (GUILayout.Button("批量替换"))
        {
            ReplaceTemplate();
        }

        if (GUILayout.Button(" 目录下 ScrollRect 替换成 NestedScrollRect"))
        {

            ReplaceAllScrollRect2NestedScrollRect();
        }

        if (GUILayout.Button("单个 ScrollRect 替换成 NestedScrollRect"))
        {
            string path = AssetDatabase.GetAssetPath(m_goReplaceTagert);
            RelaoceOneScrollRect2NestedScrollRect(path);
        }




    }



    private void DrawFolderEdit(string label, ref string result, bool isFolder = true, bool isCheckExists = true)
    {
        using (var hor = new GUILayout.HorizontalScope())
        {
            result = EditorGUILayout.TextField(label, result);
            if (GUILayout.Button("浏览"))
            {
                result = isFolder ? EditorUtility.OpenFolderPanel(label, result, "") :
                    EditorUtility.OpenFilePanel(label, result, "");
            }
            if (isCheckExists && ((isFolder && !Directory.Exists(result)) || (!isFolder && !File.Exists(result))))
            {
                GUILayout.Box("", "Wizard Error", GUILayout.Width(32), GUILayout.Height(16));
            }
        }

    }





    void ReplaceTemplate()
    {
        string[] filePaths = Directory.GetFiles(m_replaceFolder, "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < filePaths.Length; ++i)
        {
            ReplaceOnePrefab(filePaths[i]);
        }
    }


    private static long GetLocalIdentfierInFile(UnityEngine.Object obj)
    {
        PropertyInfo info = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
        SerializedObject sObj = new SerializedObject(obj);
        info.SetValue(sObj, InspectorMode.Debug, null);
        SerializedProperty localIdProp = sObj.FindProperty("m_LocalIdentfierInFile");
        return localIdProp.longValue;
    }

    private static long SetLocalIdentfierInFile(UnityEngine.Object obj,long localID)
    {
        PropertyInfo info = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
        SerializedObject sObj = new SerializedObject(obj);
        info.SetValue(sObj, InspectorMode.Debug, null);
        SerializedProperty localIdProp = sObj.FindProperty("m_LocalIdentfierInFile");
        localIdProp.longValue = localID;
        sObj.UpdateIfRequiredOrScript();
        return localIdProp.longValue;
    }


    List<ScrollRect> GetNestedScrollRect(GameObject go)
    {
        List<ScrollRect> listSR = new List<ScrollRect>();
        ScrollRect[] aryScroll = go.GetComponentsInChildren<ScrollRect>(true);
        for (int n = 0; n < aryScroll.Length; ++n)
        {

            if (aryScroll[n] as NestedScrollRect)
            {
                continue;
            }

            GameObject curObject = aryScroll[n].gameObject;

          

            Transform parent = curObject.transform.parent;
            if (parent != null&& parent.GetComponentInParent<ScrollRect>()!=null)
            {
                listSR.Add(aryScroll[n]);
            }
        }

        return listSR;
    }

    void ReplaceOnePrefab(string path)
    {
        GameObject go = PrefabUtility.LoadPrefabContents(path);

        if(null== go)
        {
            Debug.LogError("资源不存在:" + path);
            return;
        }

        List<ScrollRect> listSR = GetNestedScrollRect(go);
        bool bModified = false;
        string guiid = "";

        for (int n = 0; n < listSR.Count; ++n)
        {
            ScrollRect scroll = listSR[n];

            GameObject curObject = scroll.gameObject;

            bModified = true;

            RectTransform content = scroll.content;
            bool bHorizontal = scroll.horizontal;
            bool bVertical = scroll.vertical;
            MovementType moveType = scroll.movementType;
            float elasticity = scroll.elasticity;
            float decelerationRate = scroll.decelerationRate;
            float scrollSensitivity  = scroll.scrollSensitivity;
            RectTransform viewport = scroll.viewport;
            Scrollbar horizontalScrollbar = scroll.horizontalScrollbar;
            ScrollbarVisibility horizontalScrollbarVisibility = scroll.horizontalScrollbarVisibility;
            float horizontalScrollbarSpacing = scroll.horizontalScrollbarSpacing;
            Scrollbar verticalScrollbar = scroll.verticalScrollbar;
            ScrollbarVisibility verticalScrollbarVisibility = scroll.verticalScrollbarVisibility;
            float verticalScrollbarSpacing = scroll.verticalScrollbarSpacing;

            ScrollStyle scrollType = ScrollStyle.Grid;
            EffectiveListAnimationPlayerBase animationPlayer = null;
            bool enableAnimation = false;
            UnityEvent onRightBoundaryEvent =null;
            UnityEvent onUpBoundaryEvent =null;
            UnityEvent onLeftBoundaryEvent = null;
            float triggerBoundEventValue = 0;
            bool debugMode = false;
            float alignmentYOffset=0;
            float alignmentXOffset=0;
            float scrollStep = 0;
            UnityEvent onDownBoundaryEvent = null;
            int showRowOrCol = 1;
            bool enableOutViewportCheck = false;
            bool supportLOP = false;
            Transform aniTrigger = null;
            RectTransform itemViewProto = null;
            int showRowOrColExtend = 0;
            int gridColCount = 0;
            int dataCount = 0;
            RectOffset paddings = new RectOffset();
            Vector2 spacing = new Vector2();
            TextAnchor childAlignment = new TextAnchor();
            int ID = 0;

            bool bHadEffectiveList = false;
            EffectiveListView listView = curObject.GetComponent<EffectiveListView>();
            if(null!= listView)
            {

                long oldHashColde = GetLocalIdentfierInFile(listView);
                //AssetDatabase.TryGetGUIDAndLocalFileIdentifier(listView.GetInstanceID(), out guiid, out oldHashColde);

                bHadEffectiveList = true;
                scrollType = listView.scrollType;
                animationPlayer = listView.animationPlayer;
                enableAnimation = listView.enableAnimation;
                onRightBoundaryEvent= listView.onRightBoundaryEvent;
                onUpBoundaryEvent = listView.onUpBoundaryEvent;
                onLeftBoundaryEvent =  listView.onLeftBoundaryEvent;
                triggerBoundEventValue = listView.triggerBoundEventValue;
                debugMode =  listView.debugMode;
                alignmentYOffset =  listView.alignmentYOffset;
                alignmentXOffset = listView.alignmentXOffset; 
                scrollStep = listView.scrollStep; 
                onDownBoundaryEvent = listView.onDownBoundaryEvent; 
                showRowOrCol  = listView.showRowOrCol; 
                enableOutViewportCheck =  listView.enableOutViewportCheck;
                aniTrigger = listView.aniTrigger;
                itemViewProto = listView.itemViewProto;
                showRowOrColExtend = listView.showRowOrColExtend;
                gridColCount = listView.gridColCount;
                dataCount =  listView.dataCount;
                paddings = listView.paddings;
                spacing = listView.spacing; 
                childAlignment = listView.childAlignment;
                ID =  listView.ID;

                GameObject.DestroyImmediate(listView, true);
            }

            GameObject.DestroyImmediate(scroll, true);
            NestedScrollRect nsr = curObject.AddComponent<NestedScrollRect>();

            nsr.content = content;
            nsr.horizontal = bHorizontal;
            nsr.vertical = bVertical;
            nsr.movementType = moveType;
            nsr.elasticity = elasticity;
            nsr.decelerationRate = decelerationRate;
            nsr.scrollSensitivity = scrollSensitivity;
            nsr.viewport = viewport;
            nsr.horizontalScrollbar = horizontalScrollbar;
            nsr.horizontalScrollbarVisibility = horizontalScrollbarVisibility;
            nsr.horizontalScrollbarSpacing = horizontalScrollbarSpacing;
            nsr.verticalScrollbar = verticalScrollbar;
            nsr.verticalScrollbarVisibility = verticalScrollbarVisibility;
            nsr.verticalScrollbarSpacing = verticalScrollbarSpacing;


            if (bHadEffectiveList)
            {
                listView = curObject.AddComponent<EffectiveListView>();
                listView.scrollType = scrollType;
                listView.animationPlayer = animationPlayer;
                listView.enableAnimation = enableAnimation;
                listView.onRightBoundaryEvent = onRightBoundaryEvent;
                listView.onUpBoundaryEvent = onUpBoundaryEvent;
                listView.onLeftBoundaryEvent = onLeftBoundaryEvent;
                listView.triggerBoundEventValue = triggerBoundEventValue;
                listView.debugMode = debugMode;
                listView.alignmentYOffset = alignmentYOffset;
                listView.alignmentXOffset = alignmentXOffset;
                listView.scrollStep = scrollStep;
                listView.onDownBoundaryEvent = onDownBoundaryEvent;
                listView.showRowOrCol = showRowOrCol;
                listView.enableOutViewportCheck = enableOutViewportCheck;
                listView.aniTrigger = aniTrigger;
                listView.itemViewProto = itemViewProto;
                listView.showRowOrColExtend = showRowOrColExtend;
                listView.gridColCount = gridColCount;
                listView.dataCount = dataCount;
                listView.paddings = paddings;
                listView.spacing = spacing;
                listView.childAlignment = childAlignment;


                //newHashColde = GetLocalIdentfierInFile(listView);
               // SetLocalIdentfierInFile(listView,oldHashColde);
               // newHashColde = GetLocalIdentfierInFile(listView);
                //AssetDatabase.TryGetGUIDAndLocalFileIdentifier(listView.GetInstanceID(), out guiid, out oldHashColde);

                //listView.Init();
                //listView.ID = ID;
            }

        }

        if (bModified)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        PrefabUtility.UnloadPrefabContents(go);

        AssetDatabase.SaveAssets();

        /*
        if (oldHashColde!=0)
        {
           
            string conent = File.ReadAllText(path);
            string oldValue = oldHashColde.ToString();
            string newValue = newHashColde.ToString();
            conent.Replace(oldValue, newValue);

            Debug.LogError("替換 effecitvie list 引用= " + oldValue+ ", newValue="+ newValue);
            File.Delete(path);
            File.WriteAllText(path, conent);
        }
        */

    }

    string oldGuid = "1aa08ab6e0800fa44ae55d278d1423e3"; //the old 'guid references in the prefab file'
    string newGuid = "466f03732d4142343855a71b6331f739"; //the new guid of the new type

    private void RelaoceOneScrollRect2NestedScrollRect(string path)
    {
        var text = System.IO.File.ReadAllText(path);
        text = text.Replace(oldGuid, newGuid);
        System.IO.File.WriteAllText(path, text);
        AssetDatabase.SaveAssets();
    }

    private void ReplaceAllScrollRect2NestedScrollRect()
    {
       
        string[] filePaths = Directory.GetFiles(m_replaceFolder, "*.prefab", SearchOption.AllDirectories);
        for (int i = 0; i < filePaths.Length; ++i)
        {
            RelaoceOneScrollRect2NestedScrollRect(filePaths[i]);
        }

    }




}










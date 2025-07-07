using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GameScripts;
using XClient.Entity;

namespace GameScripts
{
    public class SpriteComparer : IComparer<Sprite>
    {
        public int Compare(Sprite x, Sprite y)
        {
            return x.name.CompareTo(y.name);
        }
    }

    [CustomEditor(typeof(SpriteRenderAni))]
    public class SpriteRenderAniEditor : Editor
    {
        private SpriteRenderAni spriteRenderAni;

        private void OnEnable()
        {
            spriteRenderAni = target as SpriteRenderAni;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("生成动画组"))
            {
                SpriteRenderer sr = spriteRenderAni.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    Debug.LogError("请添加 Sprite 组件 SpriteRender");
                    return;
                }

                Sprite s = sr.sprite;
                if (s == null)
                {
                    Debug.LogError("请保证 SpriteRender 的sprite不为null");
                    return;
                }

                string path = AssetDatabase.GetAssetPath(s);

                //Debug.LogError("path=" + path);

                string dir = Path.GetDirectoryName(path);
                //Debug.LogError("path=" + dir);
                string[] filePaths = Directory.GetFiles(dir, "*.png");
                Dictionary<string, List<Sprite>> dicActionCfg = new Dictionary<string, List<Sprite>>();
                string spritePath = null;
                string frameName = null;
                string aniName = null;
                int nLen = filePaths.Length;
                List<Sprite> listAni = null;
                for (int i = 0; i < nLen; ++i)
                {
                    spritePath = filePaths[i].Replace("\\", "/");
                    int nIndex = spritePath.LastIndexOf("/");
                    aniName = spritePath.Substring(nIndex + 1, spritePath.Length - nIndex - 7);
                    if (dicActionCfg.TryGetValue(aniName, out listAni) == false)
                    {
                        listAni = new List<Sprite>();
                        dicActionCfg.Add(aniName, listAni);
                    }

                    s = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    listAni.Add(s);
                }



                ActionItem actionItem = null;
                spriteRenderAni.actionItems.Clear();
                foreach (string name in dicActionCfg.Keys)
                {
                    listAni = dicActionCfg[name];
                    listAni.Sort(new SpriteComparer());
                    actionItem = new ActionItem();
                    actionItem.name = name;
                    actionItem.listSprite = listAni;
                    spriteRenderAni.actionItems.Add(actionItem);

                }



                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }

            spriteRenderAni?.Update();

        }


    }

}



using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using XGameEditor;
using UnityEngine.U2D;

namespace WXB.Editor
{
    public class UHyperTextEditorUtility
    {
        [MenuItem("XGame/uHyperText/配置文件")]
        static SymbolTextSettings CreateOrFindSettings()
        {
            string assetPath = "Assets/uHyperText/Res/SymbolTextSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<SymbolTextSettings>(assetPath);
            if(settings == null)
            {
                settings = ScriptableObject.CreateInstance<SymbolTextSettings>();
                AssetDatabase.CreateAsset(settings, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUIUtility.PingObject(settings);
            Selection.activeObject = settings;

            return settings;
        }

        [MenuItem("XGame/uHyperText/更新表情动画")]
        static void UpdateAnim()
        {
            //生图集
            var settings = CreateOrFindSettings();

            //从图集中获取Sprite
            Sprite[] allSprites = new Sprite[settings.animSpriteAtlas.spriteCount];
            settings.animSpriteAtlas.GetSprites(allSprites);
            Dictionary<string, Sprite> dicSprites = new Dictionary<string, Sprite>();
            foreach (Sprite s in allSprites)
            {
                dicSprites.Add(s.texture.name, s);
            }

            //根据名称进行动画分类
            Dictionary<string, Cartoon> Cartoons = new Dictionary<string, Cartoon>();
            List<Sprite> tempss = new List<Sprite>();
            for (int i = 0; i < 1000; ++i)
            {
                string animName = string.Format("anim_{0}_", i);
                for (int j = 0; j < 100; ++j)
                {
                    string frameName = animName + j;
                    Sprite s = null;
                    if (dicSprites.TryGetValue(frameName, out s))
                        tempss.Add(s);
                }

                if (tempss.Count != 0)
                {
                    Cartoon c = new Cartoon();
                    c.name = i.ToString();
                    c.fps = 5f;
                    c.sprites = new Sprite[tempss.Count];
                    c.spriteNames = new string[tempss.Count];
                    for(int k = 0; k < tempss.Count; ++k)
                    {
                        c.spriteNames[k] = tempss[k].texture.name;
                    }
                    c.width = (int)tempss[0].rect.width;
                    c.height = (int)tempss[0].rect.height;
                    c.spriteAtlas = settings.animSpriteAtlas;
                    Cartoons.Add(i.ToString(), c);
                    tempss.Clear();
                }
            }

            //排个序
            List<Cartoon> cartoons = new List<Cartoon>(Cartoons.Values);
            cartoons.Sort((Cartoon x, Cartoon y) =>
            {
                return int.Parse(x.name).CompareTo(int.Parse(y.name));
            });

            //修改配置
            settings.cartoons = cartoons.ToArray();

            //保存
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XGame.UI
{
    static public class MenuExtend
    {
        /// <summary>
        /// 获取当前选中对象
        /// </summary>
        /// <returns></returns>
        static public Transform GetSelectParent()
        {
            GameObject[] arrObj = Selection.GetFiltered<GameObject>(SelectionMode.TopLevel);
            if (arrObj.Length > 0)
            {
                return arrObj[0].transform;
            }
            return null;
        }


        [MenuItem("GameObject/UI/Button - Ex")]
        static public GameObject CreateI18NImage()
        {
            DefaultControls.Resources uiResources = new DefaultControls.Resources();    // 默认资源，可以设置默认图片之类的
            
            GameObject go = DefaultControls.CreateButton(uiResources);      // 创建Unity默认UI文本
            go.transform.BetterSetParent(GetSelectParent());
            go.transform.localPosition = Vector3.zero;
            go.layer = LayerMask.NameToLayer("UI");
            go.name = "Button_Ex";

            GameObject.DestroyImmediate(go.GetComponent<Button>());     // 删掉默认Button，加自己的
            var com = go.AddComponent<ButtonEx>();
            com.transform.localScale = Vector3.one;

            var img = go.GetComponent<Image>();
            img.sprite = (Sprite)AssetDatabase.GetBuiltinExtraResource(typeof(Sprite), "UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            return go;
        }
    }
}

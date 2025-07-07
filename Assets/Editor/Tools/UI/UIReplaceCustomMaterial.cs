/*******************************************************************
** 文件名:	UIReplaceCustomMaterial
** 版  权:	
** 创建人:	熊纪刚
** 日  期:	2019/5/23
** 版  本:	1.0
** 描  述:	用自定义材质替换ui默认材质
* 
* image控件中的材质替换为ImageSimple
* Text控件中的材质替换为TextSimple
* 替换后再游戏中测试下，有些控件显示有问题，这样的控件现在处理方式为换回默认材质
* image中纹理为空的不能替换
* 
** 应  用:  优化ui材质

**************************** 修改记录 ******************************
** 修改人:
** 日  期:
** 描  述:
********************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace XGameEditor
{
    public class UIReplaceCustomMaterial : EditorWindow
    {
        [MenuItem("XGame/UI工具/替换材质")]
        static void AddWindow()
        {
            //创建窗口
            Rect wr = new Rect(0, 0, 500, 300);
            UIReplaceCustomMaterial window = (UIReplaceCustomMaterial)EditorWindow.GetWindowWithRect(typeof(UIReplaceCustomMaterial), wr, true, "widow name");
            window.Show();
        }

        private GameObject m_uiPrefab;

        private Material m_imageCustomMaterial = null;

        private Material m_textCustomMaterial = null;

        public void Awake()
        {
        }


        //绘制窗口时调用
        void OnGUI()
        {
            m_uiPrefab = EditorGUILayout.ObjectField("UIPrefab", m_uiPrefab, typeof(GameObject), true) as GameObject;

            m_imageCustomMaterial = EditorGUILayout.ObjectField("ImageMaterial", m_imageCustomMaterial, typeof(Material), true) as Material;

            m_textCustomMaterial = EditorGUILayout.ObjectField("TextMaterial", m_textCustomMaterial, typeof(Material), true) as Material;

            if (GUILayout.Button("替换", GUILayout.Width(200)))
            {
                replaceMaterial();
            }

            if (GUILayout.Button("RestNullImage", GUILayout.Width(200)))
            {
                resetNullImage();
            }

            if (GUILayout.Button("RestBG_BlackImage", GUILayout.Width(200)))
            {
                restBg_BlackImage();
            }
        }

        void replaceMaterial()
        {
            replaceImageMaterial();

            replaceTextMaterial();
        }

        //把没有纹理的image控件材质设置为默认材质
        void resetNullImage()
        {
            Image[] imageList = m_uiPrefab.GetComponentsInChildren<Image>(true);
            //GameObject go = GameObject.Find("TabBg");
            //Image im = go.GetComponent<Image>();

            foreach (Image image in imageList)
            {
                if (image.mainTexture.name == "UnityWhite")
                {
                    image.material = null;
                }
            }
        }

        //使用bg_black为图片的image不能替换
        void restBg_BlackImage()
        {
            Image[] imageList = m_uiPrefab.GetComponentsInChildren<Image>(true);
            //GameObject go = GameObject.Find("TabBg");
            //Image im = go.GetComponent<Image>();

            foreach (Image image in imageList)
            {
                if (image.mainTexture.name == "bg_black")
                {
                    image.material = null;
                }
            }
        }

        //判断这个image是否可以替换
        //如果纹理为空不替换
        bool canReplace(Image image)
        {
            if (image.mainTexture.name == "UnityWhite")
            {
                return false;
            }

            return true;
        }

        void replaceImageMaterial()
        {
            if (m_imageCustomMaterial == null)
            {
                return;
            }

            Image[] imageList = m_uiPrefab.GetComponentsInChildren<Image>(true);
            //  Image [] imageList = GameObject.FindObjectsOfType<Image>();

            foreach (Image image in imageList)
            {
                if (canReplace(image))
                {
                    image.material = m_imageCustomMaterial;
                }
            }
        }

        void replaceTextMaterial()
        {
            if (m_textCustomMaterial == null)
            {
                return;
            }

            //Text[] textList = m_uiPrefab.GetComponentsInChildren<Text>();
            Transform[] objs = m_uiPrefab.GetComponentsInChildren<Transform>(true);

            //replaceTextMaterial(m_uiPrefab);
            foreach (Transform child in objs)
            {
                Text[] textList = child.gameObject.GetComponents<Text>();

                foreach (Text text in textList)
                {
                    text.material = m_textCustomMaterial;
                }
            }

        }

        //更新
        void Update()
        {

        }

        void OnInspectorUpdate()
        {
            //Debug.Log("窗口面板的更新");
            //这里开启窗口的重绘，不然窗口信息不会刷新
            this.Repaint();
        }

        void OnDestroy()
        {
        }
    }
}
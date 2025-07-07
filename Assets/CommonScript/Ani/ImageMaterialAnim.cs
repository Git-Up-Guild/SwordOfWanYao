/*******************************************************************
** 文件名:	ImageMaterialAnim.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2025.2.27
** 版  本:	1.0
** 描  述:	
** 应  用:  更新材质属性

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameScripts.ImageAni.ImageMaterialAnim;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GameScripts.ImageAni
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Image))]

    [ExecuteInEditMode]
    public class ImageMaterialAnim : MonoBehaviour
    {
        [System.Serializable]
        public class Data
        {
            public string name;
            public string type;
            public bool enable;
        }

        public Material m_ImageMat;
        public List<Data> Property = new List<Data>();
        //是否支持多实例
        public bool supportMulInstance = false;

        private Image m_Image;
        private MeshRenderer m_MeshRenderer;
        private MaterialPropertyBlock m_MaterialPropertyBlock;

        private void Awake()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_MaterialPropertyBlock = new MaterialPropertyBlock();
            m_Image = GetComponent<Image>();

            if(supportMulInstance)
            {
                //让 Image 和 meshrender 公用一份材质
                m_ImageMat = Instantiate(m_Image.material);
                m_Image.material = m_ImageMat;
                /*
                if(m_MeshRenderer)
                {
                    m_MeshRenderer.material = m_ImageMat;
                }
                */
            }
           
           

            if(null!= m_MeshRenderer)
            {
                m_MeshRenderer.enabled = false;
                m_MeshRenderer.material = m_Image.material;
            }

#if UNITY_EDITOR
            DelRedundancyData();
#endif

        }

        private void LateUpdate()
        {

            bool inPack = false;

            if (m_MeshRenderer.HasPropertyBlock())
            {

                if(null== m_MaterialPropertyBlock)
                {
                    m_MaterialPropertyBlock = new MaterialPropertyBlock();
                }

                m_MeshRenderer.GetPropertyBlock(m_MaterialPropertyBlock);
                foreach (var item in Property)
                {
                    if (item.enable)
                    {
                        SetValue(item.name, item.type);
                    }
                }

                if(m_Image!=null&& m_Image.sprite!=null)
                {
                    if(m_Image.sprite.packed)
                    {
                        Rect textureRect = m_Image.sprite.textureRect;
                        Texture2D texture = m_Image.sprite.texture;
                        float w = texture.width;
                        float h = texture.height;

                        float starX = textureRect.x / texture.width;
                        float starY = textureRect.y/ texture.height;// (texture.height-textureRect.y) / texture.height;
                        float spriteW = textureRect.width / texture.width;
                        float spriteH = textureRect.height / texture.height;
                        Vector4 packInfo = new Vector4(starX, starY, spriteW, spriteH);

                        inPack = true;
                        m_Image.material.SetVector("_PackInfo", packInfo);

                       // Debug.LogError("packInfo"+ packInfo);
                    }
                   
                }
            }

            if(inPack==false)
            {
                if (m_Image && m_Image.material != null)
                {
                    m_Image.material.SetVector("_PackInfo", new Vector4(0, 0, 1, 1));
                }
            }
        

        }

        private void OnDestroy()
        {
            if(supportMulInstance)
            {
                if(null!= m_ImageMat)
                {
                    Object.DestroyImmediate(m_ImageMat);
                    m_ImageMat = null;
                }
            }
        }
        void SetValue(string name, string type)
        {
            switch (type)
            {
                case "Color":
                    m_Image.color = m_MaterialPropertyBlock.GetColor(name);
                    break;
                case "Float":
                case "Range":
                    //float value = m_MaterialPropertyBlock.GetFloat(name);
                    m_Image.material.SetFloat(name, m_MaterialPropertyBlock.GetFloat(name));
                    break;
                case "Vector":
                    //Vector4 v = m_MaterialPropertyBlock.GetVector(name);
                    m_Image.material.SetVector(name, m_MaterialPropertyBlock.GetVector(name));
                    break;
                default:
                    break;
            }

        }

#if UNITY_EDITOR
        public void DelRedundancyData()
        {
            if (null == this.Property|| this.Property.Count==0)
            {
                return;
            }

            List<Data> newProperty = new List<Data>();
            int nCount = this.Property.Count;
            for (int i = 0; i < nCount; ++i)
            {
                if (this.Property[i].enable)
                    newProperty.Add(this.Property[i]);
            }

            this.Property = newProperty;
           

            AssetDatabase.SaveAssetIfDirty(this);
        }

        public void RefreshData()
        {


            if (null != m_MeshRenderer)
            {
                m_MeshRenderer.sharedMaterial = m_Image.material;
            }

            //没有材质的，先返回
            if (null == m_MeshRenderer.sharedMaterial)
            {
                return;
            }



            List<ImageMaterialAnim.Data> oldProperty = this.Property;
            this.Property = new List<ImageMaterialAnim.Data>();


            
            Shader shader = m_MeshRenderer.sharedMaterial.shader;
            int count = shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                string name = shader.GetPropertyName(i);
                bool findOld = false;
                for(int j=0;j< oldProperty.Count;++j)
                {
                    if (oldProperty[j].name== name)
                    {
                        this.Property.Add(oldProperty[j]);
                        findOld = true;
                    }
                }
                //找不到老的,才用新的
                if(!findOld)
                    this.Property.Add(new ImageMaterialAnim.Data() { name = name, type = shader.GetPropertyType(i).ToString() });
            }
        }
#endif

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ImageMaterialAnim))]
    public class ImageAnimEditor : Editor
    {
        ImageMaterialAnim imageAnim;
        MeshRenderer meshrenderer;
        private void OnEnable()
        {
            imageAnim = (target as ImageMaterialAnim);
            meshrenderer = imageAnim.GetComponent<MeshRenderer>();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("刷新") || imageAnim.Property == null || imageAnim.Property.Count == 0)
            {
                imageAnim.RefreshData();
            }

            if (GUILayout.Button("删除冗余"))
            {
                imageAnim.DelRedundancyData();
            }
        }
    }
#endif
}

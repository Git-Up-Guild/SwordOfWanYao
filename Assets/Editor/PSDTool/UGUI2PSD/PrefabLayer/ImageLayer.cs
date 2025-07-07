using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUI2PSD
{
    public class ImageLayer : PrefabLayerBase
    {
        public override EPrefabLayerType LayerType => EPrefabLayerType.Image;

        public Color32[] pixels { get; private set; }

        private string basePath = Application.dataPath + "/../";
        private Texture2D m_texture2D;

        public ImageLayer(Transform tran) : base(tran)
        {
            Image img = trans.GetComponent<Image>();
            if (img && img.sprite)
            {
                m_texture2D = img.sprite.texture;
                PsRect.width = m_texture2D.width;
                PsRect.height = m_texture2D.height;
            }

            //解析图片数据
            ResolveLayerInfo();
        }

        public override void ResolveLayerInfo()
        {
            string ImagePath = GetImagePath();
            bool isNullOrDefault = IsDefaultOrEmptyImage(ImagePath);

            if (isNullOrDefault)
            {
                int count = (int)(PsRect.width * PsRect.height);
                pixels = new Color32[count];
            }
            else
            {
                pixels = GetImageColorByIO(basePath + ImagePath, m_texture2D.width, m_texture2D.height);
            }
        }

        //相对于工程的路径 （例如：Assets/UI/commom/xinbiaoti.png）
        private string GetImagePath()
        {
            if (m_texture2D == null)
                return null;

            int instanceID = m_texture2D.GetInstanceID();
            string imgPath = AssetDatabase.GetAssetPath(instanceID);
            return imgPath;
        }

        //是否没有图片或者是系统默认图片（非外部导入的图片）
        private bool IsDefaultOrEmptyImage(string imgPath)
        {
            if (string.IsNullOrEmpty(imgPath))
                return true;
            else
                return imgPath.Contains("Resources") || imgPath.Contains("resources");
        }

        /// <summary>
        /// 以IO方式进行加载
        /// </summary>
        private Color32[] GetImageColorByIO(string ImagePath, int width, int height)
        {
            Print($"ImagePath:{ImagePath}");
            //创建文件读取流
            FileStream fileStream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            //创建文件长度缓冲区
            byte[] bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;

            //创建Texture
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(bytes);

            Color32[] colors = texture.GetPixels32();
            return colors;
        }


        /// <summary>
        /// 通过FileStream 来打开文件，这样就可以实现不锁定Image文件，到时可以让多用户同时访问Image文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /*private Bitmap ReadImageFile(string path)
        {
            if (!File.Exists(path))
            {
                return;//文件不存在
            }
            FileStream fs = File.OpenRead(path); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组 
            fs.Read(image, 0, filelength); //按字节流读取 
            System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
            fs.Close();
            Bitmap bit = new Bitmap(result);
            return bit;
        }*/
    }
}

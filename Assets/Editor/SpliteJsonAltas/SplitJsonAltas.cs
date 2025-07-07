using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class SplitJsonAltas : MonoBehaviour
{

    public string dirPath = "";

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        if(string.IsNullOrEmpty(dirPath))
        {
            return;
        }

        string[] paths = Directory.GetFiles(dirPath,"*.png",SearchOption.AllDirectories);
        for(int i=0;i< paths.Length;++i)
        {
            try
            {
                string jsonPath = paths[i].Replace(".png", ".json");
                if (File.Exists(jsonPath) == false)
                {
                    continue;
                }
                SplitJsonImage(jsonPath, paths[i]);

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                
            }

          
        }
    }

    void SplitJsonImage(string jsonPath,string texPath)
    {
        string json = File.ReadAllText(jsonPath);

        string xml = string.Empty;
        XmlDocument xmlDoc = new XmlDocument();
        XmlDictionaryReader xmlReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), XmlDictionaryReaderQuotas.Max);
        xmlDoc.Load(xmlReader);

        Dictionary<string, string> dicItems = new Dictionary<string, string>();
        dicItems.Add("x", "");
        dicItems.Add("y", "");
        dicItems.Add("width", "");
        dicItems.Add("height", "");
        dicItems.Add("name", "");
        dicItems.Add("frameY", "");
        dicItems.Add("frameWidth", "");
        dicItems.Add("frameX", "");
        dicItems.Add("frameHeight", "");


        XmlNode Root = xmlDoc.FirstChild;
        XmlNode curRoot = Root;
        while (curRoot != null)
        {
            XmlNodeList NodeList = curRoot.ChildNodes;
            foreach (XmlNode node in NodeList)
            {
                XmlNodeList NodeChildList = node.ChildNodes;
                foreach (XmlNode childnode in NodeChildList)
                {
                    if (childnode.ChildNodes.Count < 4)
                    {
                        continue;
                    }

                    foreach (XmlNode childnode2 in childnode)
                    {
                        if (dicItems.ContainsKey(childnode2.Name))
                        {
                            dicItems[childnode2.Name] = childnode2.InnerText;
                        }
                    }

                    int x = int.Parse(dicItems["x"]);
                    int y = int.Parse(dicItems["y"]);
                    int w = int.Parse(dicItems["width"]);
                    int h = int.Parse(dicItems["height"]);
                    int fx = int.Parse(dicItems["frameX"]);
                    int fy = int.Parse(dicItems["frameY"]);
                    int fw = int.Parse(dicItems["frameWidth"]);
                    int fh = int.Parse(dicItems["frameHeight"]);

                    string name = dicItems["name"];

                    OutPutImg(texPath, name, x, y, w, h, fx, fy, fw, fh);

                    //OutPutImg(texPath, name, 0, 1800, 1024, 224);


                }
            }
            curRoot = curRoot.NextSibling;

        }
    }

    void OutPutImg(string srcPath,string name,int x,int y,int w,int h, int fx, int fy, int fw, int fh)
    {
        string pathDir = null;
        int nLostSuffix = srcPath.LastIndexOf(".");
        if(nLostSuffix>0)
        {
            pathDir = srcPath.Substring(0, nLostSuffix);

            if(Directory.Exists(pathDir)==false)
            {
                Directory.CreateDirectory(pathDir);
            }
        }


        string dstPath = pathDir + "/" + name + ".png";
        int pos = srcPath.IndexOf("Assets");
        if(pos<0)
        {
            Debug.LogError("资源不在 Assets 下面");
            return;
        }

        string srcAssetPath = srcPath.Substring(pos);


        
        Texture2D src = AssetDatabase.LoadAssetAtPath<Texture2D>(srcAssetPath);
        Texture2D dst = new Texture2D(fw, fh, TextureFormat.ARGB32, false);

        //先将内容擦成透明
        Color[] colors = dst.GetPixels();
        for(int i=0;i<colors.Length;++i)
        {
            colors[i].a = 0.0f;
        }
        dst.SetPixels(colors);


        //倒转y
        y = src.height - y - h;
        int offsetx = -fx ;
        int offsety = fh - h+fy ;

        colors = src.GetPixels(x, y, w, h);
        dst.SetPixels(offsetx, offsety, w, h, colors);



        dst.Apply();
        byte[] bytes = dst.EncodeToPNG();

        if(File.Exists(dstPath))
        {
            File.Delete(dstPath);
        }

        File.WriteAllBytes(dstPath,bytes);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

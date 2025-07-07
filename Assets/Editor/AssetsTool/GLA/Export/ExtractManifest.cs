
/*******************************************************************
** 文件名: AssetBundleBuilder.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:     郑秀程
** 日  期:    2016/3/29
** 版  本:    1.0
** 描  述:    AssetBundle打包工具类
** 应  用:    1、打包的时候会自动检索所有GResources目录及相关的依赖项
**            2、默认每一个资源打成一个AssetBundle（同一个目录下相同名字不同后缀的资源会打到一起）,可以通过添加Special Directories来指定需要打
**				 包到一起的资源（注意，如果被指定的目录为GResources根目录，则该目录下的文件将打到gresources.datas中；Special Directories的目录可以重叠，
**               一个文件如果同时属于多个Special Directories，则以最深的目录为准）
**			  3、Pack By Dependency Directories里面的资源在打包的时候会把所依赖的资源（指定忽略依赖关系的目录除外)也打进来，被依赖的资源如果同时被Pack By Dependency Directories
**				 里面的多个资源依赖，这个资源会被打到目录最浅的被依赖的资源的AssetBundle里面，Pack By Dependency Directories目录下面的资源
**               本身不会打到被依赖的资源的AssetBundle里面。建议用来打包比较独立的资源，如一个角色相关的资源对应一个AssetBundle。对于复用性
**               比较高的依赖资源，建议使用默认的打包方式按类型打成单独的AssetBundle（资源目录按类型分类）.
**            4、对于UI图集，所有相同TAG的Sprite打到一个AssetBundle（以第一个检索到拥有该TAG的资源路径为AssetBundleName计算依据） 
**            5、打包的时候可以对指定的资源目录进行加密，目前加密是对打包后的AssetBundle进行整体AES加密（后续可以考虑只加密部分，如Head），
**               加密后的AssetBundle将打包到不加密的AssetBundle，方便加载；如果有指定Special Directories，请确保要加密的Asset所在的AssetBundle
**               包含的所有资源都设置成加密， 否则，AssetBundle是否机密，将取决于第一个加入该AssetBundle的Asset
**            6、打包支持当前平台以及同时打包多个平台，同时打包多个平台的时候，会自动触发平台的切换，可能会比较耗时，并且需要有对应平台的Module支持
**            7、打包Dll的时候，请确定被打包Dll不被CS文件依赖（可以被打包的DLL依赖）
**            8、打包的配置会自动保存，方便下一次进行打包或者在其他机器上打包
**
**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
#define AB_DEBUG
#define USE_ASSET_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XGameEditor
{
    public class ExtractManifest
    {

        public static void Extract()
        {
            string outputdir = GetArg("-outputdir");
            ManifestExtract(outputdir);
        }

        /// <summary>
        /// 遍历目录下所有文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static List<string> FilesPathGeter(string dir)
        {
            List<string> rtn = new List<string>();
            rtn = Directory.GetFiles(dir).ToList();

            List<string> cdir = new List<string>(Directory.GetDirectories(dir));
            if (cdir.Count > 0)
            {
                foreach (string item in cdir)
                {
                    rtn = rtn.Union(FilesPathGeter(item)).ToList();
                }
            }
            return rtn;

        }
        //分离manifest文件
        public static void ManifestExtract(string assetbundlePath)
        {
            List<string> allFiles = FilesPathGeter(assetbundlePath);
            allFiles.RemoveAll(item => Path.GetExtension(item) != ".manifest");
            string maniFestDir = Path.Combine(assetbundlePath, "Manifest");
            foreach (string item in allFiles)
            {
                if (!string.IsNullOrEmpty(maniFestDir))
                {
                    string outpath = item.Replace(assetbundlePath, maniFestDir);
                    string targetdir = Path.GetDirectoryName(outpath);
                    if (!Directory.Exists(targetdir))
                    {
                        Directory.CreateDirectory(targetdir);
                    }
                    File.Copy(item, outpath, true);
                }
                File.Delete(item);

            }
            //Directory.CreateDirectory();
        }
        private static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

    }
}
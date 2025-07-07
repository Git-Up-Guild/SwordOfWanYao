///*******************************************************************
//** 文件名: subpackageTool.cs
//** 版  权: (C) 深圳冰川网络技术有限公司 
//** 创建人: 昔文博 
//** 日  期: 2018/10/26
//** 版  本: 1.0
//** 描  述: 内置资源分包工具   
//** 应  用:     

//**************************** 修改记录 ******************************
//** 修改人:  
//** 日  期: 
//** 描  述: 
//********************************************************************/
//#define USE_ASSET_EDITOR

//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using XGame.Asset.Manager;
//using XGame.Utils;
//using XGameEditor.AssetEditor;
//using XGame.CsvReader;

//namespace XGameEditor
//{
//    public class subpackageTool : EditorWindow
//    {
//        public static void SubBuiltInPackage()
//        {
//            string[] args = { GetArg("-csvpath"), GetArg("-srcpath"), GetArg("-outpath") };
//            BuildPackge(args);
//        }

//        public static void BuildFullPackage()
//        {
//            string[] args = { GetArg("-srcpath") };
//            //BuildFullPackge(args);
//        }

//        public static void CopyBuiltInPackage()
//        {
//            string[] args = { GetArg("-srcpath"), GetArg("-outpath") };
//            //CopyPackge(args);
//        }


//        [MenuItem("XGame/Util/subpackage")]
//        public static void OpenDeleteHelperWindow()
//        {
//            subpackageTool tool = GetWindow<subpackageTool>();
//            tool.Show();
//        }

//        //分包的文件，哪些文件需要放在内包的（ABID/AB名称）
//        private string csvPath = @"C:\workplace\xGame3002\op\ab\Android_split\Data\split.csv";

//        //资源的路径
//        private string srcPath = @"C:\workplace\xGame3002\op\ab\testsrc\data";

//        //要放到内包的资源
//        private string destPath = @"C:\workplace\xGame3002\op\ab\testoutput\data";

//        public void OnGUI()
//        {
//            csvPath = EditorGUILayout.TextField("split.csv 表位置", csvPath);
//            srcPath = EditorGUILayout.TextField("总资源目录", srcPath);
//            destPath = EditorGUILayout.TextField("输出资源目录", destPath);
//            if (GUILayout.Button("分离出内包资源"))
//            {
//                string[] args = { csvPath, srcPath, destPath };
//                BuildPackge(args);
//            }
//        }

//        //[MenuItem("Util/subFullPackage")]
//        public static void TestSubFullPackage()
//        {
//            string srcPath = "X:\\Users\\TTCX_Xwb\\Desktop\\ddd";

//            string[] args = { srcPath };
//            //BuildFullPackge(args);
//        }

//        //[MenuItem("Util/copypackage")]
//        public static void TestCopypackage()
//        {
//            string srcPath = "X:\\Users\\TTCX_Xwb\\Desktop\\Data_19031201";
//            string destPath = "X:\\Users\\TTCX_Xwb\\Desktop\\cc";

//            string[] args = { srcPath, destPath };
//            //CopyPackge(args);
//        }

//#if USE_ASSET_EDITOR
//        private static Dictionary<int, List<int>> sDictPackageDeps = new Dictionary<int, List<int>>();

//        private static void InitPackageDeps(string srcPath)
//        {
//            sDictPackageDeps.Clear();
//            AssetBundle bundle = AssetBundle.LoadFromFile(srcPath + GAssetDef.AssetInfoRecordPackageID);
//            if (bundle == null)
//                return;
//            AssetInfoRecord infoRecord = bundle.LoadAsset(GAssetDef.AssetInfoRecordAssetID.ToString()) as AssetInfoRecord;
//            if (infoRecord != null)
//            {
//                Dictionary<int, AssetInfo> dictAssetInfo = new Dictionary<int, AssetInfo>();
//                for (int i = 0; i < infoRecord.assetInfos.Count; i++)
//                {
//                    AssetInfo info = infoRecord.assetInfos[i];
//                    if (!dictAssetInfo.ContainsKey(info.assetID))
//                        dictAssetInfo.Add(info.assetID, info);
//                }

//                foreach (AssetInfo info in dictAssetInfo.Values)
//                {
//                    List<int> deps;
//                    if (!sDictPackageDeps.TryGetValue(info.packageID, out deps))
//                    {
//                        deps = new List<int>();
//                        sDictPackageDeps.Add(info.packageID, deps);
//                    }

//                    foreach (int assetID in info.dpAssets)
//                    {
//                        int packageID = dictAssetInfo[assetID].packageID;
//                        if (packageID != info.packageID
//                            && !deps.Contains(packageID))
//                            deps.Add(packageID);
//                    }
//                }
//            }
//            bundle.Unload(true);
//        }
//        private static List<int> GetPackageDeps(int package)
//        {
//            List<int> ret;
//            sDictPackageDeps.TryGetValue(package, out ret);
//            return ret;
//        }
//        private static void BuildPackageOne(string srcPath, string outPath, int package, bool collectDep, ref List<int> splitList)
//        {
//            if (splitList.Contains(package))
//                return;

//            if (!File.Exists(srcPath + package))
//            {
//                Debug.LogError("File not exist package=" + package);
//                return;
//            }

//            string outDir = outPath + package;
//            int index = outDir.LastIndexOf("/");

//            if (index <= 0)
//                return;

//            outDir = outDir.Substring(0, index);

//            if (!Directory.Exists(outDir))
//            {
//                Directory.CreateDirectory(outDir);
//            }

//            File.Copy(srcPath + package, outPath + package, true);

//            splitList.Add(package);

//            if (collectDep == false)
//                return;

//            Debug.LogFormat("【导出分包资源 {0} -> {1}", srcPath + package, outPath + package);
//            List<int> deps = GetPackageDeps(package);
//            if (deps == null)
//            {
//                return;
//            }
//            foreach (var dep in deps)
//            {
//                BuildPackageOne(srcPath, outPath, dep, collectDep, ref splitList);
//            }
//        }

//        private static List<int> BuildPackgeNew(string srcPath, string outPath, ScpReader csvReader, out BinParser abBin)
//        {
//            InitPackageDeps(srcPath);

//            File.Copy(srcPath + GAssetDef.AssetInfoRecordPackageID, outPath + GAssetDef.AssetInfoRecordPackageID, true);
//            File.Copy(srcPath + GAssetDef.ABListBinFileName, outPath + GAssetDef.ABListBinFileName, true);
//            //File.Copy(srcPath + AssetEditor.GAssetDef.MappingFilePackageID, outPath + AssetEditor.GAssetDef.MappingFilePackageID, true);
//            abBin = new BinParser(outPath + GAssetDef.ABListBinFileName);

//            List<int> splitList = new List<int>();
//            //splitList.Add(AssetEditor.GAssetDef.MappingFilePackageID);
//            for (int nRow = 0; nRow < csvReader.GetRecordCount(); nRow++)
//            {
//                bool collectDep = false;
//                int abID = csvReader.GetInt(nRow, 0, -1);
//                if (abID == -1)
//                {
//                    string szName = csvReader.GetString(nRow, 1, null);
//                    if (string.IsNullOrEmpty(szName))
//                    {
//                        Debug.LogError("Split.csv not ABName nRow=" + nRow);
//                        continue;
//                    }

//                    szName = szName.Replace("\\", "/");
//                    szName = szName.Replace(".datas", "");
//                    szName = szName.Replace(".data", "");
//                    abID = AssetBundleIDMap.GetABID("Assets/" + szName, false);
//                    if (abID == -1)
//                    {
//                        abID = AssetBundleIDMap.GetABID("Assets/Artist/" + szName, false);
//                    }
//                    if (abID == -1)
//                    {
//                        Debug.LogError("ABName Invalid ABName=" + szName);
//                        continue;
//                    }

//                    collectDep = true;
//                }
//                //int nName = csvReader.GetInt(nRow, 0, 0);

//                BuildPackageOne(srcPath, outPath, abID, collectDep, ref splitList);
//            }

//            return splitList;
//        }
//#else
//    private static List<int> BuildPackgeOld(string srcPath, string outPath, XGame.CsvReader.ScpReader csvReader, out BinParser abBin)
//    {
//        File.Copy(srcPath + ResourceConfigManager.ABDataFileName, outPath + ResourceConfigManager.ABDataFileName, true);
//        File.Copy(srcPath + ResourceConfigManager.AssetBundleListFileName, outPath + ResourceConfigManager.AssetBundleListFileName, true);
//        abBin = new BinParser(outPath + ResourceConfigManager.AssetBundleListFileName);

//        List<int> splitList = new List<int>();
//        for (int nRow = 0; nRow < csvReader.GetRecordCount(); nRow++)
//        {
//            int nName = csvReader.GetInt(nRow, 0, 0);

//            if (splitList.Contains(nName))
//                continue;

//            if (!File.Exists(srcPath + nName))
//                continue;

//            string outDir = outPath + nName;
//            int index = outDir.LastIndexOf("/");

//            if (index <= 0)
//                continue;

//            outDir = outDir.Substring(0, index);

//            if (!Directory.Exists(outDir))
//            {
//                Directory.CreateDirectory(outDir);
//            }

//            File.Copy(srcPath + nName, outPath + nName, true);

//            splitList.Add(nName);
//        }

//        return splitList;
//    }
//#endif

//        private static void BuildPackge(string[] _args)
//        {
////            string csvPath = _args[0];
////            string srcPath = _args[1];
////            string outPath = _args[2];

////            csvPath = csvPath.Replace("\\", "/");
////            srcPath = srcPath.Replace("\\", "/");
////            outPath = outPath.Replace("\\", "/");
////            Debug.LogFormat("【【【分包配置路径：\n  srcPath:{0}  \n  outPath:{1}  \n  csvPath:{2}", srcPath, outPath, csvPath);
////            if (string.IsNullOrEmpty(srcPath) || string.IsNullOrEmpty(outPath))
////                return;

////            if (!Directory.Exists(srcPath))
////                return;

////            if (!File.Exists(csvPath))
////                return;

////            if (!Directory.Exists(outPath))
////                Directory.CreateDirectory(outPath);

////            if (!srcPath.EndsWith("/"))
////                srcPath += "/";

////            if (!outPath.EndsWith("/"))
////                outPath += "/";

////            ScpReader reader = new ScpReader(csvPath, 1);
////            if (reader == null)
////                return;

////            BinParser abBin;
////#if USE_ASSET_EDITOR
////            List<int> splitList = BuildPackgeNew(srcPath, outPath, reader, out abBin);
////#else
////        List<int> splitList = BuildPackgeOld(srcPath, outPath, reader, out abBin);
////#endif
////            abBin.ReadBin();
////            List<BinDatabase> modifyList = new List<BinDatabase>();
////            Dictionary<int, BinDatabase> abMap = abBin.GetAllData();
////            Debug.LogFormat("【【分包数据 splitList count:{0}  mapCount:{1}", splitList.Count, abMap.Count);
////            foreach (BinDatabase item in abMap.Values)
////            {
////                AssetBundleListInfo info = (AssetBundleListInfo)item;
////                if (info == null)
////                    continue;

////                if (!splitList.Contains(info.szName))
////                {
////                    info.nLocal = 0;
////                }
////                else
////                {
////                    info.nLocal = 1;
////                }
////                modifyList.Add(info);
////            }

////            foreach (BinDatabase item in modifyList)
////            {
////                abBin.AddData(item);
////            }

////            abBin.WriteBin();
////            abBin.Dispose();

////            splitList.Clear();
////            modifyList.Clear();

////            Debug.Log("Subpackage success!");
////        }

////        private static void BuildFullPackge(string[] _args)
////        {
////            string srcPath = _args[0];
////            srcPath = srcPath.Replace("\\", "/");

////            if (string.IsNullOrEmpty(srcPath))
////                return;

////            if (!Directory.Exists(srcPath))
////                return;

////            if (!srcPath.EndsWith("/"))
////                srcPath += "/";

////            BinParser abBin = new BinParser(srcPath + ResourceConfigManager.AssetBundleListFileName);
////            Debug.Log("【【分包Bin读取】】Path: " + srcPath + ResourceConfigManager.AssetBundleListFileName);
////            abBin.ReadBin();

////            List<AssetBundleListInfo> modifyList = new List<AssetBundleListInfo>();
////            foreach (BinDatabase item in abBin.GetAllData().Values)
////            {
////                if (item == null)
////                    continue;

////                AssetBundleListInfo info = (AssetBundleListInfo)item;
////                if (info == null)
////                    continue;

////                info.nLocal = 1;
////                modifyList.Add(info);
////                Debug.Log("【【分包添加】】info: " + info.szName);
////            }

////            foreach (AssetBundleListInfo item in modifyList)
////            {
////                abBin.AddData(item);
////            }

////            modifyList.Clear();
////            abBin.WriteBin();
////            abBin.Dispose();

////            Debug.Log("BuildFullPackge success!");
////        }

////        private static void CopyPackge(string[] _args)
////        {
////            string srcPath = _args[0];
////            string outPath = _args[1];

////            srcPath = srcPath.Replace("\\", "/");
////            outPath = outPath.Replace("\\", "/");

////            if (string.IsNullOrEmpty(srcPath) || string.IsNullOrEmpty(outPath))
////                return;

////            if (!Directory.Exists(srcPath))
////                return;

////            if (!Directory.Exists(outPath))
////                Directory.CreateDirectory(outPath);

////            if (!srcPath.EndsWith("/"))
////                srcPath += "/";

////            if (!outPath.EndsWith("/"))
////                outPath += "/";

////            DirectoryInfo dir = new DirectoryInfo(srcPath);
////            FileInfo[] assetArry = dir.GetFiles("*", SearchOption.AllDirectories);

////            foreach (FileInfo item in assetArry)
////            {
////                int num;
////                if (!int.TryParse(item.Name, out num))
////                    continue;

////                string name = item.FullName.Replace("\\", "/");
////                name = name.Replace(srcPath, "");

////                File.Copy(srcPath + name, outPath + name, true);
////            }

////            BinParser abBin = new BinParser(outPath + ResourceConfigManager.AssetBundleListFileName);
////            abBin.ReadBin();

////            Dictionary<int, BinDatabase> tempDic = abBin.GetAllData();
////            List<AssetBundleListInfo> tempList = new List<AssetBundleListInfo>();

////            foreach (BinDatabase item in tempDic.Values)
////            {
////                AssetBundleListInfo info = (AssetBundleListInfo)item;
////                if (info != null)
////                {
////                    info.nLocal = 1;
////                    tempList.Add(info);
////                }
////            }

////            foreach (AssetBundleListInfo item in tempList)
////            {
////                abBin.AddData(item);
////            }

////            abBin.WriteBin();
////            abBin.Dispose();
//        }

//        private static string GetArg(string _name)
//        {
//            var args = System.Environment.GetCommandLineArgs();
//            for (int i = 0; i < args.Length; i++)
//            {
//                if (args[i] == _name && args.Length > i + 1)
//                {
//                    return args[i + 1];
//                }
//            }
//            return null;
//        }
//    }
//}
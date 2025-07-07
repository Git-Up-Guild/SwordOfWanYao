#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace XGameEditor.Build.iOS
{
    public class XCodeProjectBuilder : Editor
    {
        [PostProcessBuild(998)]
        public static void OnBuildCallback(BuildTarget buildTarget, string path)
        {
            //过滤平台
            if (buildTarget != BuildTarget.iOS)
                return;

            //读取文件
            string projectFilePath = $"{path}/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectFilePath);

            //获取Guid
            var frameWorkTargetGuid = project.GetUnityFrameworkTargetGuid();
            var mainTargetGuid = project.GetUnityMainTargetGuid();

            //设置属性
            project.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO");
            project.SetBuildProperty(mainTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

            project.SetBuildProperty(frameWorkTargetGuid, "ENABLE_BITCODE", "NO");
            project.SetBuildProperty(frameWorkTargetGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", "YES");

            project.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", "VKVPM45NB3");
            project.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE", "217c1d74-e9ed-4e93-9f81-a87cddace684");
            project.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", "civre_dev002");

            project.SetBuildProperty(mainTargetGuid, "IPHONEOS_DEPLOYMENT_TARGET", "13.0");
            project.SetBuildProperty(mainTargetGuid, "PRODUCT_BUNDLE_IDENTIFIER", "com.hyl.civre");
            project.SetBuildProperty(mainTargetGuid, "PRODUCT_NAME", "Valor Clash");


            string strDefinetions = project.GetBuildPropertyForAnyConfig(mainTargetGuid, "GCC_PREPROCESSOR_DEFINITIONS");
            strDefinetions += " DISABLE_PUSH_NOTIFICATIONS=1";
            project.SetBuildProperty(mainTargetGuid, "GCC_PREPROCESSOR_DEFINITIONS", strDefinetions);

            //添加功能
            AddCapabilities(projectFilePath);

            //保存
            project.WriteToFile(projectFilePath);

            //修改Info.pList
            ModifiedPlistProperties(path);

 
        }

        public static void ModifiedPlistProperties(string projectDir)
        {
            string plistPath = projectDir + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);


            PlistElement pe = null;
            if (rootDict.values.TryGetValue("NSAppTransportSecurity", out pe))
            {
                PlistElementDict pd = pe.AsDict();
                if (pd != null)
                {
                    //pd.values.Remove("NSAllowsArbitraryLoadsInWebContent");
                    pd.SetBoolean("NSAllowsArbitraryLoads", true);
                    pd.values.Remove("NSAllowsArbitraryLoadsInWebContent");
                }


            }

            plist.WriteToFile(plistPath);
        }



        public static void ModifiedPodfileProperties(string projectDir)
        {
            string path = projectDir + "/podfile";
            if (!File.Exists(path))
                return;

            string conent = File.ReadAllText(path);
            File.Delete(path);

            conent += @"

post_install do |installer|
  installer.aggregate_targets.each do |target|
    target.xcconfigs.each do |variant, xcconfig|
      xcconfig_path = target.client_root + target.xcconfig_relative_path(variant)
      IO.write(xcconfig_path, IO.read(xcconfig_path).gsub('DT_TOOLCHAIN_DIR', 'TOOLCHAIN_DIR'))
    end
  end
  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      if config.base_configuration_reference.is_a? Xcodeproj::Project::Object::PBXFileReference
        xcconfig_path = config.base_configuration_reference.real_path
        IO.write(xcconfig_path, IO.read(xcconfig_path).gsub('DT_TOOLCHAIN_DIR', 'TOOLCHAIN_DIR'))
      end
    end
  end
end
";

            File.WriteAllText(path, conent);


        }

        private static void AddCapabilities(string projectPath)
        {
            ProjectCapabilityManager capabilityManager = 
                new ProjectCapabilityManager(projectPath, "Unity-iPhone/Unity-iPhone.entitlements", "Unity-iPhone");

            capabilityManager.AddAccessWiFiInformation();
            capabilityManager.AddSignInWithApple();
            capabilityManager.AddInAppPurchase();

            capabilityManager.WriteToFile();
        }
        
    }
}

#endif
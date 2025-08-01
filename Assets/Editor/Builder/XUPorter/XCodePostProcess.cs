using UnityEngine;
#if UNITY_IOS

#if UNITY_EDITOR

using XGameEditor.Build.iOS;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;

public static class XCodePostProcess
{
#if UNITY_EDITOR
	[PostProcessBuild(999)]  //最后执行
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
        if (target != BuildTarget.iOS) 
		{
			Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
			return;
		}


		//Info.plist 會被改變，這裏最後時刻再寫一次
		XCodeProjectBuilder.ModifiedPlistProperties(pathToBuiltProject);

		//修改 podfile.txt
		XCodeProjectBuilder.ModifiedPodfileProperties(pathToBuiltProject);

		/*
		//先屏蔽
		bool b = true;
		if (b)
			return;

		// Create a new project object from build target
		XCProject project = new XCProject( pathToBuiltProject );

        // Find and run through all projmods files to patch the project.
        // Please pay attention that ALL projmods files in your project folder will be excuted!

        //只處理自己的
        string[] files = Directory.GetFiles(Application.dataPath + "/Editor/XUporter/Mods", "*.projmods", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            UnityEngine.Debug.Log("ProjMod File: " + file);
            project.ApplyMod(file);
        }

		//TODO implement generic settings as a module option
		//project.overwriteBuildSetting("CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Distribution", "Release");

		// Finally save the xcode project
		project.Save();
		*/

	}
#endif

	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}
}
#endif
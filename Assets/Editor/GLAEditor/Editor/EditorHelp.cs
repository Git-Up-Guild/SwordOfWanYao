
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class EditorHelp
{
	static public BuildTarget GetPlatform ()
	{
#if UNITY_ANDROID
		return BuildTarget.Android;
#elif UNITY_IPHONE
		return BuildTarget.iOS;
#else
		return BuildTarget.StandaloneWindows;
#endif
	}

	static public string GetPlatformName (BuildTarget platform, bool isLower = false)
	{
		string path = "";

		switch (platform) {
		case BuildTarget.StandaloneWindows:
			{
				path = "Windows";
			}
			break;
		case BuildTarget.Android:
			{
				path = "Android";
			}
			break;
		case BuildTarget.iOS:
			{
				path = "IPhone";
			}
			break;
		}

		return isLower ? path.ToLower () : path;
	}

	static public string GetPlatformName ()
	{
		return GetPlatformName (GetPlatform ());
	}

    public static string GetPath(GameObject go)
    {
        Transform goTrans = go.transform;
        string result = goTrans.name;
        while (goTrans.parent != null)
        {
            goTrans = goTrans.parent;
            result = string.Format("{0}/{1}", goTrans.name, result);
        }

        return result;
    }

	static public string GetAssetPath (string cat, string assetName)
	{
		var platformT = GetPlatform ();
		var platform = GetPlatformName (platformT);
		string path = string.Format ("Assets/StreamingAssets/{0}/{2}/{1}.unity3d_{3}", platform, assetName, cat, GetPlatformName (platformT, true));
		return path;
	}

	public static void ExplorePath (string path)
	{
		Debug.Log (path);
		path = path.Replace ("/", "\\");
		System.Diagnostics.Process.Start ("explorer.exe", string.Format (@"{0}", path));
	}

	public static void CopyFile (string source, string target, bool overwrite = true, bool log = true)
	{
		try {
			File.Copy (source, target, overwrite);
			if (log) {
				Debug.LogFormat ("Moved:\n{0},\n{1}", source, target);
			}
		} catch (System.Exception e) {
			Debug.LogError (e.ToString ());
		}
	}

	public static void CheckDir (string path)
	{

		var dir = Path.GetDirectoryName (path);
		if (!Directory.Exists (dir)) {
			Directory.CreateDirectory (dir);
		}
	}

	public static void IterPrefab (System.Func<GameObject, bool> One_GO_Modify, UnityEngine.Object[] objs, string title = "title")
	{
		float proccess = 0;
		EditorUtility.DisplayProgressBar (title, "begin..", proccess);
		GameObject selgoInScene = null;
		for (int i = 0; i < objs.Length; i++) {
			try {
				GameObject selgo = objs [i] as GameObject;
				proccess = i / (float)objs.Length;
				EditorUtility.DisplayProgressBar (title, string.Format ("{1}%...{0}", selgo.name, (proccess * 100).ToString ("F2")), proccess);
				selgoInScene = PrefabUtility.InstantiatePrefab (objs [i]) as GameObject;
				if (One_GO_Modify != null) {
					var bdirty = One_GO_Modify (selgoInScene);
					if (bdirty) {
						if (PrefabUtility.GetPrefabType (selgoInScene) == PrefabType.PrefabInstance) {
							UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent (selgoInScene);
							//替换预设  
							PrefabUtility.ReplacePrefab (selgoInScene, parentObject, ReplacePrefabOptions.ConnectToPrefab);
							GameObject.DestroyImmediate (selgoInScene);
							//刷新  
							AssetDatabase.Refresh ();
							Debug.Log ("Modify:" + selgo.name);
						}
					}
				}
				GameObject.DestroyImmediate (selgoInScene);
			} catch (System.Exception ex) {
				Debug.LogError (ex.ToString ());
				if (selgoInScene) {
					GameObject.DestroyImmediate (selgoInScene);
				}
			}
		}
		EditorUtility.ClearProgressBar ();
	}

    /// <summary>
    /// 驼峰命名方式转下划线
    /// </summary>
    /// <param name="camel"></param>
    /// <returns></returns>
    static public string CamelToLowerLine(string str)
    {
        string Pattern = @"[A-Z]";
        MatchCollection s = Regex.Matches(str, Pattern);
        foreach (Match m in s)
        {
            str = str.Replace(m.Value, "_" + m.Value.ToLower());
        }

        str = str.TrimStart('_');
        return str;
    }

    /// <summary>
    /// 激活所有的子对象
    /// </summary>
    /// <param name="p"></param>
    public static void ActiveAllChildren(Transform p, List<Transform> lsActiveChildren)
    {
        if (p.gameObject.activeSelf == false)
        {
            p.gameObject.BetterSetActive(true);
            lsActiveChildren.Add(p);
        }

        for (int i = 0; i < p.childCount; ++i)
        {
            ActiveAllChildren(p.GetChild(i), lsActiveChildren);
        }
    }

    /// <summary>
    /// Applies the un read tex. 贴图像素拷贝
    /// </summary>
    /// <param name="tex">Tex.</param>
    /// <param name="target">Target.</param>
    public static void ApplyUnReadTex (Texture2D tex, Texture2D target)
	{ 
		Material mat = new Material (Shader.Find ("Unlit/Transparent"));
		RenderTexture rt = RenderTexture.GetTemporary (target.width, target.height, 0, RenderTextureFormat.ARGB32);
		Graphics.SetRenderTarget (rt);
		GL.Clear (true, true, Color.clear);
		GL.PushMatrix ();
		GL.LoadOrtho (); 
		mat.mainTexture = tex;
		mat.SetPass (0);
		GL.Begin (GL.TRIANGLES);
		var tris = new int[] {
			0, 1, 2, 0, 2, 3
		};
		var uvs = new Vector3[] {
			new Vector3 (0, 0),
			new Vector3 (0, 1),
			new Vector3 (1, 1),
			new Vector3 (1, 0), 
		};
		var atlasUvs = new Vector3[] {
			new Vector3 (0, 0),
			new Vector3 (0, 1),
			new Vector3 (1, 1),
			new Vector3 (1, 0),
		};
		foreach (int index in tris) {
			GL.TexCoord (uvs [index]);
			GL.Vertex (atlasUvs [index]);
		}
		GL.End (); 
		GL.PopMatrix ();

		target.ReadPixels (new Rect (0, 0, target.width, target.height), 0, 0);
		target.Apply ();
		RenderTexture.ReleaseTemporary (rt);
	}
	public static void ApplyUnReadTex2 (Texture2D tex, Texture2D target)
	{ 
		var offset = new int[]{ (target.width - tex.width) / 2, (target.height - tex.height) / 2 };

		var offset0 = offset [0];
		var offset1 = offset [1];


		var targetWidth = target.width;
		var targetHeight = target.height;  
		// center
		target.SetPixels (offset0, offset1, tex.width, tex.height, tex.GetPixels ());

		Color color;
		//leftbot
		color = tex.GetPixel (0, 0); 
		for (int i = 0; i < offset0; i++) {
			for (int j = 0; j < offset1; j++) {
				target.SetPixel (i, j, color);
			}
		}
		//rightbot
		color = tex.GetPixel (tex.width - 1, 0); 
		for (int i = tex.width + offset0; i < targetWidth; i++) {
			for (int j = 0; j < offset1; j++) {
				target.SetPixel (i, j, color);
			}
		}

		//lefttop
		color = tex.GetPixel (0, tex.height - 1); 
		for (int i = 0; i < offset0; i++) {
			for (int j = tex.height + offset1; j < target.height; j++) {
				target.SetPixel (i, j, color);
			}
		}

		//righttop
		color = tex.GetPixel (tex.width - 1, tex.height - 1); 
		for (int i = tex.width; i < targetWidth; i++) {
			for (int j = tex.height + offset1; j < target.height; j++) {
				target.SetPixel (i, j, color);
			}
		}

		//leftcenter
		color = tex.GetPixel (0, 0);
		for (int j = offset1; j < targetHeight - offset1; j++) {
			color = tex.GetPixel (0, j - offset1); 
			for (int i = 0; i < offset0; i++) {
				target.SetPixel (i, j, color);
			}
		}

		//rightcenter
		color = tex.GetPixel (0, 0);
		for (int j = offset1; j < targetHeight - offset1; j++) {
			color = tex.GetPixel (tex.width - 1, j - offset1); 
			for (int i = targetWidth - offset0; i < targetWidth; i++) {
				target.SetPixel (i, j, color);
			}
		}


		//topcenter
		color = tex.GetPixel (0, 0);
		for (int i = offset0; i < targetWidth - offset0; i++) {
			color = tex.GetPixel (i - offset0, tex.height - 1); 
			for (int j = targetHeight - offset1; j < targetHeight; j++) {
				target.SetPixel (i, j, color);
			}
		}

		//botcenter
		color = tex.GetPixel (0, 0);
		for (int i = offset0; i < targetWidth - offset0; i++) {
			color = tex.GetPixel (i - offset0, 0); 
			for (int j = 0; j < offset1; j++) {
				target.SetPixel (i, j, color);
			}
		} 
	}
	//public static string ToJson (object o)
	//{
 //       return Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented);
 //   }

 //   public static T ToObj<T>(string o)
 //   {
 //       return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(o);
 //   }
}
﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class UGIFontEditorHelper : Editor 
{
	[MenuItem("Assets/XGame/转换成艺术字体")]
	static public void BatchCreateArtistFont()
	{
		ArtistFont.BatchCreateArtistFont();
	}
}

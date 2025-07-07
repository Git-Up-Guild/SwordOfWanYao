using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ReferenceFinder
{
    [Serializable]
    public enum EAssetState
    {
        NORMAL,
        CHANGED,
        MISSING,
        NODATA,
    }

    public class AssetDescription
    {
        public string name = string.Empty;
        public string path = string.Empty;
        public string extension = string.Empty;
        public string assetDependencyHash;
        public List<string> dependencies = new List<string>();
        public List<string> references = new List<string>();
        public EAssetState state = EAssetState.NORMAL;

        public AssetDescription(string path)
        {
            name = Path.GetFileNameWithoutExtension(path);
            this.path = path;
            extension = Path.GetExtension(path);
        }
    }
}
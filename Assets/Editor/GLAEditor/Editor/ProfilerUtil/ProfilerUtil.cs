using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace XGameEditor.GlacierEditor.ProfilerUtil
{
    public static class ProfilerUtil
    {
        //return bytes , Use EditorUtility.FormatBytes For Format
        public static int GetRuntimeMemorySize(Object obj)
        {
            return Profiler.GetRuntimeMemorySize(obj);
        }

        //return bytes , Use EditorUtility.FormatBytes For Format
        public static int GetMemorySizeInDisk(Texture tex)
        {
            var type = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
            MethodInfo methodInfo = type.GetMethod("GetStorageMemorySize", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            return (int)methodInfo.Invoke(null, new object[] { tex });
        }
    }
}
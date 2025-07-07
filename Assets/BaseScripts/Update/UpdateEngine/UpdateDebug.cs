using UnityEngine;

namespace XGame.Update
{
    public class UpdateDebug
    {
        public static void Log(string msg)
        {
            Debug.Log("[Update]" + msg);
        }

        public static void LogWarning(string msg)
        {
            Debug.LogWarning("[Update]" + msg);
        }

        public static void LogError(string msg)
        {
            Debug.LogError("[Update]" + msg);
        }
    }
}

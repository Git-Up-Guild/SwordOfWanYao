using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Common
{
    public class GameZone
    {
        /// 获得当前游戏世界ID
        private static int gThisGameWorldID = 0;

        /// 本进程是否为公共区
        private static bool gPublicGameWorld = false;

        /// 获得当前游戏世界Name
        private static string gThisGameWorldName = "";

        /// 获得当前公共区游戏世界ID
        private static int gPublicGameWorldID = 0;

        public static bool isPublicGameWorld()
        { return gPublicGameWorld; }

        /// 设置本进程为公共区
        public static void setPublicGameWorld()
        { gPublicGameWorld = true; }

        // 设置本进程为普通区
        public static void setGeneralGameWorld()
        { gPublicGameWorld = false; }

        public static int getThisGameWorldID()
        { return gThisGameWorldID; }
        /// 设置当前游戏世界ID
        public static void setThisGameWorldID(int nGameWorldID, bool bAgreeReset)
        {
            if (gThisGameWorldID > 0 && nGameWorldID != gThisGameWorldID && !bAgreeReset)
            {
                XGame.Trace.TRACE.ErrorLn("setThisGameWorldID, gameworldid not support reset, old=" + gThisGameWorldID + " new=" + nGameWorldID);
                return;
            }

            gThisGameWorldID = nGameWorldID;
        }
        
        public static string getThisGameWorldName()
        {
            return gThisGameWorldName;
        }

        /// 设置当前游戏世界ID
        public static void setThisGameWorldName(string szGameWorldName, bool bAgreeReset)
        {
            if (!string.IsNullOrEmpty(szGameWorldName) && !gThisGameWorldName.Equals(szGameWorldName) && !bAgreeReset)
            {
                XGame.Trace.TRACE.ErrorLn("setThisGameWorldName, gameworldid not support reset, old=" + gThisGameWorldName + " new=" + szGameWorldName);
                return;
            }

            gThisGameWorldName = szGameWorldName;
        }

        
        public static int getPublicGameWorldID()
        {
            return gPublicGameWorldID;
        }

        public static string GetPublicWorldShowName()
        {
            return "跨服状态";
        }

        /// 设置公共区游戏世界ID
        public static void setPublicGameWorldID(int nGameWorldID, bool bAgreeReset)
        {
            if (gPublicGameWorldID > 0 && nGameWorldID != gPublicGameWorldID && !bAgreeReset)
            {
                XGame.Trace.TRACE.ErrorLn("setPublicGameWorldID, id not support reset, old=" + gPublicGameWorldID + " new=" + nGameWorldID);
                return;
            }

            gPublicGameWorldID = nGameWorldID;
        }

    }
}

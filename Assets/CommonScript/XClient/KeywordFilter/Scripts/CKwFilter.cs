using keywordfilter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XGame.Asset;
using XGame.UnityObjPool;

namespace XGame.KwFilter
{

    public class CKwFilter
    {
        //配置脚本路径
        public static readonly string SCP_PATH = "G_Resources/Game/KeyworldFilter/KeyworldFilter.bytes";

        //屏蔽字过滤器实例
        private static IntPtr m_kwFilterInst = IntPtr.Zero;

        public static void BuildFilterInstance()
        {
            ReleaseFilterInstance();

            IUnityObjectPool unityObjectPool = XGameComs.Get<IUnityObjectPool>();
            if (unityObjectPool == null)
            {
                Debug.LogError(string.Format("加载战斗配置脚本失败，unityObjectPool is null."));
                return;
            }

            uint handle;
            TextAsset kwfilterConfig = unityObjectPool.LoadRes<TextAsset>(SCP_PATH, out handle);          

            if (kwfilterConfig == null)
            {
                Debug.LogError("Can't loaded KeyworldFilter.bytes.");
                return;
            }
            m_kwFilterInst = kwFilterApi.BuildFilterInstance(kwfilterConfig.bytes, kwfilterConfig.bytes.Length);

            unityObjectPool.UnloadRes(handle, false, false, false, false);
        }

        public static void ReleaseFilterInstance()
        {            
            //销毁屏蔽字过滤器实例
            if (m_kwFilterInst != IntPtr.Zero)
            {
                kwFilterApi.ReleaseFilterInstance(m_kwFilterInst);
                m_kwFilterInst = IntPtr.Zero;
            }
        }

        //  检测字符串是否非法. 
        public static bool IsStringLegal(byte[] pUtf8String, int iStrLen)
        {
            if (m_kwFilterInst != IntPtr.Zero)
            {
                bool result = kwFilterApi.IsStringLegal(m_kwFilterInst, pUtf8String, iStrLen);
                return result;
            }
            return false;
        }

        //  获取最后一次违规信息. 需要在 IsStringLegal() 返回false后使用
        public static bool GetLastIllegedInfo(out int iStartPos, out int iLen)
        {
            iStartPos = 0;
            iLen = 0;
            if (m_kwFilterInst != IntPtr.Zero)
            {
                return kwFilterApi.GetLastIllegedInfo(m_kwFilterInst, out iStartPos, out iLen);
            }
            return false;
        }

        //  获取替换过后的字符串
        public static string GetAfterReplaceString(string pUtf8String, int startPos, int iLen)
        {            
            char[] arrInput = pUtf8String.ToCharArray();
            for (int i = 0; i < iLen; i++)
            {
                arrInput[startPos + i] = '*';               
            }
            return new string(arrInput);
        }
    }
}

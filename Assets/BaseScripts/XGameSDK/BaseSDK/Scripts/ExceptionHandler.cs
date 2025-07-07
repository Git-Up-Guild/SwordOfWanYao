/*******************************************************************
** 文件名:	ExceptionHandler.cs
** 版  权:	(C) 冰川网络网络科技有限公司
** 创建人:	许德纪
** 日  期:	2022.11.16
** 版  本:	1.0
** 描  述:	
** 应  用:  SDK

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XGame.AssetScript.SDK.Base
{

    public class ExceptionHandler
    {
#if UNITY_IOS

        [DllImport("__Internal")]
        private static extern void iosBaseSDK_SetupExceptionHandler();
#endif

        public static void SetupExceptionHandler()
        {
#if UNITY_IPHONE
            iosBaseSDK_SetupExceptionHandler();
#endif
        }
    }
}

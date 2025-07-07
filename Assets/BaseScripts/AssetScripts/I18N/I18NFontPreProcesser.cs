/*******************************************************************
** 文件名:	I18NFontPreProcesser.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程 
** 日  期:	2019/10/30
** 版  本:	1.0
** 描  述:	国际化字体预处理
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using XClient.Game;
using XGame.I18N;

namespace XGame.AssetScript.I18N
{
    [RequireComponent(typeof(I18NFontManager))]
    public class I18NFontPreProcesser : GamePreProcesser
    {
        private I18NFontManager m_FontManager;

        [SerializeField]
        private bool m_IsDevMode = true;

        private void OnEnable()
        {
        }

        private void Awake()
        {
            m_FontManager = GetComponent<I18NFontManager>();
        }

        public override void Execute()
        {
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS &&  !UNITY_IPHONE
            if(m_IsDevMode)
            {
                isFinished = true;
            }
            else
            {
                DOFontPreProcess();
            }
#else
            DOFontPreProcess();
#endif
        }

        private void DOFontPreProcess()
        {
            m_FontManager.PreProcess((bRet) =>
            {
                isFinished = true;
                if (!bRet)
                {
                    Debug.LogError($"[I18N] I18NFontPreProcesser fail. ");
                }
            });
        }
    }
}

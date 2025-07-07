/*******************************************************************
** 文件名:	InputEventPoint.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	吴易璋
** 日  期:	2019/11/22 15:00:21
** 版  本:	1.0
** 描  述:	鼠标点击光效
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using XGame.ScreenTouchEffect;
using XGame.TouchInput;
using UnityEngine;
using XGame.UI;
using Game;
using XGame;
using XGame.UI.Framework;

namespace XClient.Game
{
    public class InputEventPoint : IInputEventHandler
    {
        /// <summary>
        /// 输入管理器
        /// </summary>
        private IInputManager manager;

        public bool Create(object context = null)
        {
            manager = context as IInputManager;
            manager.RegEventHandler(InputEventType.TypePointDynamic, this);
            return true;
        }

        public bool OnInputEventUpdate(InputEvent inputevent)
        {
            if (inputevent.m_EventMode == InputEventType.TypePointDynamic)
            {
                IScreenTouchEffectCom screenTouchEffectCom = XGame.XGameComs.Get<IScreenTouchEffectCom>();
                if(screenTouchEffectCom != null)
                { 
                    Vector2 vPos;

                    var settings = XGameComs.Get<IUIFramework>().Settings;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(settings.CameraUIContainer, Input.mousePosition,
                       settings.CameraUIContainer.GetComponentInParent<Canvas>().worldCamera, out vPos);
                    screenTouchEffectCom.Add(vPos.x, vPos.y);
                }
            }

            return true;
        }

        public void Release()
        {
            manager.UnRegEventHandler(InputEventType.TypePointDynamic);
        }
    }
}

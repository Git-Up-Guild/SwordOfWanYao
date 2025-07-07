/*******************************************************************
** 文件名:	InputEventCamera.CS
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	吴易璋
** 日  期:	2019/11/21 21:12:29
** 版  本:	1.0
** 描  述:	
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using XGame;
using XGame.Def;
using XGame.TouchInput;
using XClient.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XClient.Game
{
    public class InputEventCamera : IInputEventHandler
    {
        IInputManager manager;
        public bool Create(object context = null)
        {
            manager = context as IInputManager;
            return true;
        }

        public bool OnInputEventUpdate(InputEvent inputevent)
        {
            switch (inputevent.m_EventMode)
            {
                case InputEventType.TypeRotateCamera:
                    break;
                case InputEventType.TypeClick:
                    break;
                case InputEventType.TypeZoomCamera:
                    break;
                default:
                    break;
            }
            return true;
        }

        public void Release()
        {
        }

        private void OnHandleDoubleFinger(InputEvent inputevent)
        {
        }

        private void SendZoomCMD(InputEvent inputevent)
        {
        }
    }
}
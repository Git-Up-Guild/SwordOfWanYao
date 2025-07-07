/*******************************************************************
** 文件名:	VisibleComponent.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.23
** 版  本:	1.0
** 描  述:	
** 应  用:  控制显示的部件

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XGame.Entity
{
    public class VisibleComponent : MonoBehaviour
    {

        //显示的根部
        public GameObject visibleRoot;

        //显示
        public virtual void EnableRenderer()
        {
            if (null != visibleRoot && visibleRoot.activeSelf == false)
            {
                visibleRoot.gameObject.BetterSetActive(true);
            }
        }

        //隐藏
        public virtual void DisableRenderer()
        {
            if (null != visibleRoot && visibleRoot.activeSelf)
            {
                visibleRoot.gameObject.BetterSetActive(false);
            }
        }

        public virtual void Clear()
        {

        }

    }

}

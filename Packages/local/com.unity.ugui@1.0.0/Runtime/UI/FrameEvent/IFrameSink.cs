/*******************************************************************
** 文件名:	IFrameSink.cs
** 版  权:	(C) 
** 创建人:	许德纪
** 日  期:	2025/3/28
** 版  本:	1.0
** 描  述:	
** 应  用:  简单的帧回调

********************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    public interface IFrameSink
    {
        //返回false后自动退订
         bool OnUpdate();
    }
}



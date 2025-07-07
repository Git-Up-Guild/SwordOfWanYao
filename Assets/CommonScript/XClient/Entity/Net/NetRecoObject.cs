/*******************************************************************
** 文件名:	NetRecoObject.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.03
** 版  本:	1.0
** 描  述:	
** 应  用:  网络轻量级对象,预制体挂上去之后，可以到其他客户端通过netID找到

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static XClient.Entity.ForwardMovement;
using static XClient.Network.NetRecoObject;

namespace XClient.Network
{
    public class NetRecoObject : NetObjectBehaviour<NetRecoObjectData>
    {

        public class NetRecoObjectData : MonoNetObject
        {
          

            protected override void OnSetupVars()
            {
               
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


/*******************************************************************
** 文件名:	EnvProviderMgr.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2025.5.12
** 版  本:	1.0
** 描  述:	
** 应  用:  所有环境提供者管理器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;
using XGame.Utils;

namespace XGame.Entity
{

    public class EnvProviderMgr : Singleton<EnvProviderMgr>
    {

        //子弹提供器
        private IBulletEnvProvider m_bulletEnvProvider;


        //获取子弹提供器
        public IBulletEnvProvider GetBulletEnvProvider()
        {
            return m_bulletEnvProvider;   
        }

        //设置子弹的环境提供器
        public void SetBulletEnvProvider(IBulletEnvProvider bulletEnvProvider)
        {
            m_bulletEnvProvider = bulletEnvProvider;
        }

    }

}


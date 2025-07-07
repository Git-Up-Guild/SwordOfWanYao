/*******************************************************************
** 文件名:	NetResourceEntity.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2024/5/21 15:35:30
** 版  本:	1.0
** 描  述:	
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using XClient.Network;
using XGame.Entity;

namespace XClient.Entity.Net
{
    /// <summary>
    /// 网络资源实体是这样一种实体，通知所有的客户端都加载指定的资源
    /// 可以在资源上添加上一个网络对象组件，修改网络对象组件，网络对象的属性也会同步给所有客户端
    /// </summary>
    public class NetGameObjectEntity : NetEntity
    {
        private string m_ResPath;

        protected override void OnInit(object context)
        {
            base.OnInit(context);

            var ctx = (context as NetEntityShareInitContext);
            if(ctx.isInitFromNet)
                m_ResPath = ctx.netInitContext.resPath;
            else
                m_ResPath = ctx.localInitContext as string;
        }

        public override string GetResPath()
        {
            return m_ResPath;
        }

    }
}

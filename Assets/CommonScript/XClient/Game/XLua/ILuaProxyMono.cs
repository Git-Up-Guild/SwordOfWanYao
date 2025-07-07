/*******************************************************************
** 文件名:	ILuaProxyMono
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	郑秀程
** 日  期:	2021/1/13
** 版  本:	1.0
** 描  述:	Lua代理Mono
** 应  用:  
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XClient.Game.Lua
{
    public class ILuaProxyMono : MonoBehaviour
    {
        /// <summary>
        /// 只负责创建，创建之后这个对象就没有用了。
        /// 之所以这么弄是因为XLUA在Asset资源目录中被创建
        /// </summary>
        /// <returns></returns>
        public virtual ILua CreateLua() { return null; }
    }
}

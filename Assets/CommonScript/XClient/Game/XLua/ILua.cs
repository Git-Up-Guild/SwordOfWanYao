/*******************************************************************
** 文件名:	IXLua
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	郑秀程
** 日  期:	2021/1/13
** 版  本:	1.0
** 描  述:	XLua接口
** 应  用:  
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XClient.Game.Lua
{
    /// <summary>
    /// XLua管理接口
    /// </summary>
    public interface ILua
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();

        /// <summary>
        /// 开始工作
        /// </summary>
        void Work();

        /// <summary>
        /// 将要销毁
        /// </summary>
        void WillDispose();

        /// <summary>
        /// 销毁
        /// </summary>
        void Dispose();

        /// <summary>
        /// 失去焦点回调
        /// </summary>
        /// <param name="focus"></param>
        void OnApplicationFocus(bool focus);
    }
}

/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: IReddotModule.cs 
Module: Reddot
Author: 郑秀程
Date: 2024.06.19
Description: 红点模块
***************************************************************************/

using XClient.Common;
using XGame.Reddot;

namespace XClient.Reddot
{
    /// <summary>
    /// 红点模块接口
    /// </summary>
    public interface IReddotModule : IModule
    {
        IReddotManager Manager { get; }
    }
}
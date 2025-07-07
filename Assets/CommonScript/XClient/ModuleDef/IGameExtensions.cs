/*******************************************************************
** 文件名:	IExternalModulesSetup.cs
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

namespace XClient.Common
{
    /// <summary>
    /// 游戏扩展
    /// </summary>
    public interface IGameExtensions
    {
        /// <summary>
        /// 设置扩展模块
        /// </summary>
        /// <param name="modules"></param>
        void OnSetupExtenModules(IModule[] modules);

        /// <summary>
        /// 模块创建完成回调
        /// </summary>
        void OnAfterSetupExtenModules();
    }
}

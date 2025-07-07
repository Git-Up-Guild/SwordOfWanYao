/*******************************************************************
** 文件名:	DAudioGroupType.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2020/1/18
** 版  本:	1.0
** 描  述:	音频类型枚举
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

namespace XClient.Common
{
    /// <summary>
    /// 声音分组。不同类型的声音可以同属一组
    /// </summary>
    public enum EAudioGroupType
    {
        /// <summary>
        /// 主控制器
        /// </summary>
        Master = 0,

        /// <summary>
        /// 背景音乐
        /// </summary>
        SubMusic,

        /// <summary>
        /// 战斗音效
        /// </summary>
        SubBattle,

        /// <summary>
        /// UI音效
        /// </summary>
        SubUI,

        /// <summary>
        /// 系统提示
        /// </summary>
        SubSys,

        /// <summary>
        /// 生物说话
        /// </summary>
        SubTalk,

        /// <summary>
        /// 最大数量
        /// </summary>
        Max = 4,
    }
}

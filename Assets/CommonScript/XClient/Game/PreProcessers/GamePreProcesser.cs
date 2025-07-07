/*******************************************************************
** 文件名:	GamePreProcesser.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程
** 日  期:	2019/11/20 15:38:06
** 版  本:	1.0
** 描  述:	游戏预处理
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;

namespace XClient.Game
{
    /// <summary>
    /// 游戏预处理基类，这些类都是一次性的，一旦完成就会销毁
    /// </summary>
    public abstract class GamePreProcesser : MonoBehaviour
    {
        /// <summary>
        /// 是否处理完成
        /// </summary>
        public bool isFinished { get; protected set; }

        /// <summary>
        /// 处理进度
        /// </summary>
        public float progressValue { get; protected set; }

        /// <summary>
        /// 进度描述
        /// </summary>
        public string progressDesc { get; protected set; }

        /// <summary>
        /// 执行，在XGameApp初始化完成后调用，此时各种组件已经初始化完成
        /// </summary>
        abstract public void Execute();
    }
}

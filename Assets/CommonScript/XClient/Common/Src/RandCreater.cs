/*******************************************************************
** 文件名:	GameClient.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	宋文武 (sww8@163.com)
** 日  期:	2014-06-25
** 版  本:	1.0
** 描  述:	随机数生成器
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using UnityEngine;
namespace XClient.Common
{
	public class RandCreater
	{
		private ulong m_lSeed;

		/// <summary>
		/// 设置随机数种子
		/// </summary>
		/// <param name="nSeed"></param>
		public void SetSeed(ulong nSeed)
		{
			m_lSeed = nSeed;
		}

		/// <summary>
		/// 获取随机数
		/// </summary>
		/// <returns></returns>
		public ulong GetRandNum()
		{
			return ((((m_lSeed) = (m_lSeed) * 214013L + 2531011L) >> 16) & 0x7fff);
		}
	}
}

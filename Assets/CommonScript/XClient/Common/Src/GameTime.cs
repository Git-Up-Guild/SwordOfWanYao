/*******************************************************************
** 文件名:	GameClient.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	陈惠旋 (sww8@163.com)
** 日  期:	2014-06-25
** 版  本:	1.0
** 描  述:	时钟管理类
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using UnityEngine;
using XGame.Utils;

namespace XClient.Common
{
	
	public class GameTime
	{
		// C#日期时间Ticks与秒数换算关系
		public const long TICKS_TO_SECONDS = 10000000;

		private static uint m_nServerTime;
		private static float m_nServerTick;

		public static void Init()
		{
			//TimeSpan ts = System.DateTime.Now - DateTime.Parse("1970-1-1");
			m_nServerTime = 0;// (uint)ts.TotalSeconds;
			m_nServerTick = GameTime.GetTickCount();
		}

		public static void SetTime(uint serverTime)
		{
			uint oldServerTick = m_nServerTime;
			m_nServerTime = serverTime;
			m_nServerTick = GameTime.GetTickCount();
#if DEBUG_LOG
///#///#			//XGame.Trace.TRACE.TraceLn("GameTime::SetTime oldtime=" + oldServerTick + ",newTime=" + m_nServerTime);
#endif
		}

		/// <summary>
		/// 获取服务器时间，1970.1.1至今的秒数
        /// 获取比较大的时间变化时，由float 转换成 int 时，转换过程中会有误差，导致通过获取服务器描述不变，
        /// 如果需要获取服务器时间秒数时，建议使用GetServerTime() add by zjc
		/// </summary>
		/// <returns></returns>
		public static uint GetTime()
		{
			uint dwRetTime = 0;
			if (m_nServerTime > 0 && m_nServerTick > 0)
			{
				uint nTicks = (uint)(GameTime.GetTickCount() - m_nServerTick);
				if (nTicks >= 0)
				{
					dwRetTime = m_nServerTime + nTicks/1000;
				}
				else
				{
					dwRetTime = XGame.Utils.DateTimeUtil.ConvertCurZoneTimeToUtcTimeSec(System.DateTime.Now);
				}
			}
			else
			{
				dwRetTime = XGame.Utils.DateTimeUtil.ConvertCurZoneTimeToUtcTimeSec(System.DateTime.Now);
			}
			return dwRetTime;
		}

		/// <summary>
		/// 将服务器时间转化成c#国际标准日期时间
		/// </summary>
		/// <returns></returns>
		public static DateTime GetDateTime()
		{
			// 获取当前日期和时间
			//DateTime refTime = new DateTime(1970, 1, 1, 0, 0, 0);//621355968000000000L
			DateTime curdate = new DateTime(621355968000000000L + (int)GetTime() * TICKS_TO_SECONDS);
			return curdate;
		}

		/// <summary>
		/// 将服务器时间转化成c#国际标准日期时间
		/// </summary>
		/// <returns></returns>
		public static DateTime GetDateTime(int seconds)
		{
			// 获取当前日期和时间
			//DateTime refTime = new DateTime(1970, 1, 1, 0, 0, 0);//621355968000000000L
			DateTime curdate = new DateTime(621355968000000000L + seconds * TICKS_TO_SECONDS);
			return curdate;
		}

		/// <summary>
		/// 获取游戏中国服务器日期时间
		/// </summary>
		/// <returns></returns>
		public static DateTime GetGameTime()
		{
			// 获取当前日期和时间
			//DateTime refTime = new DateTime(1970, 1, 1, 8, 0, 0);//621355968000000000L + 2880000000L
			DateTime curdate = new DateTime(621356256000000000L + (int)GetTime() * TICKS_TO_SECONDS);
			return curdate;
		}

		/// <summary>
		/// 获得游戏启动以来的tick数  游戏压入后台不会暂停计数
		/// </summary>
		/// <returns></returns>
		public static int GetTickCount()
		{
			return XGame.Utils.DateTimeUtil.GetTickCount();
		}
	}
}


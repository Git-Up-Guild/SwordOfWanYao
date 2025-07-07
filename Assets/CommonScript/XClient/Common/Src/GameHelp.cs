/*******************************************************************
** 文件名:	GameHelp.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	宋文武 (sww8@163.com)
** 日  期:	2016-01-25
** 版  本:	1.0
** 描  述:	游戏帮助类
** 应  用:  
	
**************************** GetBasePropertyIcon修改记录 ******************************
** 修改人: 
** 日  期: 2016-03-28
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using XGame.EtaMonitor;

namespace XClient.Common
{
    public static class GameHelp
    {
        /// <summary>
        /// 判断两个float是否相等
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool FloatIsEqual(float f1, float f2)
        {
            if (Math.Abs(f1 - f2) < 0.0001f)
            {
                return true;
            }
            return false;
        }

        #region 几何判断

        /** 计算点集合的AABB包围盒
		@param   poly	:	多边形的点数组
		@param   nCount	:	多边形点的数量
		@param   minx	:	左下角X坐标（输出)
		@param   miny	:	左下角Y坐标（输出)
		@param   maxx	:	右上角X坐标（输出)
		@param   maxy	:	右上角Y坐标（输出)
		@return			:	计算成功返回true
		*/
        public static bool CalMaxRect(List<Vector3> poly, ref float minx, ref float miny, ref float maxx, ref float maxy)
        {
            if (null == poly)
                return false;
            int nCount = poly.Count;
            minx = poly[0].x;
            miny = poly[0].z;
            maxx = poly[0].x;
            maxy = poly[0].z;
            for (int i = 1; i < nCount; i++)
            {
                if (minx > poly[i].x)
                    minx = poly[i].x;
                if (miny > poly[i].z)
                    miny = poly[i].z;
                if (maxx < poly[i].x)
                    maxx = poly[i].x;
                if (maxy < poly[i].z)
                    maxy = poly[i].z;
            }
            return true;
        }

        /** 计算点集合的AABB包围盒
		@param   poly	:	多边形的点数组
		@param   nCount	:	多边形点的数量
		@param   minx	:	左下角X坐标（输出)
		@param   miny	:	左下角Y坐标（输出)
		@param   maxx	:	右上角X坐标（输出)
		@param   maxy	:	右上角Y坐标（输出)
		@return			:	计算成功返回true
		*/
        public static bool CalMaxRect(Vector3[] poly, ref float minx, ref float miny, ref float maxx, ref float maxy)
        {
            if (null == poly)
                return false;
            int nCount = poly.Length;
            minx = poly[0].x;
            miny = poly[0].z;
            maxx = poly[0].x;
            maxy = poly[0].z;
            for (int i = 1; i < nCount; i++)
            {
                if (minx > poly[i].x)
                    minx = poly[i].x;
                if (miny > poly[i].z)
                    miny = poly[i].z;
                if (maxx < poly[i].x)
                    maxx = poly[i].x;
                if (maxy < poly[i].z)
                    maxy = poly[i].z;
            }
            return true;
        }

        /** 判断点是否在标准矩阵内  
		@param   minx	:	左下角X坐标
		@param   miny	:	左下角Y坐标
		@param   maxx	:	右上角X坐标
		@param   maxy	:	右上角Y坐标
		@param   p		:	点坐标
		@return			:	在矩形区域内返回true
		*/
        public static bool InRect(float minx, float miny, float maxx, float maxy, Vector3 p)
        {
            if (p.x >= minx && p.x <= maxx && p.z >= miny && p.z <= maxy)
                return true;
            return false;
        }

        /// <summary>
        /// 判断点是否在多边形区域内，暂时不考虑y轴
        /// 注释：如果是每帧调用，会不会太耗性能，待优化
        /// </summary>
        /// <param name="region"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool PtInRegion(List<Vector3> region, Vector3 pt)
        {
            //XGame.Trace.TRACE.ErrorLn("call PtInRegion count=" + region.Count + ",pos="+pt);
            if (region == null || region.Count <= 2)
                return false;

            //判断边长是否和x轴和z轴平行,如果是的话可优化判断
            //配置规则是从左下角逆时针配置
            if (region.Count == 4)
            {
                if ((FloatIsEqual(region[0].z, region[1].z) && FloatIsEqual(region[2].z, region[3].z)
                    && FloatIsEqual(region[0].x, region[3].x) && FloatIsEqual(region[1].x, region[2].x))
                    || (FloatIsEqual(region[0].x, region[1].x) && FloatIsEqual(region[2].x, region[3].x)
                    && FloatIsEqual(region[0].z, region[3].z) && FloatIsEqual(region[1].z, region[2].z)))
                {

                    float fMinX = Mathf.Min(Mathf.Min(region[0].x, region[1].x), Mathf.Min(region[2].x, region[3].x));
                    float fMaxX = Mathf.Max(Mathf.Max(region[0].x, region[1].x), Mathf.Max(region[2].x, region[3].x));

                    float fMinZ = Mathf.Min(Mathf.Min(region[0].z, region[1].z), Mathf.Min(region[2].z, region[3].z));
                    float fMaxZ = Mathf.Max(Mathf.Max(region[0].z, region[1].z), Mathf.Max(region[2].z, region[3].z));


                    if (pt.x < fMinX || pt.x > fMaxX || pt.z < fMinZ || pt.z > fMaxZ)
                    {
                        //区域外
                        return false;
                    }
                    else
                    {
                        //区域内
                        return true;
                    }
                }
            }


            float fminx = 0.0f;
            float fminy = 0.0f;
            float fmaxx = 0.0f;
            float fmaxy = 0.0f;
            //计算AABB包围盒
            if (CalMaxRect(region, ref fminx, ref fminy, ref fmaxx, ref fmaxy))
            {
                //不在包围盒里面
                if (InRect(fminx, fminy, fmaxx, fmaxy, pt) == false)
                {
                    return false;
                }
            }


            bool ret = false;
            int nCount = region.Count;
            int i = 0;
            int j = nCount - 1;
            for (; i < nCount; j = i++)
            {
                if ((((region[i].z <= pt.z) && (pt.z < region[j].z)) ||
                    ((region[j].z <= pt.z) && (pt.z < region[i].z)))
                    && (pt.x < (region[j].x - region[i].x) * (pt.z - region[i].z) / (region[j].z - region[i].z) + region[i].x))
                {
                    ret = !ret;
                }
            }
            return ret;
        }

        /// <summary>
        /// 判断点是否在多边形区域内，暂时不考虑y轴
        /// 注释：如果是每帧调用，会不会太耗性能，待优化
        /// </summary>
        /// <param name="region"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool PtInRegion(Vector3[] region, Vector3 pt)
        {
            //XGame.Trace.TRACE.ErrorLn("call PtInRegion count=" + region.Count + ",pos="+pt);
            if (region == null || region.Length <= 2)
                return false;

            //判断边长是否和x轴和z轴平行,如果是的话可优化判断
            //配置规则是从左下角逆时针配置
            if (region.Length == 4)
            {
                if ((FloatIsEqual(region[0].z, region[1].z) && FloatIsEqual(region[2].z, region[3].z)
                    && FloatIsEqual(region[0].x, region[3].x) && FloatIsEqual(region[1].x, region[2].x))
                    || (FloatIsEqual(region[0].x, region[1].x) && FloatIsEqual(region[2].x, region[3].x)
                    && FloatIsEqual(region[0].z, region[3].z) && FloatIsEqual(region[1].z, region[2].z)))
                {

                    float fMinX = Mathf.Min(Mathf.Min(region[0].x, region[1].x), Mathf.Min(region[2].x, region[3].x));
                    float fMaxX = Mathf.Max(Mathf.Max(region[0].x, region[1].x), Mathf.Max(region[2].x, region[3].x));

                    float fMinZ = Mathf.Min(Mathf.Min(region[0].z, region[1].z), Mathf.Min(region[2].z, region[3].z));
                    float fMaxZ = Mathf.Max(Mathf.Max(region[0].z, region[1].z), Mathf.Max(region[2].z, region[3].z));


                    if (pt.x < fMinX || pt.x > fMaxX || pt.z < fMinZ || pt.z > fMaxZ)
                    {
                        //区域外
                        return false;
                    }
                    else
                    {
                        //区域内
                        return true;
                    }
                }
            }

            float fminx = 0.0f;
            float fminy = 0.0f;
            float fmaxx = 0.0f;
            float fmaxy = 0.0f;
            //计算AABB包围盒
            if (CalMaxRect(region, ref fminx, ref fminy, ref fmaxx, ref fmaxy))
            {
                //不在包围盒里面
                if (InRect(fminx, fminy, fmaxx, fmaxy, pt) == false)
                {
                    return false;
                }
            }

            bool ret = false;
            int nCount = region.Length;
            int i = 0;
            int j = nCount - 1;
            for (; i < nCount; j = i++)
            {
                if ((((region[i].z <= pt.z) && (pt.z < region[j].z)) ||
                    ((region[j].z <= pt.z) && (pt.z < region[i].z)))
                    && (pt.x < (region[j].x - region[i].x) * (pt.z - region[i].z) / (region[j].z - region[i].z) + region[i].x))
                {
                    ret = !ret;
                }
            }
            return ret;
        }

        /// <summary>
        /// 判断圆形和矩形是否相交 参考算法https://www.zhihu.com/question/24251545
        /// </summary>
        /// <param name="region"></param>
        /// <param name="pt"></param>
        /// <param name="fRadius"></param>
        /// <returns></returns>
        public static bool CircleIntersectRegion(Vector3[] region, Vector3 pt, float fRadius)
        {
            if (region == null || region.Length != 4)
                return false;

            // 计算圆心
            Vector2 vStart;
            vStart.x = region[0].x;
            vStart.y = region[0].z;

            Vector2 vEnd;
            vEnd.x = region[2].x;
            vEnd.y = region[2].z;

            Vector2 vDiagonal = vEnd - vStart;
            //对角线长度
            float fLength = vDiagonal.magnitude;
            //
            vDiagonal.Normalize();
            // 中心点坐标
            Vector2 vCenter = vStart + vDiagonal * fLength * 0.5f;

            // 第1步：转换至第1象限
            Vector2 vPt;
            vPt.x = pt.x;
            vPt.y = pt.z;
            // 求绝对值
            Vector2 vV = vPt - vCenter;
            vV.x = Mathf.Abs(vV.x);
            vV.y = Mathf.Abs(vV.y);

            // 计算第一象限的h点
            bool bFind = false;
            Vector2 vH = default(Vector2);
            for (int i = 0; i < region.Length; i++)
            {
                if (region[i].x - vCenter.x > 0.00001f && region[i].z - vCenter.y > 0.00001f)
                {
                    vH.x = region[i].x - vCenter.x;
                    vH.y = region[i].z - vCenter.y;
                    bFind = true;
                    break;
                }
            }

            if (bFind == false)
                return false;
            // 第2步：求圆心至矩形的最短距离矢量
            Vector2 vU = vV - vH;
            vU.x = Mathf.Max(vU.x, 0);
            vU.y = Mathf.Max(vU.y, 0);

            float fLen = vU.sqrMagnitude;

            // 第3步：长度平方与半径平方比较
            return fLen < fRadius * fRadius;
        }

        /// <summary>
        /// 远和扇形是否相交 参考算法http://blog.csdn.net/zaffix/article/details/25339837
        /// </summary>
        /// <param name="vCenter">圆心</param>
        /// <param name="fRadius">半径</param>
        /// <param name="vFanCenter">扇形圆心</param>
        /// <param name="vFanPos">扇形正前方最远点</param>
        /// <param name="vFanPos">扇形半径</param>
        /// <param name="theta">扇形夹角弧度值theta(0,pi) </param>
        /// <returns></returns>
        //public static bool IsIsCircleIntersectFan(Vector3 vCenter, float fRadius, Vector3 vFanCenter, Vector3 vFanPos, float fFanRadius, float fTheta)
        //{
        //	if (fFanRadius < 0.00001f || fTheta < 0.00001f || fTheta > 3.1416f)
        //		return false;
        //	// 计算扇形正前方向量 v = p1p2  
        //	float vx = vFanPos.x - vFanCenter.x;
        //	float vy = vFanPos.z - vFanCenter.z;

        //	// 计算扇形半径 R = v.length()  
        //	float R = fFanRadius;

        //	// 圆不与扇形圆相交，则圆与扇形必不相交  
        //	if ((vCenter.x - vFanPos.x) * (vCenter.x - vFanPos.x) + (vCenter.z - vFanPos.z) * (vCenter.z - vFanPos.z) > (R + fRadius) * (R + fRadius))
        //		return false;

        //	// 根据夹角 theta/2 计算出旋转矩阵，并将向量v乘该旋转矩阵得出扇形两边的端点p3,p4  
        //	float h = fTheta * 0.5f;
        //	float c = Mathf.Cos(h);
        //	float s = Mathf.Sin(h);
        //	float x3 = vFanCenter.x + (vx * c - vy * s);
        //	float y3 = vFanCenter.z + (vx * s + vy * c);
        //	float x4 = vFanCenter.x + (vx * c + vy * s);
        //	float y4 = vFanCenter.z + (-vx * s + vy * c);

        //	// 如果圆心在扇形两边夹角内，则必相交  
        //	float d1 = EvaluatePointToLine(vCenter.x, vCenter.z, vFanCenter.x, vFanCenter.z, x3, y3);
        //	float d2 = EvaluatePointToLine(vCenter.x, vCenter.z, x4, y4, vFanCenter.x, vFanCenter.z);
        //	if (d1 >= 0 && d2 >= 0)
        //		return true;

        //	// 如果圆与任一边相交，则必相交  
        //	if (IsCircleIntersectLineSeg(vCenter.x, vCenter.z, fRadius, vFanCenter.x, vFanCenter.z, x3, y3))
        //		return true;
        //	if (IsCircleIntersectLineSeg(vCenter.x, vCenter.z, fRadius, vFanCenter.x, vFanCenter.z, x4, y4))
        //		return true;

        //	return false;  
        //}

        // 判断点P(x, y)与有向直线P1P2的关系. 小于0表示点在直线左侧，等于0表示点在直线上，大于0表示点在直线右侧  
        // 参考算法 http://blog.csdn.net/zaffix/article/details/25005057
        public static float EvaluatePointToLine(float x, float y, float x1, float y1, float x2, float y2)
        {
            float a = y2 - y1;
            float b = x1 - x2;
            float c = x2 * y1 - x1 * y2;
            return a * x + b * y + c;
        }

        // 圆与线段碰撞检测   参考算法 http://blog.csdn.net/zaffix/article/details/25160505
        // 圆心p(x, y), 半径r, 线段两端点p1(x1, y1)和p2(x2, y2)  
        public static bool IsCircleIntersectLineSeg(float x, float y, float r, float x1, float y1, float x2, float y2)
        {
            float vx1 = x - x1;
            float vy1 = y - y1;
            float vx2 = x2 - x1;
            float vy2 = y2 - y1;


            // len = v2.length()  
            float len = Mathf.Sqrt(vx2 * vx2 + vy2 * vy2);

            // v2.normalize()  
            vx2 /= len;
            vy2 /= len;

            // u = v1.dot(v2)  
            // u is the vector projection length of vector v1 onto vector v2.  
            float u = vx1 * vx2 + vy1 * vy2;

            // determine the nearest point on the lineseg  
            float x0 = 0.0f;
            float y0 = 0.0f;
            if (u <= 0)
            {
                // p is on the left of p1, so p1 is the nearest point on lineseg  
                x0 = x1;
                y0 = y1;
            }
            else if (u >= len)
            {
                // p is on the right of p2, so p2 is the nearest point on lineseg  
                x0 = x2;
                y0 = y2;
            }
            else
            {
                // p0 = p1 + v2 * u  
                // note that v2 is already normalized.  
                x0 = x1 + vx2 * u;
                y0 = y1 + vy2 * u;
            }

            return (x - x0) * (x - x0) + (y - y0) * (y - y0) <= r * r;
        }

        #endregion

        public static IGame GetGame()
        {
            return GameGlobal.Instance;
        }

        /// <summary>
        /// 调整路径，在离终点distance的地方停下来
        /// </summary>
        /// <param name="pathList"></param>
        /// <param name="fDistance"></param>
        /// <returns></returns>
        public static bool AdjustPahtListAtEnd(ref List<Vector3> pathList, float fDistance)
        {
            if (pathList == null || pathList.Count < 2 || fDistance <= 0.1f)
                return false;

            int index = -1;
            float fleftDistance = fDistance;
            //遍历列表,获取截断的位置
            for (int i = pathList.Count - 2; i >= 0; i--)
            {
                float dis = Vector3.Distance(pathList[i], pathList[i + 1]);
                fleftDistance -= dis;
                if (fleftDistance < 0)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return true;
            }

            Vector3 vStartPos = pathList[index + 1];
            Vector3 vEndPos = pathList[index];

            Vector3 vForward = vStartPos - vEndPos;
            vForward.Normalize();
            pathList[index + 1] = vEndPos + vForward * (-fleftDistance);

            //最后把大于index后面的点都删除
            for (int i = pathList.Count - 1; i > index + 1; i--)
            {
                pathList.RemoveAt(i);
            }
            return true;
        }

        /// <summary>
        /// 调整路径，在离起点distance的地方停下来
        /// </summary>
        /// <param name="pathList"></param>
        /// <param name="fDistance"></param>
        /// <returns></returns>
        public static bool AdjustPahtListAtBegin(ref List<Vector3> pathList, float fDistance)
        {
            if (pathList == null || pathList.Count < 2 || fDistance < 0.1f)
                return false;
            int index = -1;
            float fleftDistance = fDistance;
            //遍历列表,获取截断的位置
            for (int i = 1; i < pathList.Count; i++)
            {
                float dis = Vector3.Distance(pathList[i], pathList[i - 1]);
                fleftDistance -= dis;
                if (fleftDistance < 0)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return true;
            }


            Vector3 vStartPos = pathList[index - 1];
            Vector3 vEndPos = pathList[index];

            Vector3 vForward = vStartPos - vEndPos;
            vForward.Normalize();
            pathList[index] = vEndPos + vForward * (-fleftDistance);

            //最后把大于index后面的点都删除
            for (int i = pathList.Count - 1; i > index; i--)
            {
                pathList.RemoveAt(i);
            }
            return true;
        }

        /// <summary>
        /// 获取监控器
        /// </summary>
        /// <param name="monitorID">监控器id</param>
        /// <returns>监控器实体</returns>
        public static MonitorBase GetMonitor(int monitorID)
        {
            IGame game = GetGame();
            if (game != null)
                return game.GetEtaMonitor(monitorID);
            return null;
        }

        /// <summary>
        /// 获取监视器
        /// </summary>
        /// <param name="monitorID"></param>
        /// <param name="nID"></param>
        /// <param name="desc"></param>
        /// <param name="userTime"></param>
        public static void SetMonitor(int monitorID, int nID, string desc, float userTime)
        {
            MonitorBase monitor = GetMonitor(monitorID);
            if (monitor == null) return;
            monitor.Add(nID, desc, userTime);
        }

        /// <summary>
        /// 发送网络消息
        /// </summary>
        /// <param name="msg"></param>
        public static void SendMessage_CS(gamepol.TCSMessage msg)
        {
            INetModule netModule = GameGlobal.Instance.NetModule;
            if (null == netModule) return ;
            netModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL,NetDefine.ENDPOINT_ZONE,msg);
        }

        /// <summary>
        /// 判断物品是否足够
        /// </summary>
        /// <param name="goodsID"></param>
        /// <param name="isShowTips"></param>
        /// <returns></returns>
        public static bool IsGoodsEngough(int goodsID, int needNum, bool isShowTips = true)
        {
            var goodsNum = GetGoodsNum(goodsID);
            if(goodsNum < needNum)
            {
                var cfg = GameGlobal.GameScheme.Goods_0(goodsID);
                if (cfg != null)
                    ShowSystemFlowText($"{cfg.szName}数量不足！");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 减少物品的数量
        /// </summary>
        /// <param name="goodsID"></param>
        /// <param name="reduceNum"></param>
        /// <returns></returns>
        public static bool ReduceGoodsNum(int goodsID, int reduceNum)
        {
            var goodsNum = GetGoodsNum(goodsID);
            if (goodsNum < reduceNum)
                return false;
            SetGoodsNum(goodsID, goodsNum - reduceNum);
            return true;
        }

        /// <summary>
        /// 获取物品的数量（含虚拟物品）
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        public static int GetGoodsNum(int configId)
        {
            if (configId == NumbericGoodsID.Money)
            {
                if (GameGlobal.Instance.GameInitConfig.serverConfig.isRoomMode)
                    return (int)GameGlobal.RoleAgent.data.goldNum.Value;
                else
                    return (int)GameGlobal.Role.data.goldNum.Value;
            }

            if (configId == NumbericGoodsID.Diamond)
            {
                if (GameGlobal.Instance.GameInitConfig.serverConfig.isRoomMode)
                    return (int)GameGlobal.RoleAgent.data.diamondNum.Value;
                else
                    return (int)GameGlobal.Role.data.diamondNum.Value;
            }

            if (configId == NumbericGoodsID.Diamond2)
            {
                if (GameGlobal.Instance.GameInitConfig.serverConfig.isRoomMode)
                    return (int)GameGlobal.RoleAgent.data.diamondNum2.Value;
                else
                    return (int)GameGlobal.Role.data.diamondNum2.Value;
            }

            return GameGlobal.EntityWorld.Default.GetGoodsEntityNumByConfigId(configId);
        }

        /// <summary>
        /// 设置物品数量（包含虚拟物品）
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="num"></param>
        public static void SetGoodsNum(int configId, int num)
        {
            if (configId == NumbericGoodsID.Money)
            {
                if(GameGlobal.Instance.GameInitConfig.serverConfig.isRoomMode)
                    GameGlobal.RoleAgent.data.goldNum.Value = num;
                else
                    GameGlobal.Role.data.goldNum.Value = num;
                return;
            }

            if (configId == NumbericGoodsID.Diamond)
            {
                if (GameGlobal.Instance.GameInitConfig.serverConfig.isRoomMode)
                    GameGlobal.RoleAgent.data.diamondNum.Value = num;
                else
                    GameGlobal.Role.data.diamondNum.Value = (num);
                return;
            }

            var entity = GameGlobal.EntityWorld.Default.GetGoodsEntityByConfigId(configId);
            entity?.SetNum(num);
        }

        /// <summary>
        /// 显示系统提示
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void ShowSystemFlowText(string content)
        {
            GameGlobal.FlowTextManager.AddFlowText(1, content, 0, 0);
        }

        
        //格式化战力数值
        public static string GetPowerScoreFormatString(int score)
        {
            if (score < 1000000)
                return score.ToString();
            //string formattedNumber = 
            else if (score < 100000000)
            {
                if (score % 10000 == 0)
                    return String.Format("{0}万", score /10000);
                else
                    return String.Format("{0:F2}万", score * 0.0001f);
            }
            else
            {
                if (score % 100000000 == 0)
                    return String.Format("{0}亿", score / 100000000);
                else
                    return String.Format("{0:F2}亿", score * 0.00000001f);
            }
        }


    }
}

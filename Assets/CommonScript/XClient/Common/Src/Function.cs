/*******************************************************************
** 文件名:	DGlobalGame.cs
** 版  权:	(C) 深圳冰川网络网络科技股份有限公司
** 创建人:	宋文武 (sww8@163.com)
** 日  期:	2016-01-25
** 版  本:	1.0
** 描  述:	游戏全局通用函数
** 应  用:  
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
#pragma warning disable 0168

using UnityEngine;
using System;
using System.Text;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using XGame.Utils;
using XGame.Buffer;

namespace XClient.Common
{
	public static class Function
	{
		public static string Md5(string strToEncrypt)
		{
			return HashHelper.ComputeStringMD5(strToEncrypt);
		}

		public static byte[] GetEncodeBytes(string str)
		{
			Encoding encoder = null;
			try
			{
				encoder = Encoding.UTF8;
			}
			catch (Exception ex)
			{
				XGame.Trace.TRACE.ErrorLn("Function::GetEncodeBytes 获取gb2312编码错误.ex=" + ex.ToString());
				encoder = Encoding.Default;
			}

			if (encoder == null)
			{
				encoder = Encoding.Default;
			}

			return encoder.GetBytes(str);
		}

		public static string GetEncodeString(byte[] str)
		{
			if (str == null)
			{
				return string.Empty;
			}
			Encoding encoder = null;
			try
			{
				encoder = Encoding.UTF8;
			}
			catch (Exception ex)
			{
				XGame.Trace.TRACE.ErrorLn("Function::GetEncodeBytes 获取gb2312编码错误.ex=" + ex.ToString());
				encoder = Encoding.Default;
			}

			if (encoder == null)
			{
				encoder = Encoding.Default;
			}

			return encoder.GetString(str);
		}

		/// <summary>
		/// 获取用户自定义路径
		/// </summary>
		/// <returns></returns>
		public static string GetApplicationUserPath()
		{
		    return XGame.GamePath.GetUserDataRootDir();
		}

        /// <summary>
        /// 根据字符串解析XML对象
        /// </summary>
        /// <param name="szData">字符串</param>
        /// <returns></returns>
        public static XMLNode GetXmlReaderByData(string szData)
        {
            if (string.IsNullOrEmpty(szData)) return null;
            XMLNode xn = XmlParser.Parse(szData);
            if (xn == null)
            {
                XGame.Trace.TRACE.ErrorLn("Api::GetXmlReaderByData() 读取错误");
            }
            return xn;
        }

        /// <summary>
        /// 是否是移动端
        /// </summary>
        public static bool IsMobilePlatform()
		{
		    return Platform.IsMobilePlatform();
		}

		/// <summary>
		/// 是否处于测试环境
		/// </summary>
		public static bool IsEditorMode()
		{
			// 如果是Windows平台下的编辑器，则为测试环境
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				return true;
			}

			return false;
		}

        public static string _GT(string szValue)
        {
            return szValue;//mo.Instance.getText(szValue);
        }

        public static string _NGT(string szValue)
        {
            return szValue;
        }

        public static string getText(string utf8_str, int utf8_len, UInt32 type)
        {
            return utf8_str;//mo.Instance.getText(utf8_str,utf8_len,type);
        }

        /// <summary>
        /// 是否是内网环境
        /// </summary>
        /// <returns></returns>
        public static bool IsNatedEnvironment()
		{
			//todo
			return true;
		}

		/// <summary>
		/// 获取设备唯一编号
		/// </summary>
		/// <returns></returns>
		public static UInt32 GetDeviceSerialNo()
		{
			//todo
			return (UInt32)SystemInfo.graphicsDeviceID;
		}

		//计算2维向量的角度
		public static float VectorAngle(Vector2 from, Vector2 to)
		{
			float angle;
			Vector3 cross=Vector3.Cross(from, to);
			angle = Vector2.Angle(from, to);
			return cross.z > 0 ? angle : -angle;
		}

		/// <summary>
		/// 2维向量和世界坐标X轴正方向的角度(-180,180)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static float Vector2AngleWithXAxis(float x, float y)
		{
			Vector2 to;
			to.x = x;
			to.y = y;
			Vector2 from;
			from.x = 1;
			from.y = 0;
			return VectorAngle(from,to);
		}

		/// <summary>
		/// 角度转单位向量
		/// </summary>
		/// <param name="fAngle"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static void Angle2Vector2WithXAxis(float fAngle,ref float x, ref float y)
		{
			x = Mathf.Cos(fAngle * 0.0174533f);
			y = Mathf.Sin(fAngle * 0.0174533f);
		}

		/// <summary>  
		/// SHA1 加密，返回大写字符串  
		/// </summary>  
		public static string SHA1(string str)
		{
			SHA1 sha1 = new SHA1CryptoServiceProvider();
			byte[] bytes_in = Encoding.UTF8.GetBytes(str);
			byte[] bytes_out = sha1.ComputeHash(bytes_in);
			string result = BitConverter.ToString(bytes_out);
			result = result.Replace("-", "");
			return result;
		}

		/// <summary>
		/// MD5
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string MD5(string str)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] data = Encoding.UTF8.GetBytes(str);
			byte[] result = md5.ComputeHash(data);
			String strReturn = String.Empty;
			for (int i = 0; i < result.Length; i++)
				strReturn += result[i].ToString("x").PadLeft(2, '0');
			return strReturn;
		}

		public static string UrlEncode(string str)
		{
			StringBuilder sb = new StringBuilder();
			byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); 
			for (int i = 0; i < byStr.Length; i++)
			{
				sb.Append(@"%" + Convert.ToString(byStr[i], 16));
			}

			return (sb.ToString());
		}

        /// <summary>
        /// CPacketSend 转成 CPacketRecv
        /// </summary>
        /// <param name="PacketSend"></param>
        /// <returns></returns>
        public static CPacketRecv RecvFromSend(CPacketSend PacketSend)
        {
            if (PacketSend == null) return null;
            CPacketRecv pszMsg = new CPacketRecv();

            byte[] SendBuf = PacketSend.GetByte();
            pszMsg.Init(SendBuf);

            return pszMsg;
        }

        /// <summary>
        /// byte[] 转成 CPacketRecv
        /// </summary>
        /// <param name="SendBuf"></param>
        /// <returns></returns>
        public static CPacketRecv RecvFromByte(byte[] SendBuf)
        {
            CPacketRecv pszMsg = new CPacketRecv();
            if (SendBuf == null) return pszMsg;
            pszMsg.Init(SendBuf);

            return pszMsg;
        }

        public static CPacketRecv RecvFromString(string Text)
        {
            CPacketRecv pszMsg = new CPacketRecv();
            if (string.IsNullOrEmpty(Text)) return pszMsg;
            byte[] SendBuf = StringUtil.StringToByte(Text);
            if (SendBuf == null) return pszMsg;

            pszMsg.Init(SendBuf);

            return pszMsg;
        }


        /// <summary>
        /// 输出设备信息
        /// </summary>
        public static void OutputDeviceInfo()
		{
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("开始输出设备信息----------------------------------------------------------");
#endif

#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备型号:" + SystemInfo.deviceModel);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备名称:" + SystemInfo.deviceName);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备唯一标识:" + SystemInfo.deviceUniqueIdentifier);
#endif

#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备操作系统:" + SystemInfo.operatingSystem);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备内存数量:" + SystemInfo.systemMemorySize);
#endif

#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备处理器核心数:" + SystemInfo.processorCount);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备处理器名称:" + SystemInfo.processorType);
#endif

#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备显卡标识码:" + SystemInfo.graphicsDeviceID);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备显卡名称:" + SystemInfo.graphicsDeviceName);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备显卡厂商:" + SystemInfo.graphicsDeviceVendor);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备显卡厂商标识码:" + SystemInfo.graphicsDeviceVendorID);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备显卡的显存数量:" + SystemInfo.graphicsMemorySize);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备显卡支持的图形API版本:" + SystemInfo.graphicsDeviceVersion);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备图形着色器性能级别:" + SystemInfo.graphicsShaderLevel);
#endif


#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备最大纹理数量:" + SystemInfo.maxTextureSize);
#endif

#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持NPOT纹理?" + ((SystemInfo.npotSupport == NPOTSupport.Full) ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设置支持同时渲染多少个目标:" + SystemInfo.supportedRenderTargetCount);
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持3D纹理?" + (SystemInfo.supports3DTextures ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持加速计?" + (SystemInfo.supportsAccelerometer ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持计算着色器?" + (SystemInfo.supportsComputeShaders ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持陀螺仪?" + (SystemInfo.supportsGyroscope ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持图像效果？" + (SystemInfo.supportsImageEffects ? "支持" : "不支持"));
#endif


#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持GPU绘制调用实例?" + (SystemInfo.supportsInstancing ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持访问位置服务?" + (SystemInfo.supportsLocationService ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持渲染纹理?" + (SystemInfo.supportsRenderTextures ? "支持" : "不支持"));
#endif
#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持内置阴影？" + (SystemInfo.supportsShadows ? "支持" : "不支持"));
#endif

#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("设备是否支持震动?" + (SystemInfo.supportsVibration ? "支持" : "不支持"));
#endif

#if DEBUG_LOG
///#///#			XGame.Trace.TRACE.TraceLn("结束输出设备信息----------------------------------------------------------");
#endif
		}

	}
}


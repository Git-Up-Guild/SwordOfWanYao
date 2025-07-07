using System;
using System.Runtime.InteropServices;

namespace keywordfilter
{
    public class kwFilterApi
    {
        
		#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
			const string KWFILTERDLL = "__Internal";
		#else
			const string KWFILTERDLL = "libkwfilter";
		#endif
		

		//  创建过滤器
        [DllImport(KWFILTERDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr BuildFilterInstance(byte[] pConfig, int iLen);


		//  释放过滤器
        [DllImport(KWFILTERDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseFilterInstance(IntPtr kwFilter);

        //  检测字符串是否非法.  
		//  调用前需要使用 Encoding.UTF8.GetBytes( str  ); 进行编码
        [DllImport(KWFILTERDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsStringLegal(IntPtr kwFilter, byte[] pUtf8String, int iStrLen);


		//  获取最后一次违规信息. 需要在 IsStringLegal() 返回false后使用
		//  返回值:                  是否能获取违规信息.  某些情况下可能有BUG, 能查出句子有问题,但取不出违规信息. 所以要先检查这个值
		//  iStartPos, iLen:    违规字的位置, 索引从0开始.   (注:   是unicode字符串的位置, 不是utf8串的位置)
        [DllImport(KWFILTERDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetLastIllegedInfo(IntPtr kwFilter, out int iStartPos, out int iLen);

		//  测试函数, 不要调用
        [DllImport(KWFILTERDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Test(IntPtr kwFilter, byte[] pUtf8String, int iStrLen);
        


    }

}



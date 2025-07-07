using System.Security.Cryptography;
using System.Text;

namespace XGame.AssetScript.SDK.Core
{
    public static class SDKUtility
    {
        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ComputeStringMD5(string input)
        {
            MD5 md5 = MD5.Create();

            //将字符串转成字节数组
            byte[] buffer = Encoding.UTF8.GetBytes(input);

            //调用加密方法
            byte[] byteArray = md5.ComputeHash(buffer);

            StringBuilder sb = new StringBuilder();

            //遍历字节数组
            foreach (byte b in byteArray)
            {
                //将字节数组转成16进制的字符串。X表示16进制，2表示每个16字符占2位
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}

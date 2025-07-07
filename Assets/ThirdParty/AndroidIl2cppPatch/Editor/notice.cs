
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class DingTalkHelper
{
    private const string WEB_HOOK = @"https://oapi.dingtalk.com/robot/send?access_token=a9067abdae10656b8c20bf26de699c42fac02823350596ac6a2d994ab0c785e3";   //设置自己创建的机器人地址

    /// <summary>
    /// 钉钉机器人发送通知消息
    /// </summary>
    /// <param name="msg"></param>
    public static void Notify()
    {
        string textMsg = "{ \"msgtype\": \"markdown\", \"markdown\": {\"title\":\"" + "打包完成通知" + "\" ,\"text\": \"" + "#### 打包完成\n\n " + "> ![screenshot](http://118.195.193.61//picture/334.vilitown.vfight.png)\n " + "> ######  [前往下载地址](http://192.168.2.33:8080/) \n" + "\"}}";
        string s = Post(textMsg, null);

        Debug.Log("Post return:" + s);
    }

    public static void Notify_fail()
    {
        string textMsg = "{\"text\": {\"content\":\"打包完成通知：打包失败，请检查\" },\"msgtype\":\"text\"}";
        string s = Post(textMsg, null);

        Debug.Log("Post return:" + s);
    }
    #region Post
    /// <summary>
    /// 以Post方式提交命令
    /// </summary>
    /// <param name="apiurl">请求的URL</param>
    /// <param name="jsonstring">请求的json参数</param>
    /// <param name="headers">请求头的key-value字典</param>
    private static string Post(string jsonstring, Dictionary<string, string> headers = null)
    {
        //远程证书无效
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate,
                     X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };

        WebRequest request = WebRequest.Create(WEB_HOOK);
        request.Method = "POST";
        request.ContentType = "application/json";
        if (headers != null)
        {
            foreach (var keyValue in headers)
            {
                if (keyValue.Key == "Content-Type")
                {
                    request.ContentType = keyValue.Value;
                    continue;
                }
                request.Headers.Add(keyValue.Key, keyValue.Value);
            }
        }

        if (string.IsNullOrEmpty(jsonstring))
        {
            request.ContentLength = 0;
        }
        else
        {
            byte[] bs = Encoding.UTF8.GetBytes(jsonstring);
            request.ContentLength = bs.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(bs, 0, bs.Length);
            newStream.Close();
        }


        WebResponse response = request.GetResponse();
        Stream stream = response.GetResponseStream();
        Encoding encode = Encoding.UTF8;
        StreamReader reader = new StreamReader(stream, encode);
        string resultJson = reader.ReadToEnd();
        return resultJson;
    }
    #endregion
}
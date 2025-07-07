using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGameEditor.FunctionOpen
{
    public static class EncodingUtility
    {

        /// <summary>
        /// 获取常用的文本编码列表
        /// </summary>
        private static Encoding[] encodings = new Encoding[] {
            Encoding.UTF8,
            Encoding.ASCII,
            Encoding.GetEncoding("gb2312"), // 简体中文
            Encoding.Unicode,
            Encoding.BigEndianUnicode,
            Encoding.UTF32,
            Encoding.GetEncoding("big5"),   // 繁体中文
            Encoding.Default
        };

        /// <summary>
        /// 检测指定文件的编码，并返回其名称。
        /// </summary>
        /// <param name="filePath">待检测文件的路径。</param>
        /// <returns>检测到的文件编码名称。若无法确定编码，则返回null（或根据需要抛出异常）。</returns>
        public static Encoding DetectFileEncoding(string filePath)
        {
            byte[] buffer = new byte[4096];
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                int readBytes = fileStream.Read(buffer, 0, buffer.Length);

                if (readBytes == 0)
                {
                    Console.WriteLine("文件为空。");
                    return null; // 或根据需要抛出异常
                }

                Encoding detectedEncoding = null;

                // 遍历常用编码列表，尝试解码文件内容
                foreach (var encoding in encodings)
                {
                    try
                    {
                        string decodedString = encoding.GetString(buffer, 0, readBytes);
                        if (!decodedString.Contains("\ufffd") && !decodedString.Contains("\\uFFFD"))
                        {
                            // 确定编码后，将正确名称赋值给detectedEncodingName，并跳出循环
                            detectedEncoding = encoding;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"使用 {encoding.EncodingName} 解码时出错: {ex.Message}");
                    }
                }

                if (detectedEncoding == null)
                    Console.WriteLine($"无法确定文件的编码。{filePath}");

                return detectedEncoding;
            }
        }
    }
}

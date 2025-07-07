/*****************************************************
** 文 件 名：ShaderUtils
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/6/19 22:09:41
** 内容简述：Shader 关键字 查看
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.ShaderVariantCollection;

namespace XGameEditor.ShaderTool
{
    public struct ShaderVariantData
    {
        public int[] passTypes;
        public string[] keywordLists;
        public string[] remainingKeywords;
    }

    class ShaderUtils
    {
        public static ShaderVariantData GetShaderEntriesData(Shader sd, ShaderVariantCollection svc, ref List<string> SelectedKeywords)
        {
            string[] keywordLists = null, remainingKeywords = null;
            int[] FilteredVariantTypes = null;
            MethodInfo GetShaderVariantEntries = typeof(ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.NonPublic | BindingFlags.Static); ;
            object[] args = new object[] { sd,
                256,
                SelectedKeywords.ToArray(),
                svc,
                 FilteredVariantTypes,
                 keywordLists,
                 remainingKeywords};
            GetShaderVariantEntries.Invoke(null, args);
            ShaderVariantData svd = new ShaderVariantData();
            svd.passTypes = args[4] as int[];
            svd.keywordLists = args[5] as string[];
            svd.remainingKeywords = args[6] as string[];
            return svd;
        }

        public static long GetVariantCount(Shader shader)
        {
            var fnGetVariantCount = typeof(ShaderUtil).GetMethod("GetVariantCount", BindingFlags.NonPublic | BindingFlags.Static);
            object[] args = new object[] { shader, true };
            var count = fnGetVariantCount.Invoke(null, args);
            return Convert.ToInt64(count);
        }

        public static string[] GetShaderGlobalKeywords(Shader shader)
        {
            if (shader != null)
            {
                var fnGetGlobalKeywords = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.NonPublic | BindingFlags.Static);
                object[] args = new object[] { shader };

                return fnGetGlobalKeywords.Invoke(null, args) as string[];
            }
            return new string[0];
        }

        public static string[] GetShaderLocalKeywords(Shader shader)
        {
            if (shader != null)
            {
                var fnGetLocalkeywords = typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.NonPublic | BindingFlags.Static);
                object[] args = new object[] { shader };
                return fnGetLocalkeywords.Invoke(null, args) as string[];
            }
            return new string[0];
        }
    }
}

/*******************************************************************
** 文件名: VerifyActionBase.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博
** 日  期: 2018/17/7
** 版  本: 1.0
** 描  述: 资源检测工具—数值校验器
** 应  用: 负责对数值进行指定规则的合法性校验
**         校验字符串格式：[] + 校验范围值 "EQ" =, "NE" !=, "LT" <, "GT" >, "LE" <=, "GE" >=, "IV" 区间, "ICIV" 左右包含型区间
**         例：1.[EQ]5 代表校验某值是否等于5
**             2.[ICIV]2;4 代表校验某值是否大于等于2且小于等于4    
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections.Generic;

namespace XGameEditor.AssetVerifyTools
{
    public class ValueVerifier
    {
        /// <summary>
        /// 运算类型
        /// </summary>
        public enum OperatorType
        {
            Unknown = -1,
            Equal,
            NotEqual,
            Less,
            Greater,
            LessOrEqual,
            GreaterOrEqual,
            Interval,
            InclusiveInterval,
        }

        /// <summary>
        /// 区间
        /// </summary>
        public struct Interval
        {
            public float minValue;
            public float maxValue;

            public Interval(float min, float max)
            {
                minValue = min;
                maxValue = max;
            }
        }

        //校验数据
        private string m_verifyString;
        //检验标准值
        private float m_fSingleValue;
        //校验标准区间
        private List<Interval> m_intervalList;
        //运算类型
        private OperatorType m_operatorType;
        //运算简写定义
        private readonly string[] OperatorDefineArry = new string[8] { "EQ", "NE", "LT", "GT", "LE", "GE", "IV", "ICIV" };

        public ValueVerifier()
        {
            m_verifyString = "";
            m_fSingleValue = 0;
            m_intervalList = new List<Interval>();
            m_operatorType = OperatorType.Unknown;
        }

        ~ValueVerifier()
        {
            m_verifyString = "";
            m_fSingleValue = 0;
            m_intervalList.Clear();
            m_intervalList = null;
            m_operatorType = OperatorType.Unknown;
        }

        /// <summary>
        /// 解析运算字符串
        /// </summary>
        /// <param name="originalData">运算标准字符串</param>
        /// <returns></returns>
        public bool TryParseString(string originalData)
        {
            if (string.IsNullOrEmpty(originalData))
            {
                return false;
            }

            //校验括号是否成对出现
            if (!originalData.StartsWith("["))
            {
                return false;
            }

            int nEndIndex = originalData.IndexOf("]");
            if(nEndIndex < 3)
            {
                return false;
            }

            m_verifyString = originalData;

            string szOperator = originalData.Substring(1, nEndIndex - 1);
            for(int i =0; i < OperatorDefineArry.Length; i++)
            {
                if (szOperator.Equals(OperatorDefineArry[i]))
                {
                    m_operatorType = (OperatorType)i;
                    break;
                }
            }

            originalData = originalData.Remove(0, szOperator.Length + 2);

            if(m_operatorType == OperatorType.Interval || m_operatorType == OperatorType.InclusiveInterval)
            {
                string[] valueArry = originalData.Split(';');
                //区间型范围值必须成对出现
                if(valueArry.Length == 0 || valueArry.Length % 2 != 0)
                {
                    return false;
                }

                int nArryIndex = 0;
                m_intervalList = new List<Interval>();
                while(nArryIndex < (valueArry.Length - 1))
                {
                    float nMin, nMax;
                    if(!float.TryParse(valueArry[nArryIndex++], out nMin) || !float.TryParse(valueArry[nArryIndex++], out nMax))
                    {
                        return false;
                    }

                    Interval section = new Interval(nMin, nMax);
                    m_intervalList.Add(section);
                }
            }
            else
            {
                if(!float.TryParse(originalData, out m_fSingleValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 校验值合法性
        /// </summary>
        /// <param name="value">待校验值</param>
        /// <returns>是否通过校验</returns>
        public bool VerifyValue(float value)
        {
            switch (m_operatorType)
            {
                case OperatorType.Equal:
                    {
                        return value == m_fSingleValue;
                    }
                case OperatorType.NotEqual:
                    {
                        return value != m_fSingleValue;
                    }
                case OperatorType.Less:
                    {
                        return value < m_fSingleValue;
                    }
                case OperatorType.Greater:
                    {
                        return value > m_fSingleValue;
                    }
                case OperatorType.LessOrEqual:
                    {
                        return value <= m_fSingleValue;
                    }
                case OperatorType.GreaterOrEqual:
                    {
                        return value >= m_fSingleValue;
                    }
                case OperatorType.Interval:
                    {
                        foreach (Interval section in m_intervalList)
                        {
                            if (value > section.minValue && value < section.maxValue)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                case OperatorType.InclusiveInterval:
                    {
                        foreach (Interval section in m_intervalList)
                        {
                            if (value >= section.minValue && value <= section.maxValue)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                case OperatorType.Unknown:
                default:
                    {
                        return false;
                    }              
            }
        }
    }
}

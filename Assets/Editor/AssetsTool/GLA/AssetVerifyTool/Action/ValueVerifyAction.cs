/*******************************************************************
** 文件名: ValueVerifyAction.cs
** 版  权: (C) 深圳冰川网络技术有限公司 
** 创建人: 昔文博 
** 日  期: 2018/12/7
** 版  本: 1.0
** 描  述: 资源检测工具—赋值型检测行为  
** 应  用:     

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections;

namespace XGameEditor.AssetVerifyTools
{
	public class ValueVerifyAction : CommonVerifyAction
	{
		/// <summary>
		/// 获取行为类型
		/// </summary>
		/// <returns></returns>
		public override VerifyActionType GetActionType()
		{
			return VerifyActionType.ValueVerify;
		}

		/// <summary>
		/// 执行检测
		/// </summary>
		/// <param name="verifyObj">检测对象</param>
		/// <param name="ruleData">检测规则</param>
		/// <param name="resultData">检测结果</param>
		public override void Execute(UnityEngine.Object verifyObj, VerifyRuleData ruleData, ref VerifyResult resultData)
		{
			base.Execute(verifyObj, ruleData, ref resultData);
			
			if(resultData.GetVerifyResult() == VerifyResultType.Failed)
				return;

			Type objType = verifyObj.GetType();
			//获取属性集合
            System.Reflection.PropertyInfo[] propInfoArry = objType.GetProperties();
            foreach(System.Reflection.PropertyInfo propInfo in propInfoArry)
            {
                if (propInfo == null || propInfo.Name != ruleData.paramArry[0])
                {
					continue;
				}
          
                System.Object propObj = propInfo.GetValue(verifyObj, null);
				if(propObj == null)
				{
					continue;
				}

				//获取当前属性值
				float propValue;
				if(!float.TryParse(propObj.ToString(), out propValue))
				{
					continue;
				}

				//解析限定范围
				ValueVerifier valVerifier = new ValueVerifier();
				if(!valVerifier.TryParseString(ruleData.paramArry[1]))
				{
					resultData.SetResultType(VerifyResultType.Failed);
					resultData.AddOutputInfo(LogLevel.Error,"校验限定值解析出错！配置：" + ruleData.paramArry[1], verifyObj);
					continue;
				}

				//如果合法，直接跳过
				if(valVerifier.VerifyValue(propValue))
				{
					continue;
				}
				
				resultData.SetResultType(VerifyResultType.Failed);
				resultData.AddOutputInfo(LogLevel.Error, propInfo.Name + "与限定值不一致！当前值:"+ propValue, verifyObj);

				int fix;
				if(string.IsNullOrEmpty(ruleData.paramArry[2]) || !int.TryParse(ruleData.paramArry[2], out fix))
				{
					continue;
				}

				//不需要修复，则跳过
				if(fix != 1)
				{
					continue;
				}

				//先检查标准值
				int standradVal;
				if(string.IsNullOrEmpty(ruleData.paramArry[3]) || !int.TryParse(ruleData.paramArry[3], out standradVal))
				{
					resultData.SetResultType(VerifyResultType.Failed);
					resultData.AddOutputInfo(LogLevel.Error, "检测规则标准值解析失败！规则id:" + ruleData.ruleId, verifyObj);
					continue;
				}

				if(propObj.GetType() == typeof(int))
				{
					propInfo.SetValue(verifyObj, (standradVal as object), null);
					resultData.SetResultType(VerifyResultType.Fixed);
				}
				else if(propObj.GetType() == typeof(float))
				{
					propInfo.SetValue(verifyObj, ((float)standradVal as object), null);
					resultData.SetResultType(VerifyResultType.Fixed);
				}			
			} //end of foreach

            System.Reflection.FieldInfo[] fieldInfoArry = objType.GetFields();
            foreach(System.Reflection.FieldInfo fieldInfo in fieldInfoArry)
            {
                if (fieldInfo == null ||  fieldInfo.Name != ruleData.paramArry[0])
                {
					continue;
				}

				System.Object propObj = fieldInfo.GetValue(verifyObj);
				if(propObj == null)
				{
					continue;
				}

				float propValue;
				if(!float.TryParse(propObj.ToString(), out propValue))
				{
					continue;
				}

				ValueVerifier valVerifier = new ValueVerifier();
				if(!valVerifier.TryParseString(ruleData.paramArry[1]))
				{
					resultData.SetResultType(VerifyResultType.Failed);
					resultData.AddOutputInfo(LogLevel.Error,"校验限定值解析出错！配置：" + ruleData.paramArry[1], verifyObj);
					continue;
				}

				//如果合法，直接跳过
				if(valVerifier.VerifyValue(propValue))
				{
					continue;
				}
				
				resultData.SetResultType(VerifyResultType.Failed);
				resultData.AddOutputInfo(LogLevel.Error, fieldInfo.Name + "与限定值不一致！当前值:"+ propValue, verifyObj);

				int fix;
				if(string.IsNullOrEmpty(ruleData.paramArry[2]) || !int.TryParse(ruleData.paramArry[2], out fix))
				{
					continue;
				}

				//不需要修复，则跳过
				if(fix != 1)
				{
					continue;
				}

				if(string.IsNullOrEmpty(ruleData.paramArry[3]))
				{
					resultData.SetResultType(VerifyResultType.Failed);
					resultData.AddOutputInfo(LogLevel.Error, "检测规则标准值为空！", verifyObj);
					continue;
				}


				int standradVal;
				if(!int.TryParse(ruleData.paramArry[3], out standradVal))
				{
					resultData.SetResultType(VerifyResultType.Failed);
					resultData.AddOutputInfo(LogLevel.Error, "自动修复标准值解析错误！规则id:"+ ruleData.ruleId, verifyObj);
				}
				else
				{
					if(propObj.GetType() == typeof(int))
					{
						fieldInfo.SetValue(verifyObj, ((int)standradVal as object));
						resultData.SetResultType(VerifyResultType.Fixed);
					}
					else if(propObj.GetType() == typeof(float))
					{
						fieldInfo.SetValue(verifyObj, ((float)standradVal as object));
						resultData.SetResultType(VerifyResultType.Fixed);
					}	
				}		           
            } //end of foreach
		} //end of execute 
	} //end of class
} //end of namespace

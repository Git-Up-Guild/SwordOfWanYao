/*******************************************************************
** 文件名:	Object2Lua.cs
** 版  权:	(C) 深圳冰川网络技术有限公司
** 创建人:李世柳	
** 日  期:	2020/6/22
** 版  本:	1.0
** 描  述:	object转lua
** 应  用:  	

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace XGameEditor.MapBehavior
{
    public class Object2Lua 
    {
        public delegate bool IsClassObject(object value);

        static string AppendData(string str,string appendStr)
        {
            if (appendStr == null) return str;
            if (str != null)
            {
                str = str + "," + appendStr;
            }
            else
            {
                str = appendStr;
            }
            return str;
        }

        public static string SerializeObjectValue(object obj,string interText=null, IsClassObject callBack=null)
        {
            if (obj == null) return null;
            string luaTableStr = null;

            Type objType = obj.GetType();
            FieldInfo[] fArr = objType.GetFields(BindingFlags.Default | BindingFlags.Public| BindingFlags.Instance | BindingFlags.CreateInstance);

            for (int i = 0; i < fArr.Length; i++)
            {
                //success = SerializeValue(fArr[i].GetValue(obj), ref luaTableStr);
                object value = fArr[i].GetValue(obj);
                string fileName = fArr[i].Name;
                string luaStr = SerializeValue(fileName, value, callBack);
                luaTableStr = AppendData(luaTableStr, luaStr);
              

            }
            if (interText != null)
            {
                luaTableStr= AppendData(luaTableStr, interText);
            }
            return "{" + luaTableStr + "}";
        }

        static string GetString(string key,string value)
        {
            if (key == null)
            {
                return string.Format("{0}", value);
            }
            return string.Format("{0}={1}", key, value);
        }

        public static string SerializeValue(string key, object value, IsClassObject callBack)
        {
            double doubleValue = 0;

            if (value == null)
            { 
                return GetString( key, "nil");
            }
 

            if (value is GameObject)
            {
                return null;
            }
            if(value is Transform)
            {
                return null;
            }


            if (value is string)
            { 
                return GetString(key,"'" + value+"'");
            }
            else if (value is IDictionary)
            {
                string str = SerializeDict(value as IDictionary, callBack);
              
                return GetString(key, str);
            }
            else if (value is IList)
            {
                string str = SerializeList(value as IList, callBack);
               
                return GetString(key, str);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            { 
                return GetString(key, "true");
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            { 
                return GetString(key, "false");
            }
            else if (value is Color)
            {
                Color color = (Color)value;
                string colorStr = "{" + string.Format("r={0},g={1},b={2},a={3}",
                    color.r, color.b, color.g, color.a) + "}";
              
                return GetString(key, colorStr);
            }
            else if (value is Vector2)
            {
                Vector2 v = (Vector2)value;
                string vStr = "{" + string.Format("x={0},y={1}", v.x, v.y) + "}";

                return GetString( key, vStr);
            }
            else if (value is Vector3)
            {
                Vector3 v = (Vector3)value;
                string vStr = "{" + string.Format("x={0},y={1},z={2}", v.x, v.y, v.z) + "}";
                 
                return GetString(key, vStr);
            }
            else if (value is Vector4)
            {
                Vector4 v = (Vector4)value;
                string vStr = "{" + string.Format("x={0},y={1},z={2},w={3}", v.x, v.y, v.z, v.w) + "}";
                
                return GetString(key, vStr);
            }
            else if (double.TryParse(value.ToString(), out doubleValue))
            { 
                return GetString(key, value.ToString());
            }
            else if (value == null)
            {
               
                return GetString(key, "nil");
            }
            else if (callBack != null && callBack(value))
            {
                string strVal = SerializeObjectValue(value,null, callBack);

                strVal = GetString(key, strVal)+"\n";
                return strVal;
                //return string.Format("{0}={1}\n", key, strVal);
            }

            else if(value is Enum)
            {
                int intV = (int)value;
                return GetString(key, intV.ToString());
               // Debug.LogError("未知道类型：" + value.GetType());
            }
            else
            {
                Debug.LogError("未知道类型：" + value.GetType());
            }
            
            return GetString( key, "nil");
        }


        public static string SerializeList(IList anArray, IsClassObject callBack)
        {
            string luaTable = null;
            for (int i = 0; i < anArray.Count; i++)
            {
                string value = SerializeValue(null, anArray[i], callBack);
                
                luaTable=AppendData(luaTable, value);
            }
            luaTable = "{" + luaTable + "}";
 
            return luaTable;
            // return string.Format("{0}={1},", key, luaTable);
        }


        public static string SerializeDict(IDictionary anObject, IsClassObject callBack)
        {
            string luaTable = null;
            IDictionaryEnumerator e = anObject.GetEnumerator();

            bool first = true;
            while (e.MoveNext())
            {
                string dictKey = e.Key.ToString();

                object value = e.Value;

                if (e.Key is string)
                {
                    dictKey = e.Key.ToString();
                }
                else if (e.Key is ValueType)
                {
                    dictKey = "[" + e.Key.ToString() + "]";
                }
                else
                {
                    luaTable = "nil";
                    Debug.Log("字典暂时不支持：key= " + e.Key.GetType());
                    break;
                }
                //luaTable = luaTable + SerializeValue(dictKey, value, callBack);
                luaTable= AppendData(luaTable, SerializeValue(dictKey, value, callBack));
            }

            luaTable = "{" + luaTable + "}";
            return luaTable;
            // return string.Format("{0}={1},", key, luaTable);
        }
    }

}

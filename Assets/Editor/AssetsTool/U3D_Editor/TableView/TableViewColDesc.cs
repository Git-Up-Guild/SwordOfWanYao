using UnityEngine;
using System.Reflection;
using System;
using UnityEditor;

namespace XGameEditor.GlacierEditor.TableView
{
    public class TableViewColDesc
    {
        public string PropertyName;
        public string TitleText;

        public TextAnchor Alignment;
        public string Format;
        public float WidthInPercent;

        public MemberInfo MemInfo;

        public static object MemberValue(object obj, MemberInfo memInfo)
        {
            if (obj == null)
                return "";
            if (memInfo == null)
                return "";

            var pi = memInfo as PropertyInfo;
            if (pi != null)
            {
                return pi.GetValue(obj, null);
            }

            var fi = memInfo as FieldInfo;
            if (fi != null)
            {
                return fi.GetValue(obj);
            }

            return "";
        }

        public static string MemberToString(object obj, MemberInfo memInfo, string fmt)
        {
            object val = MemberValue(obj, memInfo);
            if (val == null)
                return "";

            if (fmt == "<fmt_bytes>")
                return EditorUtility.FormatBytes((int)val);
            if (val is float)
                return ((float)val).ToString(fmt);
            if (val is double)
                return ((double)val).ToString(fmt);
            return val.ToString();
        }

        public string FormatObject(object obj)
        {
            return MemberToString(obj, MemInfo, Format);
        }

        public int Compare(object o1, object o2)
        {
            object fv1 = MemberValue(o1, MemInfo);
            object fv2 = MemberValue(o2, MemInfo);

            IComparable fc1 = fv1 as IComparable;
            IComparable fc2 = fv2 as IComparable;
            if (fc1 == null || fc2 == null)
            {
                return fv1.ToString().CompareTo(fv2.ToString());
            }

            return fc1.CompareTo(fc2);
        }
    }

}


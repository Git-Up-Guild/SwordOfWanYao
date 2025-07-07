/*****************************************************
** 文 件 名：ObjectPropertyAttributeDefineAttribute
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/1/7 21:05:16
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using System;

namespace XGameEditor.Attr
{
    public class ObjectPropertyAttrLayoutAttribute : Attribute
    {
        public System.Type targetType;
        public ObjectPropertyAttrLayoutAttribute(System.Type targetType)
        {
            this.targetType = targetType;
        }
    }
}

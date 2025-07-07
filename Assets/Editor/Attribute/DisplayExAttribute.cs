/*****************************************************
** 文 件 名：DisplayExAttribute
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/1/7 21:05:16
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using XGame.Attr;

namespace XGameEditor.Attr
{
    public class DispVector3Attribute : DisplayAttribute
    {
        public DispVector3Attribute(string label, bool isReadOnly = false, bool isHide = false) : base(label, isReadOnly, isHide) 
        {
        }
        public DispVector3Attribute(string label, int tW, int lW = 0, int pW = 0, bool isReadOnly = false, bool isHide = false)
            : base(label, tW, lW, pW, isReadOnly, isHide)
        {
        }
    }

    public class DispVector2Attribute : DisplayAttribute
    {
        public DispVector2Attribute(string label, bool isReadOnly = false, bool isHide = false) : base(label, isReadOnly, isHide)
        {
        }
        public DispVector2Attribute(string label, int tW, int lW = 0, int pW = 0, bool isReadOnly = false, bool isHide = false)
            : base(label, tW, lW, pW, isReadOnly, isHide)
        {
        }
    }

    /// <summary>
    /// 对象序列化
    /// </summary>
    public class SerializableObjAttribute : DisplayAttribute
    {
        public SerializableObjAttribute(string label, bool isReadOnly = false, bool isHide = false) : base(label, isReadOnly, isHide) {}
        public SerializableObjAttribute() : base("", false, true) { }
        public SerializableObjAttribute(string label, int tW, int lW = 0, int pW = 0, bool isReadOnly = false, bool isHide = false)
            : base(label, tW, lW, pW, isReadOnly, isHide)
        {
        }
    }

    /// <summary>
    /// 对象列表特性
    /// </summary>
    public class SerializableObjListAttribute : DisplayAttribute
    {
        public string funcDel;
        public string funcAdd;

        public SerializableObjListAttribute(string label, string funcDel, string funcAdd, bool isReadOnly = false, bool isHide = false) : base(label, isReadOnly, isHide)
        {
            this.funcAdd = funcAdd;
            this.funcDel = funcDel;
        }

        public SerializableObjListAttribute(string label, string funcDel, string funcAdd, int tW = 0, int lW = 0, int pW = 0, bool isReadOnly = false, bool isHide = false)
            : base(label, tW, lW, pW, isReadOnly, isHide)
        {
            this.funcAdd = funcAdd;
            this.funcDel = funcDel;
        }
    }

    /// <summary>
    /// 对象序列化
    /// </summary>
    public class SerializableObjExAttribute : SerializableObjAttribute
    {
        public SerializableObjExAttribute(string label, bool isReadOnly = false, bool isHide = false) : base(label, isReadOnly, isHide) { }
        public SerializableObjExAttribute() : base("", false, true) { }
        public SerializableObjExAttribute(string label, int tW, int lW = 0, int pW = 0, bool isReadOnly = false, bool isHide = false)
            : base(label, tW, lW, pW, isReadOnly, isHide)
        {
        }
    }


    /// <summary>
    /// 对象列表特性
    /// </summary>
    public class SerializableObjListExAttribute : DisplayAttribute
    {
        public bool isUseDelBtn;
        public bool isUseAddBtn;

        public SerializableObjListExAttribute(string label, bool useDelBtn, bool useAddBtn = false, bool isReadOnly = false, bool isHide = false) : base(label, isReadOnly, isHide)
        {
            isUseDelBtn = useDelBtn;
            isUseAddBtn = useAddBtn;
        }

        public SerializableObjListExAttribute(string label, bool useDelBtn, bool useAddBtn, int tW = 0, int lW = 0, int pW = 0, bool isReadOnly = false, bool isHide = false)
            : base(label, tW, lW, pW, isReadOnly, isHide)
        {
            isUseDelBtn = useDelBtn;
            isUseAddBtn = useAddBtn;
        }
    }
}

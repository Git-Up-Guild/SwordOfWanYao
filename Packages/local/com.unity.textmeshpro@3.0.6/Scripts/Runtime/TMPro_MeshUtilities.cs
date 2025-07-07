using UnityEngine;
using UnityEngine.TextCore;
using System;
using System.Collections.Generic;

namespace TMPro
{
    /// <summary>
    /// Flags to control what vertex data is pushed to the mesh and renderer.
    /// </summary>
    public enum TMP_VertexDataUpdateFlags
    {
        None = 0x0,
        Vertices = 0x1,
        Uv0 = 0x2,
        Uv2 = 0x4,
        Uv4 = 0x8,
        Colors32 = 0x10,
        All = 0xFF
    };


    /// <summary>
    /// TMP custom data type to represent 32 bit characters.
    /// </summary>
    //public struct TMP_Char
    //{
    //    private int m_value;

    //    private TMP_Char(int value)
    //    {
    //        this.m_value = value;
    //    }

    //    private TMP_Char(TMP_Char value)
    //    {
    //        this.m_value = (int)value;
    //    }

    //    public static implicit operator TMP_Char(int value)
    //    {
    //        return new TMP_Char(value);
    //    }

    //    public static implicit operator TMP_Char(char c)
    //    {
    //        return new TMP_Char(c);
    //    }

    //    public static explicit operator int(TMP_Char value)
    //    {
    //        return value.m_value;
    //    }

    //    public override string ToString()
    //    {
    //        return m_value.ToString();
    //    }
    //}


    //public struct TMP_VertexInfo
    //{
    //    public TMP_Vertex topLeft;
    //    public TMP_Vertex bottomLeft;
    //    public TMP_Vertex topRight;
    //    public TMP_Vertex bottomRight;
    //}


    [Serializable]
    public struct VertexGradient
    {
        public Color topLeft;
        public Color topRight;
        public Color bottomLeft;
        public Color bottomRight;

        public VertexGradient (Color color)
        {
            this.topLeft = color;
            this.topRight = color;
            this.bottomLeft = color;
            this.bottomRight = color;
        }

        /// <summary>
        /// The vertex colors at the corners of the characters.
        /// </summary>
        /// <param name="color0">Top left color.</param>
        /// <param name="color1">Top right color.</param>
        /// <param name="color2">Bottom left color.</param>
        /// <param name="color3">Bottom right color.</param>
        public VertexGradient(Color color0, Color color1, Color color2, Color color3)
        {
            this.topLeft = color0;
            this.topRight = color1;
            this.bottomLeft = color2;
            this.bottomRight = color3;
        }
    }


    public struct TMP_PageInfo
    {
        public int firstCharacterIndex;
        public int lastCharacterIndex;
        public float ascender;
        public float baseLine;
        public float descender;

        public void Reset()
        {
            firstCharacterIndex = 0;
            lastCharacterIndex = 0;
            ascender = 0;
            baseLine = 0;
            descender = 0;
        }
        // public float extents;
    }


    /// <summary>
    /// Structure containing information about individual links contained in the text object.
    /// </summary>
    public struct TMP_LinkInfo
    {
        public TMP_Text textComponent;

        public int hashCode;

        public int linkIdFirstCharacterIndex;
        public int linkIdLength;
        public int linkTextfirstCharacterIndex;
        public int linkTextLength;

        internal char[] linkID;

        public void Reset()
        {
            textComponent = null;
            hashCode = 0;
            linkIdFirstCharacterIndex = 0;
            linkIdLength = 0;
            linkTextfirstCharacterIndex = 0;
            linkTextLength = 0;
            linkID = null;

    }


        internal void SetLinkID(char[] text, int startIndex, int length)
        {
            if (linkID == null || linkID.Length < length) linkID = new char[length];

            for (int i = 0; i < length; i++)
                linkID[i] = text[startIndex + i];
        }

        /// <summary>
        /// Function which returns the text contained in a link.
        /// </summary>
        /// <param name="textInfo"></param>
        /// <returns></returns>
        public string GetLinkText()
        {
            string text = string.Empty;
            TMP_TextInfo textInfo = textComponent.textInfo;

            for (int i = linkTextfirstCharacterIndex; i < linkTextfirstCharacterIndex + linkTextLength; i++)
                text += textInfo.characterInfo[i].character;

            return text;
        }


        /// <summary>
        /// Function which returns the link ID as a string.
        /// </summary>
        /// <param name="text">The source input text.</param>
        /// <returns></returns>
        public string GetLinkID()
        {
            if (textComponent == null)
                return string.Empty;

            return new string(linkID, 0, linkIdLength);
            //return textComponent.text.Substring(linkIdFirstCharacterIndex, linkIdLength);

        }
    }


    /// <summary>
    /// Structure containing information about the individual words contained in the text object.
    /// </summary>
    public struct TMP_WordInfo
    {
        // NOTE: Structure could be simplified by only including the firstCharacterIndex and length.

        public TMP_Text textComponent;

        public int firstCharacterIndex;
        public int lastCharacterIndex;
        public int characterCount;
        //public float length;

        public void Reset()
        {
            textComponent = null;
            firstCharacterIndex = 0;
            lastCharacterIndex = 0;
            characterCount = 0;

        }

        /// <summary>
        /// Returns the word as a string.
        /// </summary>
        /// <returns></returns>
        public string GetWord()
        {
            string word = string.Empty;
            TMP_CharacterInfo[] charInfo = textComponent.textInfo.characterInfo;

            for (int i = firstCharacterIndex; i < lastCharacterIndex + 1; i++)
            {
                word += charInfo[i].character;
            }

            return word;
        }
    }


    public struct TMP_SpriteInfo
    {
        public int spriteIndex; // Index of the sprite in the sprite atlas.
        public int characterIndex; // The characterInfo index which holds the key information about this sprite.
        public int vertexIndex;
    }


    //public struct SpriteInfo
    //{
    //
    //}


    public struct Extents
    {
        internal static Extents zero = new Extents(Vector2.zero, Vector2.zero);
        internal static Extents uninitialized = new Extents(new Vector2(32767, 32767), new Vector2(-32767, -32767));

        public Vector2 min;
        public Vector2 max;

        public Extents(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public override string ToString()
        {
            string s = "Min (" + min.x.ToString("f2") + ", " + min.y.ToString("f2") + ")   Max (" + max.x.ToString("f2") + ", " + max.y.ToString("f2") + ")";
            return s;
        }
    }


    [Serializable]
    public struct Mesh_Extents
    {
        public Vector2 min;
        public Vector2 max;


        public Mesh_Extents(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public override string ToString()
        {
            string s = "Min (" + min.x.ToString("f2") + ", " + min.y.ToString("f2") + ")   Max (" + max.x.ToString("f2") + ", " + max.y.ToString("f2") + ")";
            //string s = "Center: (" + ")" + "  Extents: (" + ((max.x - min.x) / 2).ToString("f2") + "," + ((max.y - min.y) / 2).ToString("f2") + ").";
            return s;
        }
    }

    // internal struct TMP_TextProcessingState
    // {
    //         // Multi Font & Material support related
    //         public TMP_FontAsset CurrentFontAsset;
    //         public TMP_SpriteAsset CurrentSpriteAsset;
    //         public Material CurrentMaterial;
    //         public int CurrentMaterialIndex;
    //
    //         public float CurrentFontSize;
    //         public float FontScale;
    //         public float FontScaleMultiplier;
    //         public FontStyles FontStyle;
    //         public int ItalicAngle;
    //
    //         public float CharacterSpacing;
    //         public float CharacterMonoSpacing;
    //         public bool TagNoParsing;
    //
    //         public float HorizontalAdvance;
    //         public float MaxCapHeight;
    //         public float MaxTextAscender;
    //         public float MaxTextDescender;
    //         public float MaxElementAscender;
    //         public float MaxElementDescender;
    //         public float StartOfLineAscender;
    //         public float MaxLineAscender;
    //         public float MaxLineDescender;
    //         public float PageAscender;
    //
    //         public int PreviousWordBreak;
    //         public int TotalCharacterCount;
    //         //public int VisibleCharacterCount;
    //         //public int VisibleSpriteCount;
    //         public int VisibleLinkCount;
    //         public int FirstCharacterIndex;
    //         public int FirstVisibleCharacterIndex;
    //         public int LastCharacterIndex;
    //         public int LastVisibleCharIndex;
    //
    //         public int LineNumber;
    //         public float baselineOffset;
    //         public float lineOffset;
    //         public bool isDrivenLineSpacing;
    //         public bool IsNonBreakingSpace;
    //
    //         public HorizontalAlignmentOptions HorizontalAlignment;
    //         public float MarginLeft;
    //         public float MarginRight;
    //
    //         public float PreferredWidth;
    //         public float PreferredHeight;
    //
    //         public Color32 VertexColor;
    //         public Color32 UnderlineColor;
    //         public Color32 StrikethroughColor;
    //         //public Color32 HighlightColor;
    //
    //         public Extents MeshExtents;
    //         public TMP_LineInfo lineInfo;
    //
    //         public int spriteAnimationID;
    //
    //         public TMP_FontStyleStack BasicStyleStack;
    //         public TMP_RichTextTagStack<int> ItalicAngleStack;
    //         public TMP_RichTextTagStack<Color32> ColorStack;
    //         public TMP_RichTextTagStack<Color32> UnderlineColorStack;
    //         public TMP_RichTextTagStack<Color32> StrikethroughColorStack;
    //         public TMP_RichTextTagStack<Color32> HighlightColorStack;
    //         public TMP_RichTextTagStack<HighlightState> HighlightStateStack;
    //         public TMP_RichTextTagStack<TMP_ColorGradient> ColorGradientStack;
    //         public TMP_RichTextTagStack<float> SizeStack;
    //         public TMP_RichTextTagStack<float> IndentStack;
    //         public TMP_RichTextTagStack<FontWeight> FontWeightStack;
    //
    //         public TMP_RichTextTagStack<float> BaselineStack;
    //         //public TMP_RichTextTagStack<int> ActionStack;
    //         public TMP_RichTextTagStack<MaterialReference> MaterialReferenceStack;
    //         public TMP_RichTextTagStack<HorizontalAlignmentOptions> LineJustificationStack;
    // }


    // Structure used for Word Wrapping which tracks the state of execution when the last space or carriage return character was encountered.
    public struct WordWrapState
    {
        public int previous_WordBreak;
        public int total_CharacterCount;
        public int visible_CharacterCount;
        public int visible_SpriteCount;
        public int visible_LinkCount;
        public int firstCharacterIndex;
        public int firstVisibleCharacterIndex;
        public int lastCharacterIndex;
        public int lastVisibleCharIndex;
        public int lineNumber;

        public float maxCapHeight;
        public float maxAscender;
        public float maxDescender;
        public float startOfLineAscender;
        public float maxLineAscender;
        public float maxLineDescender;
        public float pageAscender;

        public HorizontalAlignmentOptions horizontalAlignment;
        public float marginLeft;
        public float marginRight;

        public float xAdvance;
        public float preferredWidth;
        public float preferredHeight;
        //public float maxFontScale;
        public float previousLineScale;

        public int wordCount;
        public FontStyles fontStyle;
        public int italicAngle;
        public float fontScaleMultiplier;

        public float currentFontSize;
        public float baselineOffset;
        public float lineOffset;
        public bool isDrivenLineSpacing;
        public float glyphHorizontalAdvanceAdjustment;

        public float cSpace;
        public float mSpace;

        public TMP_TextInfo textInfo;
        public TMP_LineInfo lineInfo;

        public Color32 vertexColor;
        public Color32 underlineColor;
        public Color32 strikethroughColor;
        public Color32 highlightColor;
        public TMP_FontStyleStack basicStyleStack;
        public TMP_TextProcessingStack<int> italicAngleStack;
        public TMP_TextProcessingStack<Color32> colorStack;
        public TMP_TextProcessingStack<Color32> underlineColorStack;
        public TMP_TextProcessingStack<Color32> strikethroughColorStack;
        public TMP_TextProcessingStack<Color32> highlightColorStack;
        public TMP_TextProcessingStack<HighlightState> highlightStateStack;
        public TMP_TextProcessingStack<TMP_ColorGradient> colorGradientStack;
        public TMP_TextProcessingStack<float> sizeStack;
        public TMP_TextProcessingStack<float> indentStack;
        public TMP_TextProcessingStack<FontWeight> fontWeightStack;
        public TMP_TextProcessingStack<int> styleStack;
        public TMP_TextProcessingStack<float> baselineStack;
        public TMP_TextProcessingStack<int> actionStack;
        public TMP_TextProcessingStack<MaterialReference> materialReferenceStack;
        public TMP_TextProcessingStack<HorizontalAlignmentOptions> lineJustificationStack;
        public int spriteAnimationID;

        public TMP_FontAsset currentFontAsset;
        public TMP_SpriteAsset currentSpriteAsset;
        public Material currentMaterial;
        public int currentMaterialIndex;

        public Extents meshExtents;

        public bool tagNoParsing;
        public bool isNonBreakingSpace;
    }


    /// <summary>
    /// Structure used to store retrieve the name and hashcode of the font and material
    /// </summary>
    public struct TagAttribute
    {
        public int startIndex;
        public int length;
        public int hashCode;
    }


    public struct RichTextTagAttribute
    {
        public int nameHashCode;
        public int valueHashCode;
        public TagValueType valueType;
        public int valueStartIndex;
        public int valueLength;
        public TagUnitType unitType;
    }



    public class ItemPool<T> where T: new()
    {
        static private Stack<T> m_itemPool = new Stack<T>(32);

        static private int DEFAULT_MAX_SIZE = 128;
        //分配一个数组
        static public T AlocItem()
        {
            if(m_itemPool.Count>0)
            {
                T t = m_itemPool.Pop();
                return t;
            }
            return new T();
        }

        //回收一个数组
        static public void RecycleItem(T item)
        {
            if (null == item)
            {
                return;
            }


#if UNITY_EDITOR
            if (m_itemPool.Contains(item))
            {
                Debug.LogError("重复回收数组对象：len=" + item);
            }
#endif

            //先限定只缓存512
            if (m_itemPool.Count < DEFAULT_MAX_SIZE)
                m_itemPool.Push(item);

        }


    }


    public class ListArrayPool<T>
    {
        static private Dictionary<int, List<T[]>> m_dicDicList = new Dictionary<int, List<T[]>>();

        static private int DEFAULT_MAX_SIZE = 512;
        //分配一个数组
        static public T[] AlocArray(int nCount)
        {
            List<T[]> stackArry = null;
            if (m_dicDicList.TryGetValue(nCount, out stackArry))
            {
                if (stackArry.Count > 0)
                {
                    int nInndex = stackArry.Count - 1;
                    T[] v = stackArry[nInndex];
                    stackArry.RemoveAt(nInndex);

                    //假如有null数组,排除空数组
                    if(v==null)
                    {
                        return AlocArray(nCount);
                    }

                    return v;
                }
            }

            return new T[nCount];
        }

        //回收一个数组
        static public void RecycleArray(T[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<T[]> stackArry = null;
            if (m_dicDicList.TryGetValue(nCount, out stackArry) == false)
            {
                stackArry = new List<T[]>();
                m_dicDicList.Add(nCount, stackArry);
            }


#if UNITY_EDITOR
            if (stackArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            //先限定只缓存512
            if(stackArry.Count< DEFAULT_MAX_SIZE)
            stackArry.Add(arry);

        }

        static public void Resize(ref T[] src, int size)
        {
            if (src.Length >= size)
            {
                return;
            }

            T[] dst = AlocArray(size);
            Array.Copy(src, dst, src.Length);
            RecycleArray(src);
            src = dst;
        }

    }


    public class TMPro_MeshUtilities
    {


     




        /*
        //字节缓存
        static private Dictionary<int, List<Vector3[]>> m_dicVector3List = new Dictionary<int, List<Vector3[]>>();
        static private Dictionary<int, List<Vector2[]>> m_dicVector2List = new Dictionary<int, List<Vector2[]>>();
        static private Dictionary<int, List<Vector4[]>> m_dicVector4List = new Dictionary<int, List<Vector4[]>>();
        static private Dictionary<int, List<Color32[]>> m_dicColor32List = new Dictionary<int, List<Color32[]>>();
        static private Dictionary<int, List<int[]>> m_dicIntList = new Dictionary<int, List<int[]>>();
        static private Dictionary<int, List<char[]>> m_dicCharList = new Dictionary<int, List<char[]>>();
        static private Dictionary<int, List<uint[]>> m_dicUintList = new Dictionary<int, List<uint[]>>();

        //分配一个int性质数组
        static public int[] AlocIntArray(int nCount)
        {
            List<int[]> stackIntArry = null;
            if (m_dicIntList.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    int[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new int[nCount];
        }

        //回收一个int 数组
        static public void RecycleIntArray(int[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<int[]> stackIntArry = null;
            if (m_dicIntList.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<int[]>();
                m_dicIntList.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            stackIntArry.Add(arry);

        }


        //分配一个int性质数组
        static public uint[] AlocUintArray(int nCount)
        {
            List<uint[]> stackIntArry = null;
            if (m_dicUintList.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    uint[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new uint[nCount];
        }

        //回收一个int 数组
        static public void RecycleUintArray(uint[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<uint[]> stackIntArry = null;
            if (m_dicUintList.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<uint[]>();
                m_dicUintList.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            stackIntArry.Add(arry);

        }

        //分配一个Vector2性质数组
        static public Vector2[] AlocVector2Array(int nCount)
        {
            List<Vector2[]> stackIntArry = null;
            if (m_dicVector2List.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    Vector2[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new Vector2[nCount];
        }

        //回收一个Vector2 数组
        static public void RecycleVector2Array(Vector2[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<Vector2[]> stackIntArry = null;
            if (m_dicVector2List.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<Vector2[]>();
                m_dicVector2List.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            stackIntArry.Add(arry);

        }


        //分配一个Vector3性质数组
        static public Vector3[] AlocVector3Array(int nCount)
        {
            List<Vector3[]> stackIntArry = null;
            if (m_dicVector3List.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    Vector3[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new Vector3[nCount];
        }

        //回收一个Vector3 数组
        static public void RecycleVector3Array(Vector3[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<Vector3[]> stackIntArry = null;
            if (m_dicVector3List.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<Vector3[]>();
                m_dicVector3List.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            stackIntArry.Add(arry);

        }

        //分配一个Vector4性质数组
        static public Vector4[] AlocVector4Array(int nCount)
        {
            List<Vector4[]> stackIntArry = null;
            if (m_dicVector4List.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    Vector4[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new Vector4[nCount];
        }

        //回收一个Vector4 数组
        static public void RecycleVector4Array(Vector4[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<Vector4[]> stackIntArry = null;
            if (m_dicVector4List.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<Vector4[]>();
                m_dicVector4List.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            stackIntArry.Add(arry);

        }

        static public Color32[] AlocColor32Array(int nCount)
        {
            List<Color32[]> stackIntArry = null;
            if (m_dicColor32List.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    Color32[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new Color32[nCount];
        }

        //回收一个Color32 数组
        static public void RecycleColor32Array(Color32[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<Color32[]> stackIntArry = null;
            if (m_dicColor32List.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<Color32[]>();
                m_dicColor32List.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            stackIntArry.Add(arry);

        }


        //new char
        static public char[] AlocCharArray(int nCount)
        {
            List<char[]> stackIntArry = null;
            if (m_dicCharList.TryGetValue(nCount, out stackIntArry))
            {
                if (stackIntArry.Count > 0)
                {
                    int nInndex = stackIntArry.Count - 1;
                    char[] v = stackIntArry[nInndex];
                    stackIntArry.RemoveAt(nInndex);
                    return v;
                }
            }

            return new char[nCount];
        }

        //char 数组
        static public void RecycleCharArray(char[] arry)
        {
            if (null == arry)
            {
                return;
            }

            int nCount = arry.Length;

            List<char[]> stackIntArry = null;
            if (m_dicCharList.TryGetValue(nCount, out stackIntArry) == false)
            {
                stackIntArry = new List<char[]>();
                m_dicCharList.Add(nCount, stackIntArry);
            }


#if UNITY_EDITOR
            if (stackIntArry.IndexOf(arry) >= 0)
            {
                Debug.LogError("重复回收数组对象：len=" + arry.Length);
            }
#endif

            stackIntArry.Add(arry);

        }

        static public void ResizeVector4Array(ref Vector4[] src,int size)
        {
            if(src.Length>= size)
            {
                return;
            }

            Vector4[] dst= AlocVector4Array(size);
            Array.Copy(src,dst, src.Length);
            RecycleVector4Array(src);
            src = dst;
        }

        static public void ResizeVector3Array(ref Vector3[] src, int size)
        {
            if (src.Length >= size)
            {
                return;
            }

            Vector3[] dst = AlocVector3Array(size);
            Array.Copy(src, dst, src.Length);
            RecycleVector3Array(src);
            src = dst;
        }


        static public void ResizeVector2Array(ref Vector2[] src, int size)
        {
            if (src.Length >= size)
            {
                return;
            }

            Vector2[] dst = AlocVector2Array(size);
            Array.Copy(src, dst, src.Length);
            RecycleVector2Array(src);
            src = dst;
        }

        static public void ResizeColor32Array(ref Color32[] src, int size)
        {
            if (src.Length >= size)
            {
                return;
            }

            Color32[] dst = AlocColor32Array(size);
            Array.Copy(src, dst, src.Length);
            RecycleColor32Array(src);
            src = dst;
        }

        static public void ResizeIntArray(ref int[] src, int size)
        {
            if (src.Length >= size)
            {
                return;
            }

            int[] dst = AlocIntArray(size);
            Array.Copy(src, dst, src.Length);
            RecycleIntArray(src);
            src = dst;
        }

        static public void ResizeUintArray(ref uint[] src, int size)
        {
            if (src.Length >= size)
            {
                return;
            }

            uint[] dst = AlocUintArray(size);
            Array.Copy(src, dst, src.Length);
            RecycleUintArray(src);
            src = dst;
        }
        */

    }

}

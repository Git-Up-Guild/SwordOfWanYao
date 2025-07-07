using UnityEngine;

namespace WXB
{
    /// <summary>
    /// 一行
    /// </summary>
    public class Line
    {
        public Line()
        {
        }

        public void SetSize(Vector2 s)
        {
            size = s;
        }

        //当前行的宽度与高度
        private Vector2 size; 

        //行宽度
        public float x { get { return size.x; } set { size.x = value; } }

        //行高度
        public float y { get { return size.y; } set { size.y = value; } }

        //行尺寸
        public Vector2 s { get { return size; } }

        public void Clear()
        {
            size = Vector2.zero;
        }

        // Y轴修正
        public float minY { get; set; }
        public float maxY { get; set; }

        public override string ToString()
        {
            return string.Format("w:{0} h:{1} minY:{2} maxY:{3} fh:{4}", x, y, minY, maxY, fontHeight);
        }

        public float fontHeight { get { return maxY - minY; } }

    }
}
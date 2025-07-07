using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UGUI2PSD
{
    public class Ugui2Psd_Setting : ScriptableObject
    {
        public int PsdWidth = 1280;
        public int PsdHeight = 720;
        public string OutputBasePath;   //输出psd的基础路径

        public float CanvasX = 360;     //画布坐标x
        public float CanvasY = 640;     //画布坐标y
    }
}

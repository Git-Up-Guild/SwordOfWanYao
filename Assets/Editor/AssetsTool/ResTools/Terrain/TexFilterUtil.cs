/*******************************************************************
** 文件名: TexFilterUtil.cs
** 版  权:    (C) 深圳冰川网络技术有限公司 
** 创建人:     郑秀程
** 日  期:    2016/5/3
** 版  本:    1.0
** 描  述:    纹理采样辅助类
** 应  用:    

**************************** 修改记录 ******************************
** 修改人:  
** 日  期: 
** 描  述: 
********************************************************************/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TexFilterUtil : MonoBehaviour {

    public enum ZFilterMode
    {
        Point,
        Bilinear,
        //暂未实现
        //Trilinear,
    }

    public static float GetFilteredData(ZFilterMode filterMode,Array data, float u, float v,int i=0)
    {
        if (filterMode == ZFilterMode.Point)
        {
            return GetPointFilteredData<float>(data, u, v, i);
        } else
        {
            return GetBilinearFilteredData(data,u,v,i);
        }
    }


    public static float GetBilinearFilteredData(Array data, float u, float v,int i=0) {
        int w = data.GetLength(0);
        int h = data.GetLength(1);
        u = u * w - 0.5f;
        v = v * h - 0.5f;
        int x = Mathf.Clamp(Mathf.RoundToInt(u),0,w-2);
        int y = Mathf.Clamp(Mathf.RoundToInt(v),0,h-2);
        float u_ratio = u - x;
        float v_ratio = v - y;
        float u_opposite = 1 - u_ratio;
        float v_opposite = 1 - v_ratio;
        float result;
        if (data.Rank > 2)
        {
            result = ((float)data.GetValue(x,y,i) * u_opposite  + (float)data.GetValue(x,y,i) * u_ratio) * v_opposite + 
                ((float)data.GetValue(x,y,i) * u_opposite  + (float)data.GetValue(x,y,i) * u_ratio) * v_ratio;
        } else
        {
            result = ((float)data.GetValue(x,y) * u_opposite  + (float)data.GetValue(x,y) * u_ratio) * v_opposite + 
                ((float)data.GetValue(x,y) * u_opposite  + (float)data.GetValue(x,y) * u_ratio) * v_ratio;
        }
        return result;
    }

    public static T GetPointFilteredData<T>(Array data, float u, float v,int i = 0)
    {   

        int w = data.GetLength(0);
        int h = data.GetLength(1);
        int x = Mathf.RoundToInt(w * u);
        int y = Mathf.RoundToInt(h * v);
        if (data.Rank > 2)
        {
            return (T)data.GetValue(x,y,i);
        } else {
            return (T)data.GetValue(x,y);
        }
    }



}

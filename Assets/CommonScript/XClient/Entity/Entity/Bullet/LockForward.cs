/*******************************************************************
** 文件名:	LockForward.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.29
** 版  本:	1.0
** 描  述:	
** 应  用:  锁定朝向

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockForward : MonoBehaviour
{
    //锁定方向
    public Vector3 forward = new Vector3(0,0,1);

    //是否支持方向缩放
    public bool directScale = false;

    //正向朝向是否右方
   // public bool rightDirect = false;

    public Transform lockTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (lockTransform == null)  
            lockTransform = this.transform; 
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 curForward = transform.forward;
        if (curForward != forward)
        {
           
            transform.forward = forward;    


            if(null!= lockTransform)
            {
                Vector3 scale = lockTransform.localScale;
                float absX = Mathf.Abs(scale.x);

                /*
                if(rightDirect)
                {
                    absX = -absX;
                }
                */

                float scaleX = curForward.x < 0 ? absX : -absX;
                if (scaleX != scale.x)
                {
                    scale.x = scaleX;
                    lockTransform.localScale = scale;
                }
            }
           

        }
    }
}

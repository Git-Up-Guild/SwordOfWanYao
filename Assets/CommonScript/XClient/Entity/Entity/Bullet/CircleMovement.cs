/*******************************************************************
** 文件名:	CircleMovement.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.01
** 版  本:	1.0
** 描  述:	
** 应  用:  环绕圆周运动

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XClient.Entity
{
    public class CircleMovement : MonoBehaviour
    {
        // 绕哪个点旋转
        public Transform centerTransform;
        // 圆的半径
        public float radius = 3;
        // 旋转速度
        public float rotationSpeed = 10;

        //记录上一次位置,用来更新朝向
        public Vector3 m_lastPos;

        //移动标识
        private bool m_bMoving = true;

        private void Start()
        {
            m_lastPos = transform.position;
        }

        public void StartMove(Transform centerTransform,float speed, float radius)
        {
            m_bMoving = true;
            rotationSpeed = speed;
            this.radius = radius;
            this.centerTransform = centerTransform; 
        }


        private void Update()
        {
            // 计算圆周运动的新位置
            float angle = Time.time * rotationSpeed; // 使用时间来计算角度
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 newPosition = centerTransform.position + offset;

            // 设置物体的位置和朝向
            transform.position = newPosition;
            transform.forward = (newPosition - m_lastPos).normalized;
            m_lastPos = newPosition;
        }

    }
}


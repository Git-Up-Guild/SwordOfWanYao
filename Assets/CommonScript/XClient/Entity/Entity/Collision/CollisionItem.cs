/*******************************************************************
** 文件名:	CollisionItem.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.28
** 版  本:	1.0
** 描  述:	
** 应  用:  碰撞对象监控脚本

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;

namespace XClient.Entity
{
    public class CollisionItem : MonoBehaviour
    {
        //碰撞后回调
        public ICollisionSink collisionSink;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        // 碰撞事件，当两个物体发生碰撞时被调用
        void OnCollisionEnter(Collision collision)
        {
            Debug.LogError("碰撞事件：" + collision.gameObject.name);
        }

        // 触发器事件，当一个物体进入另一个物体的触发器时被调用
        void OnTriggerEnter(Collider other)
        {
            if (collisionSink == null)
            {
              //  Debug.LogWarning("性能空消耗请检测 进入触发器：" + other.gameObject.name);
                return;
            }

            IDReco reco = other.gameObject.GetComponent<IDReco>();
            if (reco!=null)
            {
                collisionSink.OnCollision(reco);
            }

           // Debug.LogError("进入触发器：" + other.gameObject.name);
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {

        }

        /*
        private void OnCollisionExit2D(Collision2D collision)
        {
          
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
         
        }
        */

        private void OnTriggerEnter2D(Collider2D collision)
        {

        }

        /*
        private void OnTriggerExit2D(Collider2D collision)
        {

           
        }

        private void OnTriggerStay2D(Collider2D collision)
        {

            
        }
        */
    }

}

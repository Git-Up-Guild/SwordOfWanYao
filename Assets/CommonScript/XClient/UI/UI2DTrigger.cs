/*******************************************************************
** 文件名:	
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	许德纪
** 日  期:	2020/9/24 11:55:34
** 版  本:	1.0
** 描  述:	
** 应  用:  2D触发器检测

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace XClient.UI
{
    public class UI2DTrigger : MonoBehaviour
    {
        public Rigidbody2D rigidbody2D;
        public Collider2D collider2D;

        private UnityAction<string> onTriggerrStayCallback;
        private UnityAction<string> onTriggerEnterCallback;
        private UnityAction<string> onTriggerExitCallback;

        private string onTriggerrStayRootName;
        private string onTriggerEnterRootName;
        private string onTriggerExitRootName;

        void Awake()
        {
            if (rigidbody2D == null)
            {
                rigidbody2D = GetComponent<Rigidbody2D>();
            }

            if (rigidbody2D == null)
            {
                Debug.LogError("没有添加Rigidbody2D，name:" + transform.name);
                return;
            }

            if (collider2D == null)
            {
                collider2D = GetComponent<Collider2D>();
            }

            if (collider2D == null)
            {
                Debug.LogError("没有添加Collider2D，name:" + transform.name);
                return;
            }

            rigidbody2D.simulated = true;
            rigidbody2D.useAutoMass = false;
            rigidbody2D.mass = 1;
            rigidbody2D.angularDrag = 0;
            rigidbody2D.gravityScale = 0;
            collider2D.isTrigger = true;
        }

        private void OnMouseUp()
        {
            Debug.LogError("UI2DTrigger>> OnMouseUp");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (onTriggerEnterCallback != null)
            {
                onTriggerEnterCallback.Invoke(collision.name);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (onTriggerExitCallback != null)
            {
                onTriggerExitCallback.Invoke(collision.name);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (onTriggerrStayCallback != null)
            {
                onTriggerrStayCallback.Invoke(collision.name);
            }
        }

        public void AddOnTriggerEnterListener(UnityAction<string> func, string rootName = null)
        {
            if (onTriggerEnterCallback == null)
            {
                onTriggerEnterRootName = rootName;
                onTriggerEnterCallback = func;
            }
            else
            {
                Debug.LogError("重复注册TriggerEnter，name:" + transform.name);
            }
        }

        public void RemoveOnTriggerEnterListener()
        {
            if (onTriggerEnterCallback != null)
            {
                onTriggerEnterCallback = null;
            }
            onTriggerEnterRootName = null;
        }

        public void AddOnTriggerExitListener(UnityAction<string> func, string rootName = null)
        {
            if (onTriggerExitCallback == null)
            {
                onTriggerExitRootName = rootName;
                onTriggerExitCallback = func;
            }
            else
            {
                Debug.LogError("重复注册TriggerExit，name:" + transform.name);
            }
        }

        public void RemoveOnTriggerExitListener()
        {
            if (onTriggerExitCallback != null)
            {
                onTriggerExitCallback = null;
            }
            onTriggerExitRootName = null;
        }

        public void AddOnTriggerStayListener(UnityAction<string> func, string rootName = null)
        {
            if (onTriggerrStayCallback == null)
            {
                onTriggerrStayRootName = rootName;
                onTriggerrStayCallback = func;
            }
            else
            {
                Debug.LogError("重复注册TriggerStay，name:" + transform.name);
            }
        }

        public void RemoveOnTriggerStayListener()
        {
            if (onTriggerrStayCallback != null)
            {
                onTriggerrStayCallback = null;
            }
            onTriggerrStayRootName = null;
        }


        public void OpenTrigger()
        {
            this.enabled = true;
            if (collider2D != null)
            {
                collider2D.enabled = true;
            }
        }

        public void CloseTrigger()
        {
            this.enabled = false;
            if (collider2D != null)
            {
                collider2D.enabled = false;
            }
        }

        /// <summary>
        /// 返回树型名字
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="rootName"></param>
        /// <returns></returns>
        private string GetGameObjectPath(Transform transform, string rootName = null)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (string.IsNullOrEmpty(rootName))
                {
                    path = transform.name + "/" + path;
                }
                else
                {
                    if (transform.name == rootName)
                    {
                        break;
                    }
                    else
                    {
                        path = transform.name + "/" + path;
                    }
                }
            }
            return path;
        }
    }
}

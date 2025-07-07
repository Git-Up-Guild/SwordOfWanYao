/*******************************************************************
** 文件名:	 ElementCache.cs
** 版  权:	(C) 深圳冰川网络科技有限公司
** 创建人:	许德纪
** 日  期:	2025/02/21 10:45:36
** 版  本:	1.0
** 描  述:	
** 应  用:  临时存储GameObject, 让他的生存期和这个脚本的父亲一致

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XGame
{
    public class ElementCache : MonoBehaviour
    {
        List<GameObject> cacheElement = new List<GameObject>();
        // Start is called before the first frame update
        void Start()
        {

        }

        //增加一个对象到缓存
        public void Recycle(GameObject element)
        {
            if(element==null)
            {
                return; 
            }

            if (element.transform.parent!=this.transform)
                element.transform.BetterSetParent(this.transform);

            if(element.activeSelf)
            {
                element.BetterSetActive(false);
            }

            cacheElement.Add(element);  
        }

        //从缓存中提取一个对象
        public GameObject Alloc(GameObject porto,Transform parent = null)
        {
            if(cacheElement.Count>0)
            {
                GameObject element = cacheElement[cacheElement.Count-1];
                cacheElement.RemoveAt(cacheElement.Count-1);

                if (parent!=null&&element.transform.parent != parent)
                    element.transform.BetterSetParent(parent);


                return element;
            }

            return GameObject.Instantiate(porto, parent);   
        }

      
    }
}



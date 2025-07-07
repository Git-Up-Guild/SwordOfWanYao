/*******************************************************************
** 文件名:	FrameEventMgr.cs
** 版  权:	(C) 
** 创建人:	许德纪
** 日  期:	2025/3/28
** 版  本:	1.0
** 描  述:	
** 应  用:  简单的帧回调管理对象

********************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    public class FrameEventMgr : MonoBehaviour
    {
        //正在运行的帧事件回调队列
        // private HashSet<IFrameSink> frameSinks = new HashSet<IFrameSink>();

        //等待删除队列
        //private HashSet<IFrameSink> waitDelFrameSinks = new HashSet<IFrameSink>();

        //HashSet 产生GC，尝试用 List， ListRemove 特殊处理
        private List<IFrameSink> frameSinks = new List<IFrameSink>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            RemoveFrameSinkImp();


            int nCount = frameSinks.Count;
            for(int i=0;i<nCount;++i)
            {
                if (frameSinks[i]!=null)
                {
                    if(frameSinks[i].OnUpdate()==false)
                    {
                        //需要删除的，设置成 null
                        frameSinks[i] = null;
                    }
                    
                }
            }

            /*
            foreach (var sink in frameSinks)
            {

                if(sink==null)
                {
                    frameSinks.Remove(sink);
                    return;
                }

                if(sink.OnUpdate()==false)
                {
                    waitDelFrameSinks.Add(sink);
                }
            }*/

            RemoveFrameSinkImp();
        }

        public void AddFrameSink(IFrameSink sink)
        {
            if(null== sink)
            {
                return;
            }

            if(frameSinks.Contains(sink)==false)
                frameSinks.Add(sink);

           // waitDelFrameSinks.Remove(sink);
        }

        public void RemoveFrameSink(IFrameSink sink)
        {
            if (null == sink)
            {
                return;
            }


            int index = frameSinks.IndexOf(sink);
            if(index>=0)
            {
                frameSinks[index] = null;
            }

            /*
            if(waitDelFrameSinks.Contains(sink)==false)
                waitDelFrameSinks.Add(sink);
            */

        }

        private void RemoveFrameSinkImp()
        {


            int nCount = frameSinks.Count;
            for (int i = nCount-1; i>=0; --i)
            {
                if (frameSinks[i] == null)
                {
                    frameSinks.RemoveAt(i);

                }else //遇到不是null， 先跳过，等待下次完成
                {
                    break;
                }
            }


            /*
            if (waitDelFrameSinks.Count == 0)
                return;

            foreach(var sink in waitDelFrameSinks)
            {
                frameSinks.Remove(sink);
            }

            waitDelFrameSinks.Clear();  
            */
        }
    }
}



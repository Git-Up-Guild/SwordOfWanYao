/*****************************************************
** 文 件 名：UHyperTextFactory
** 版    本：V1.0
** 创 建 人：郑秀程
** 创建日期：2020/7/14 12:09:28
** 内容简述：
** 修改记录：
日期	版本	修改人	修改内容   
*****************************************************/

using UnityEngine;
//using XGame.Poolable;

namespace WXB
{
    /// <summary>
    /// 对象工厂
    /// </summary>
    public static class UHyperTextFactory
    {
        public static Line CreateLine(Vector2 size)
        {

            return new Line();
            /*
            var poolManager = XGameComs.Get<IItemPoolManager>();
            if (poolManager == null)
            {
                var l = new Line();
                l.SetSize(size);
                return l;
            }
            else
            {
                var l = poolManager.PopObjectItem<Line>();
                l.SetSize(size);
                return l;
            }*/
        }

        public static void ReleaseLine(Line line)
        {
            /*
            var poolManager = XGameComs.Get<IItemPoolManager>();
            if (poolManager != null)
            {
                line.Clear();
                poolManager.PushObjectItem(line);
            }
            */

           
        }

        public static T CreateNode<T>(IOwner owner, Anchor anchor) where T : NodeBase, new()
        {
            /*
            var poolManager = XGameComs.Get<IItemPoolManager>();
            if(poolManager == null)
            {
                T t = new T();
                t.Reset(owner, anchor);
                return t;
            }
            else
            {
                var t = poolManager.PopObjectItem<T>();
                t.Reset(owner, anchor);
                return t;
            }
            */

            return null;
        }

        public static void ReleaseNode(NodeBase node)
        {
            if (node == null)
            {
                Debug.LogErrorFormat("FreeNode error! node == null");
                return;
            }

            /*
            var poolManager = XGameComs.Get<IItemPoolManager>();
            if (poolManager == null)
            {
                node.Release();
            }
            else
            {
                node.Release();
                poolManager.PushObjectItem(node);
            }
            */
        }
    }
}

/*******************************************************************
** 文件名:	XGameSink.cs
** 版  权:	(C) 深圳冰川网络网络科技有限公司
** 创建人:	郑秀程 
** 日  期:	2019/10/30
** 版  本:	1.0
** 描  述:	对接XGame
** 应  用:  	
	
**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using XGame.FlowText;
using XGame.UnityObjPool;
using UnityEngine;

namespace Game
{
    public class XGameSink : IFlowTextManagerSink , IUnityObjectPoolSinkWithObj
    {
        public void Create()
        {
        }

        public void Release()
        {
        }

        public uint LoadFlowTextPrefab(string path, IFlowTextPrefabResSink sink)
        {
            IUnityObjectPool pool = XGame.XGameComs.Get<IUnityObjectPool>();
            uint handle = pool.LoadRes<GameObject>(path, sink, this, true);
            return handle;
        }

        public void OnUnityObjectLoadCancel(uint handle, object ud)
        {
            if (ud is IFlowTextPrefabResSink)
            {
                IFlowTextPrefabResSink sink = ud as IFlowTextPrefabResSink;
                sink.OnPrefabLoadCancel(handle);
            }
        }

        public void OnUnityObjectLoadComplete(UnityEngine.Object res, uint handle, object ud)
        {
            if(ud is IFlowTextPrefabResSink)
            {
                IFlowTextPrefabResSink sink = ud as IFlowTextPrefabResSink;
                sink.OnPrefabLoadComplete(res as GameObject, handle);
            }
        }

        public void UnloadFlowTextPrefab(uint handle)
        {
            IUnityObjectPool pool = XGame.XGameComs.Get<IUnityObjectPool>();
            //飘字延后180帧再释放，提高复用率
            pool.UnloadRes(handle,true,true,false,true,180);
        }
    }
}

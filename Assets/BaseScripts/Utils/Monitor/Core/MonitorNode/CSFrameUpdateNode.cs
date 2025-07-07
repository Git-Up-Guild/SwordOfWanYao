/************************************************************************************
* Copyright (c) 2020 All Rights Reserved.
*命名空间：Assets.Scripts.Monitor.Core.MonitorNode
*文件名： CSFrameUpdateNode.cs
*创建人： 敖贤文
*创建时间：2020/7/29 15:59:25 
*描述
*=====================================================================
*修改标记
*修改时间：
*修改人：
*描述：
************************************************************************************/

using XGame.FrameUpdate;
using System;
using System.Collections.Generic;
using XGame;

namespace XClient.Scripts.Monitor
{
    public class CSFrameUpdateManagerNode : MonitorNodeBase
    {
        public override MonitorNodeData SnapShot()
        {
            //同desc为同脚本内注册信息，算在一起
            MonitorNodeData result = new MonitorNodeData();
            IFrameUpdateManager frameUpdateManager = XGameComs.Get<IFrameUpdateManager>();
            Dictionary<string, int> data = new Dictionary<string, int>();
            if (frameUpdateManager != null)
            {
                Dictionary<Action, MonitorInfo> info = frameUpdateManager.GetCheckInfo();
                foreach (var kv in info)
                {
                    if(!data.ContainsKey(kv.Value.Desc))
                    {
                        data.Add(kv.Value.Desc, kv.Value.Count);
                    }
                    else
                    {
                        data[kv.Value.Desc] += kv.Value.Count;
                    }
                }
                foreach (var item in data)
                {
                    result.Add(new MonitorDataUnit(item.Key, item.Value));
                }
                return result;
            }
            return MonitorNodeData.Invalid;
        }
    }
}

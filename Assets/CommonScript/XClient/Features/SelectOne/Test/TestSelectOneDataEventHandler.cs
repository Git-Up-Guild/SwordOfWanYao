using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XClient.Common;

namespace XClient.SelectOne.Test
{
    public class TestSelectOneDataEventHandler : MonoBehaviour
    {
        public void OnSelect(object target, int idx)
        {
            GameGlobal.FlowTextManager.AddFlowText(0, $"你选择了第{idx}项", 0, 0);
        }
    }
}

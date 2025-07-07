/*******************************************************************
** 文件名:	BroadcastFlowText.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.9.04
** 版  本:	1.0
** 描  述:	
** 应  用:  向前移动效果

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XClient.Network;
using XGame.FlowText;
using static XClient.Entity.BroadcastFlowText;

namespace XClient.Entity
{
    public class BroadcastFlowText : NetObjectBehaviour<BroadcastFlowTextData>
    {

        public class BroadcastFlowTextData : MonoNetObject
        {
            //飘字的版本号
            public NetVarLong m_version;

            //移动的目标点
            public NetVarFloatArray m_FlowContext;



            protected override void OnSetupVars()
            {
               // IsDebug = true;

                //IsDebug = true;
                m_version = SetupVar<NetVarLong>("m_version", true, true);
                m_FlowContext = SetupVar<NetVarFloatArray>("m_FlowContext", true, true);

            }
        }

        //上一个版本号
        private long m_lastVersion;


        //飘子现场
        static private FlowTextContext m_flowContext = new FlowTextContext();
        static private Camera mainCamera = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        protected override void  OnUpdate()
        {
            if (m_lastVersion != NetObj.m_version.Value)
            {

                if (NetObj.IsOwner)
                {
                    NetObj.SyncImmediately();
                }

               

                if (null == mainCamera)
                {
                    Camera[] allCameras = Camera.allCameras;
                    int nLen = allCameras.Length;
                    for (int i = 0; i < nLen; ++i)
                    {
                        if (allCameras[i].tag == "MainCamera")
                        {
                            mainCamera = allCameras[i];
                            break;
                        }
                    }

                }

                m_lastVersion = NetObj.m_version.Value;
                List<float> listContext = NetObj.m_FlowContext.Value;
                int nCount = listContext.Count;
                for (int i = 0; (i + 5) <= nCount; i += 5)
                {
                    int textID = (int)listContext[i];
                    int hp = (int)listContext[i + 1];
                    Vector3 pos = new Vector3(listContext[i + 2], listContext[i + 3], listContext[i + 4]);

                    //远程对象同步过来的
                    if(NetObj.IsOwner==false)
                    {
                        pos = MonsterSystem.Instance.WorldPositionToBattlePosition(pos,false);
                    }

                    IFlowTextManager flowTextMgr = XGame.XGameComs.Get<IFlowTextManager>();
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(pos);
                    m_flowContext.content = hp.ToString();
                    m_flowContext.startPosition = flowTextMgr.ScreenPositionToLayerLocalPosition(textID, screenPos, mainCamera);
                    flowTextMgr.AddFlowText(textID, m_flowContext);


                }

                //清除内容
                listContext.Clear();
            }
        }



        public void FlowDamageText(int textID, Vector3 pos, int hp)
        {

            NetObj.m_FlowContext.Value.Add(textID);
            NetObj.m_FlowContext.Value.Add(hp);
            NetObj.m_FlowContext.Value.Add(pos.x);
            NetObj.m_FlowContext.Value.Add(pos.y);
            NetObj.m_FlowContext.Value.Add(pos.z);

            NetObj.m_FlowContext.SetDirty();
            NetObj.m_version.Value = NetObj.m_version.Value + 1;
        }
    }

}

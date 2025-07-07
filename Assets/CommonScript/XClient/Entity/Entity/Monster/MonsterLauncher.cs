/*******************************************************************
** 文件名:	MonsterLaucher.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.25
** 版  本:	1.0
** 描  述:	
** 应  用:  刷怪发射器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.EventEngine;
using XGame;

namespace XClient.Entity
{
    //区域刷新的方式
    public enum REFRESH_RECT_TYPE
    {
        RECT_TYPE_CIRCLE,
        RECT_TYPE_RECT,
    }


    //参数类型
    public enum PARAM_TYPE
    {
        PARAM_TYPE_COUNT,
        PARAM_TYPE_PROP,
    }


    public class MonsterLauncher : MonoBehaviour, IEventExecuteSink
    {
        //怪物阵营
        [Header("阵营属性")]
        public ulong camp = 1;

        //怪物ID
        [Header("怪物ID列表")]
        public List<int>  listMonsterIDs;

        [Header("刷新参数,参考PARAM_TYPE")]
        public List<int> listRefreshParam;

        [Header("刷新参数类型")]
        public PARAM_TYPE paramType = PARAM_TYPE.PARAM_TYPE_COUNT;

        //刷怪间隔
        [Header("刷怪间隔")]
        public float refresh_Interval = 5.0f;

        //刷怪次数,-1代表无限次
        [Header("刷怪次数,-1代表无限次")]
        public int refresh_ExcuteCount = -1;

        //刷怪上限
        [Header("怪物上限,-1代表无限制")]
        public int limit_MonsterCount = 100;

        //每次刷怪个数
        [Header("每次刷怪个数")]
        public int refresh_MonsterCount = 10;

        //每次刷怪个数
        [Header("刷新范围方式")]
        public REFRESH_RECT_TYPE rectType = REFRESH_RECT_TYPE.RECT_TYPE_CIRCLE;

        [Header("友方的阵营类别")]
        public List<ulong> listFriendCamps;

        [Header("敌方阵营列表,不填情况除了己方阵营,都是敌人")]
        public List<ulong> listEnemyCamps;

        //是否随机位置
        public bool bRandomPos = true;

        //固定刷新位置
        public Vector3 refreshPos;


        //已经刷新了的次数
        private int m_refreshCount;

        //上次刷新时间
        private float m_lastRefreshTime = 0;

        //位置信息
        private List<Vector3> m_listPos = new List<Vector3>();

        //创建的生物
        private HashSet<ulong> m_hashEntIDs = new HashSet<ulong>();

        //刷新个数
        private List<int> m_refreshMonsters = new List<int>();

        //单次刷怪的临时列表
        private List<IMonster> m_listMonster = new List<IMonster>();

        // Start is called before the first frame update
        void Start()
        {
            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine?.Subscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "MonsterLauncher:Start");// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
        }

        // Update is called once per frame
        void Update()
        {
            //到达了刷新上限
            if (MonsterSystem.Instance == null || GameGlobal.Role == null || refresh_ExcuteCount >= 0 && refresh_ExcuteCount <= m_refreshCount ||
                limit_MonsterCount >= 0 && limit_MonsterCount <= m_hashEntIDs.Count)
            {
                return;
            }

            float curTime = Time.realtimeSinceStartup;
            if(curTime- m_lastRefreshTime> refresh_Interval)
            {
                ++m_refreshCount;
                m_lastRefreshTime = curTime;
                RefreshMonster();
            }

           
        }

        public List<IMonster> RefreshMonster()
        {
            Vector3 pos = this.transform.position;
            Vector3 forward = this.transform.forward;

            IMonster monster = null;
            m_listMonster.Clear();
            //计算刷怪
            __CalcRefreshMonsters();

            int nCount = m_refreshMonsters.Count;
            __RectOrder(nCount);
            for(int i=0;i< nCount; ++i)
            {
                if(m_hashEntIDs.Count>= limit_MonsterCount)
                {
                    break;
                }

                // m_listPos[i]
                //pos.y = 0;

                CreateMonsterContext createContext = MonsterSystem.s_CreateContext;
                createContext.listFriendCamps = listFriendCamps;
                createContext.listEnemyCamps = listEnemyCamps;
                createContext.camp = camp;
                createContext.creatureID = m_refreshMonsters[i];
                createContext.displayID = m_refreshMonsters[i];
                createContext.forward = forward;
                createContext.pos = m_listPos[i];
                monster = MonsterSystem.Instance.CreateMonster(createContext);

                //monster = MonsterSystem.Instance.CreateMonster(m_refreshMonsters[i], m_listPos[i], forward,camp, listFriendCamps, listEnemyCamps);
                m_hashEntIDs.Add(monster.id);
                m_listMonster.Add(monster);
            }

            return m_listMonster;

        }


       // System.Random seedRd = new System.Random();
        private void __RectOrder(int nCount)
        {
            m_listPos.Clear();

            //固定位置刷新
            if (bRandomPos==false)
            {
                for (int i = 0; i < nCount; ++i)
                {
                    m_listPos.Add(refreshPos);
                }
                return  ;
            }
            
            Vector3 vPos = this.transform.position;
 

           // int nCount = refresh_MonsterCount;
     

            Vector3 vScale = this.transform.localScale;
            Vector3 targetPos = vPos;

            //int nConcentratedCount = (int)(concentratedPer * nCount);
            float scale = 1.0f;//Mathf.Lerp(0.2f, 0.7f, ((float)nConcentratedCount) / 50.0f);
            float nSizeX = (vScale.x)/2;
            float nScaleSizeX = (nSizeX * scale);
            float nSizeZ = vScale.z/2;

            //圆形的以X为准
            if(rectType==REFRESH_RECT_TYPE.RECT_TYPE_CIRCLE)
            {
                nSizeZ = nScaleSizeX;
            }


            for (int i = 0; i < nCount; ++i)
            {

                targetPos.x = vPos.x+Random.Range(-nScaleSizeX, nScaleSizeX);
                targetPos.z = vPos.z+Random.Range(-nSizeZ, nSizeZ);

                m_listPos.Add(targetPos);
            }


        }

        private void __CalcRefreshMonsters()
        {
            m_refreshMonsters.Clear();
            switch(paramType)
            {
                case PARAM_TYPE.PARAM_TYPE_COUNT:
                    {
                        int nMonsterID = 0;
                        int nMonsterCount = 0;
                        int nCount = listMonsterIDs.Count;
                        for(int i=0;i< nCount;++i)
                        {
                            nMonsterID = listMonsterIDs[i];
                            nMonsterCount = 1;
                            if(listRefreshParam.Count>i)
                            {
                                nMonsterCount = listRefreshParam[i];
                            }
                            //添加怪物个数
                            for(int j=0;j< nMonsterCount;++j)
                            {
                                m_refreshMonsters.Add(nMonsterID);
                            }
                        }
                        
                    }
                    break;
                case PARAM_TYPE.PARAM_TYPE_PROP:
                    {
                        int sumProp = GetPropSum();

                        int nMonsterID = 0;
                        for (int i = 0; i < refresh_MonsterCount; ++i)
                        {
                            nMonsterID = RandmonOneMonster(sumProp);
                            if(nMonsterID>0)
                            {
                                m_refreshMonsters.Add(nMonsterID);
                            }
                            
                        }
                    }
                    break;
                default:
                    break;


            }
        }

        //随机刷取个怪物
        private int RandmonOneMonster(int sumProp)
        {
            int curSum = 0;
            int prop = Random.Range(0, sumProp);
            int nCount = listRefreshParam.Count;
            for (int i = 0; i < nCount; ++i)
            {
                curSum += listRefreshParam[i];
                if(curSum> prop)
                {
                    if(listMonsterIDs.Count>i)
                    {
                        return listMonsterIDs[i];
                    }
                    Debug.LogError("怪物ID列表和概率个数不对应，请检查");
                    break;
                }
            }

            return 0;
        }


        //获取总概率
        private int GetPropSum()
        {
            int sum = 0;    
            int nCount = listRefreshParam.Count;
            for (int i=0;i<nCount;++i)
            {
                sum += listRefreshParam[i];
            }

            return sum;
        }

        private void OnDestroy()
        {
            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            if (eventEngine != null)
            {
                eventEngine.UnSubscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0);// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
            }
           
        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            IMonster monster = pContext as IMonster;
            if (null != monster)
            {
                ulong entID = monster.id;
                m_hashEntIDs.Remove(entID);

            }
        }
    }

}


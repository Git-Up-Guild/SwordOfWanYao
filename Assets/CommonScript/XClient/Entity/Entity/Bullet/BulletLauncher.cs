/*******************************************************************
** 文件名:	BulletLauncher.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.6.27
** 版  本:	1.0
** 描  述:	
** 应  用:  子弹发射器发射器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Network;
using XGame;
using XGame.Entity;

namespace XClient.Entity
{
    //地图相关配置
    [Serializable]
    public class BulletItem
    {
        //子弹ID
        public int bulletID;

        //发射间隔
        public float cooldingTime = 2;
    }

    //发射方式
    public enum BULLET_FIRE_TYPE
    {
        FIRE_SINGLE,
        FIRE_FAN,
    }


    public class BulletLauncher : MonoBehaviour
    {
        //发射器的环境提供者
        public IBulletEnvProvider bulletEnvProvider;

        //阵营提供者
        public IDReco IDReco;

        //子弹发射的根部
        public Transform root;

        //发射方式
        public BULLET_FIRE_TYPE fireType = BULLET_FIRE_TYPE.FIRE_SINGLE;

        //发射个数
        public int count = 1;

        //间隔时间
        public float interval = 0;

        //角度范围
        public float AngeleRange = 120;

        //是否随机选取角度
        public bool randomAngle = false;

        //偏移值
        public float startDistance = 0.0f;

        //子弹列表
        public List<BulletItem> bulletItems;

        //记录上次发射时间
        private List<float> listCooldingTime = new List<float>();

        //发射器的网络对象ID
        //private ulong netLauncherObjectID = 0;
        private NetRecoObject netLauncherObject;


        // Start is called before the first frame update
        void Awake()
        {
            //没有环境提供器的，就使用默认的
            if (bulletEnvProvider == null)
            {
                bulletEnvProvider = BulletEnvProviderDefault.Instance;
            }

            if (root == null)
            {
                root = this.transform;
            }

            netLauncherObject = root.GetComponent<NetRecoObject>();
        }

        // Update is called once per frame
        void Update()
        {
            //发射器所属实体，没有攻击能力的，不发射
            if (IDReco != null && IDReco.canAttack == false)
            {
                return;
            }


            int nCount = bulletItems.Count;
            while (listCooldingTime.Count < nCount)
            {
                listCooldingTime.Add(0);
            }


            BulletItem bulletItem = null;
            float curTime = Time.realtimeSinceStartup;

            for (int i = 0; i < nCount; ++i)
            {
                bulletItem = bulletItems[i];
                if (curTime - listCooldingTime[i] >= bulletItem.cooldingTime)
                {
                    //更新冷却时间
                    listCooldingTime[i] = curTime;
                    FireBullet(0, bulletItem.bulletID);
                }
            }
        }

        //发射一个子弹
        public bool FireBullet(int userData, int bullectID)
        {
            switch (fireType)
            {
                case BULLET_FIRE_TYPE.FIRE_FAN:
                    return __OnFireFan(userData, bullectID);
                default:
                    return __OnFireDefault(userData, bullectID);
            }
            //return false;
        }

        private bool __OnFireFan(int userData, int bullectID)
        {
            bool bRet = false;
            BulletFireContext mainFireContext = null;
            //float curTime = Time.realtimeSinceStartup;
            // if (bulletEnvProvider.CanFire(bullectID))
            {
                ulong srcID = IDReco != null ? IDReco.entID : 0;

                //for (int i = 0; i < count; ++i)
                {
                    //构建发射现场
                    mainFireContext = bulletEnvProvider.BuildFireContext(srcID, bullectID, root, userData);

                    //发射器的网络ID
                    mainFireContext.netLauncherObjectID =
                        netLauncherObject != null ? netLauncherObject.NetObj.NetID : 0;

                    //发射一个子弹
                    mainFireContext.bulletEnvProvider = bulletEnvProvider;

                    //延时时间
                    mainFireContext.delayduration = 0;

                    //设置阵营
                    if (null != IDReco)
                    {
                        mainFireContext.src = srcID;
                        mainFireContext.camp = IDReco.camp;
                        mainFireContext.listFriendCamps = IDReco.listFriendCamps;
                        mainFireContext.listEnemyCamps = IDReco.listEnemyCamps;

                        //初始化目标信息
                        if (__InitTargetInfo(mainFireContext, userData, bullectID, IDReco) == false)
                        {
                            return false;
                        }


                        //子弹基础偏移
                        if (startDistance > 0)
                        {
                            Vector3 forward = mainFireContext.targetPos - mainFireContext.srcPos;
                            forward.y = 0;
                            mainFireContext.srcPos += forward * startDistance;
                        }

                        //随机角度
                        if (randomAngle)
                        {
                        }
                    }


                    if (mainFireContext.target != 0)
                    {
                        ICreatureEntity entity = GameGlobal.EntityWorld.Local.GetEntity(mainFireContext.target) as ICreatureEntity;
                        if (null != entity)
                        {
                            IDReco target = entity.GetComponent<IDReco>();
                            if (null != target)
                            {
                                mainFireContext.targetPos = target.transform.position;
                            }
                        }
                    }


                    bRet |= BulletSystem.Instance.CreateBullet(bullectID, mainFireContext) != null;

                    //主体发射成功了，发射从体
                    if (bRet && count > 1)
                    {
                        Vector3 mainForward = mainFireContext.targetPos - mainFireContext.srcPos;
                        mainForward.y = 0;
                        mainForward = mainForward.normalized;
                        float angleStep = AngeleRange / (count - 1);
                        float distance = Vector3.Distance(mainFireContext.targetPos, mainFireContext.srcPos);

                        BulletFireContext subFireContext = null;
                        int subCount = count - 1;
                        for (int i = 0; i < subCount; ++i)
                        {
                            //构建发射现场
                            subFireContext = bulletEnvProvider.BuildFireContext(srcID, bullectID, root, userData);

                            //发射器的网络ID
                            subFireContext.netLauncherObjectID =
                                netLauncherObject != null ? netLauncherObject.NetObj.NetID : 0;

                            //发射一个子弹
                            subFireContext.bulletEnvProvider = bulletEnvProvider;

                            //延时时间
                            subFireContext.delayduration = interval * (i + 1);

                            //设置阵营
                            if (null != IDReco)
                            {
                                subFireContext.src = srcID;
                                subFireContext.camp = IDReco.camp;
                                subFireContext.listFriendCamps = IDReco.listFriendCamps;
                                subFireContext.listEnemyCamps = IDReco.listEnemyCamps;

                                float angle = angleStep * (i / 2 + 1);
                                if (i % 2 == 0)
                                {
                                    angle = -angle;
                                }

                                subFireContext.srcPos = mainFireContext.srcPos;
                                Vector3 forward = __CalcTurnForward(mainForward, angle);

                                //子弹基础偏移
                                if (startDistance > 0)
                                {
                                    subFireContext.srcPos += forward * startDistance;
                                }

                                //初始化目标信息
                                subFireContext.target = 0;
                                subFireContext.targetPos = subFireContext.srcPos + forward * distance;

                                //发射从子弹
                                BulletSystem.Instance.CreateBullet(bullectID, subFireContext);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("没有多余的子弹需要排除");
                    }


                    return bRet;
                }
            }
        }

        private Vector3 __CalcTurnForward(Vector3 srcForward, float angle)
        {
            Vector3 eulerAngles = Quaternion.LookRotation(srcForward).eulerAngles;
            eulerAngles.y += angle;

            return Quaternion.Euler(eulerAngles) * Vector3.forward;
        }

        private bool __OnFireDefault(int userData, int bullectID)
        {
            bool bRet = false;
            BulletFireContext fireContext = null;
            //float curTime = Time.realtimeSinceStartup;
            // if (bulletEnvProvider.CanFire(bullectID))
            {
                ulong srcID = IDReco != null ? IDReco.entID : 0;

                for (int i = 0; i < count; ++i)
                {
                    //构建发射现场
                    fireContext = bulletEnvProvider.BuildFireContext(srcID, bullectID, root, userData);

                    //发射器的网络ID
                    fireContext.netLauncherObjectID = netLauncherObject != null ? netLauncherObject.NetObj.NetID : 0;

                    //发射一个子弹
                    fireContext.bulletEnvProvider = bulletEnvProvider;

                    //延时时间
                    fireContext.delayduration = interval * i;

                    //设置阵营
                    if (null != IDReco)
                    {
                        fireContext.src = srcID;
                        fireContext.camp = IDReco.camp;
                        fireContext.listFriendCamps = IDReco.listFriendCamps;
                        fireContext.listEnemyCamps = IDReco.listEnemyCamps;

                        //初始化目标信息
                        if (__InitTargetInfo(fireContext, userData, bullectID, IDReco) == false)
                        {
                            return false;
                        }


                        //子弹基础偏移
                        if (startDistance > 0)
                        {
                            Vector3 forward = fireContext.targetPos - fireContext.srcPos;
                            forward.y = 0;
                            fireContext.srcPos += forward.normalized * startDistance;
                           // fireContext.srcLocalPos += forward * startDistance;
                        }else
                        {
                            
                        }

                        //随机角度
                        if (randomAngle)
                        {
                        }
                    }


                    bRet |= BulletSystem.Instance.CreateBullet(bullectID, fireContext) != null;
                }


//                Debug.LogError("发射普通子弹 bullectID"+ bullectID);

                return bRet;
            }
        }

        //初始化目标相关的信息
        private bool __InitTargetInfo(BulletFireContext fireContext, int userData, int bullectID, IDReco src)
        {
            ulong srcID = src != null ? src.entID : 0;
            //选择一个目标
            if (bulletEnvProvider.IsNeedTarget(srcID, bullectID, userData))
            {
                IDReco targetReco = bulletEnvProvider.SelectTarget(srcID, bullectID, src, userData);
                if (null != targetReco)
                {
                    fireContext.target = targetReco.entID;
                    fireContext.targetPos = targetReco.transform.position;


                    IBulletEnvProvider bulletEnvProvider = EnvProviderMgr.Instance.GetBulletEnvProvider();

                    fireContext.targetLocalPos = bulletEnvProvider.GetEntityRoot()
                        .InverseTransformPoint(fireContext.targetPos);

                    if (bulletEnvProvider.IsNeedForwardTarget(srcID, bullectID, userData))
                    {
                        IEntityManager manager = GameGlobal.EntityWorld.Local;
                        ICreatureEntity entity = manager.GetEntity(fireContext.target) as ICreatureEntity;
                        ForwardMovement2D fm = entity.GetComponent<ForwardMovement2D>();
                        // EntityMovePart movePart = entity.GetPart<EntityMovePart>();
                        // if (movePart.IsMoving() == false)
                        //if (fm != null && fm.IsMoving() == false)
                        {
                            Vector3 forward = fireContext.targetPos - IDReco.transform.position;
                            forward.y = 0;
                            IDReco.transform.forward = forward.normalized;
                        }
                    }
                }
                else
                {
                    //一定需要目标才能释放
                    //if (bulletEnvProvider.IsNeedTarget(srcID, bullectID, userData))
                    return false;
                }
            }

            return true;
        }


        public bool LockFireBullet(int iBulletUID, int iBulletID, bool srcFaceLeft, IDReco targetIDReco)
        {
            return __OnLockFire(iBulletUID, iBulletID, srcFaceLeft, targetIDReco);
        }

        private bool __OnLockFire(int iBulletUID, int iBulletID, bool srcFaceLeft, IDReco targetIDReco)
        {
            bool bRet = false;
            BulletFireContext fireContext = null;
            //float curTime = Time.realtimeSinceStartup;
            // if (bulletEnvProvider.CanFire(bullectID))
            {
                //设置阵营
                if (null != IDReco)
                {
                    ulong srcID = IDReco.entID;
//构建发射现场
                    fireContext = bulletEnvProvider.BuildFireContext(srcID, iBulletID, root, 0);

                    //发射器的网络ID
                    fireContext.netLauncherObjectID = (ulong)iBulletUID;

                    //发射一个子弹
                    fireContext.bulletEnvProvider = bulletEnvProvider;

                    //延时时间
                    fireContext.delayduration = 0;


                    fireContext.src = srcID;
                    fireContext.camp = IDReco.camp;
                    fireContext.listFriendCamps = IDReco.listFriendCamps;
                    fireContext.listEnemyCamps = IDReco.listEnemyCamps;

                    //初始化目标信息
                    if (__InitLockTargetInfo(fireContext, iBulletID, IDReco, targetIDReco) == false)
                    {
                        return false;
                    }


                    cfg_Bullet bulletCfg = GameGlobal.GameScheme.Bullet_0(iBulletID);
                    //子弹基础偏移
                    /*
                    var moveArr =  bulletCfg.arrLaunchPointMove;
                    if (moveArr.Length > 1)
                    {
                        Vector3 srcLocalPos = fireContext.srcLocalPos;
                        var offsetX = moveArr[0];
                        srcLocalPos.x += srcFaceLeft ? -offsetX : offsetX;
                        srcLocalPos.y += moveArr[1];
                        fireContext.srcLocalPos = srcLocalPos;
                    }
                    */
                    //随机角度
                    if (randomAngle)
                    {
                    }
                }

                bRet = BulletSystem.Instance.CreateBullet(iBulletID, fireContext) != null;

                return bRet;
            }
        }

        //初始化目标相关的信息
        private bool __InitLockTargetInfo(BulletFireContext fireContext, int bullectID, IDReco srcIDReco,
            IDReco targetIDReco)
        {
            if (null != targetIDReco)
            {
                fireContext.target = targetIDReco.entID;
                fireContext.targetPos = targetIDReco.hitTrans.position;

                IBulletEnvProvider bulletEnvProvider = EnvProviderMgr.Instance.GetBulletEnvProvider();

                fireContext.targetLocalPos = bulletEnvProvider.GetEntityRoot()
                    .InverseTransformPoint(fireContext.targetPos);

                //if (bulletEnvProvider.IsNeedForwardTarget(srcID, bullectID, userData))
                //{
                //    IEntityManager manager = XGameComs.Get<IEntityManager>();
                //    ICreatureEntity entity = manager.GetEntity(fireContext.target) as ICreatureEntity;
                //    ForwardMovement2D fm = entity.GetComponent<ForwardMovement2D>();
                //    // EntityMovePart movePart = entity.GetPart<EntityMovePart>();
                //    // if (movePart.IsMoving() == false)
                //    //if (fm != null && fm.IsMoving() == false)
                //    {
                //        Vector3 forward = fireContext.targetPos - IDReco.transform.position;
                //        forward.y = 0;
                //        IDReco.transform.forward = forward.normalized;
                //    }
                //}
            }

            return true;
        }
    }
}
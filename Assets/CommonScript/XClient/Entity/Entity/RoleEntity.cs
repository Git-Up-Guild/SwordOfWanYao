using gamepol;
using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{

    public class RoleEntity : VisibleEntity, IRoleEntity
    {
        public bool isSelf => true;

        public EntityIDGenerator entityIDGenerator => GameGlobal.LocalEntityIDGenerator;

        public RoleDataPart data
        {
            get;
            private set;
        }
        public string RoleName
        {
            get;
            private set;
        }

        //int 属性
        private int[] m_arrNumProp = new int[(int) EnRoleProp.ROLE_PROP_PRIVATE]; //属性
        private long[] m_arrNumProp64 = new long[(int) EnRoleProp64.ROLE_PROP64_PRIVATE]; //属性

        //数据ID
        private long m_roleID;

        protected override void OnInit(object context)
        {
            if (!GameGlobal.Instance.GameInitConfig.serverConfig.isRoomMode)
                GameGlobal.LocalEntityIDGenerator.SetMasterID(id);
            TMSG_ENTITY_CREATE_ROLE_NTF createContext = context as TMSG_ENTITY_CREATE_ROLE_NTF;

            if(createContext != null)
            {
                TRolePrivateContext stPrivateContext = createContext.get_stPrivateContext();
                RoleName = stPrivateContext.get_szName();

                Array.Copy(stPrivateContext.get_arrNumProp(), m_arrNumProp, m_arrNumProp.Length);
                Array.Copy(stPrivateContext.get_arrNumProp64(), m_arrNumProp64, m_arrNumProp64.Length);
                long[] prop = stPrivateContext.get_arrNumProp64();
                m_roleID = prop[((int)EnRoleProp64.ROLE_PROP64_PDBID)];
            }
   

            // //临时代码，后面名字要玩家取
            // string localName = PlayerPrefs.GetString("RoleName");
            // if (string.IsNullOrEmpty(localName))
            // {
            //     int maxCount = GameGlobal.GameScheme.RandomName_nums();
            //     long timeStamp = DateTimeOffset.UtcNow.Ticks;
            //
            //     int lastIndex = (int)(timeStamp % maxCount);
            //     int firstIndex = (int)(timeStamp % maxCount);
            //
            //     localName = GameGlobal.GameScheme.RandomName(lastIndex).szLasttName + GameGlobal.GameScheme.RandomName(firstIndex).szFirstName;
            //     PlayerPrefs.SetString("RoleName", localName);
            //     PlayerPrefs.Save();
            // }

            // RoleName = localName;
    
        }

        protected override IEntityPart CreatePart(PartInfo Info, object context)
        {
            var part = base.CreatePart(Info, context);

            //将数据部件拿出来
            if (part is RoleDataPart)
                data = part as RoleDataPart;
            return part;
        }

        protected override void OnPositionUpdate()
        {
            //修改自身坐标
            visiblePart?.SetPosition(position);

            //修改数据部件，进行数据广播
            data.position.Value = position;
        }

        public override string GetResPath()
        {
            return GameGlobal.Instance.GameInitConfig.playerInitData.playerPrefab.path;
        }

        public void SetIntProp(int id, int value)
        {
            if (id >= 0 && id < m_arrNumProp.Length)
            {
                m_arrNumProp[id] = value;
            }
        }

        public int GetIntProp(int id)
        {
            if (id >= 0 && id < m_arrNumProp.Length)
            {
                return m_arrNumProp[id];
            }
            return 0;
        }

        public void SetIntProp64(int id, long value)
        {
            if (id >= 0 && id < m_arrNumProp64.Length)
            {
                m_arrNumProp64[id] = value;
            }
        }

        public long GetIntProp64(int id)
        {
            if (id >= 0 && id < m_arrNumProp64.Length)
            {
                return m_arrNumProp64[id];
            }
            return 0;
        }

        public long GetRoleID()
        {
            return m_roleID;
        }

        public void SetName(string name)
        {
            RoleName = name;
        }

        public int GetPower()
        {
          
            return 0;
        }
    }

}
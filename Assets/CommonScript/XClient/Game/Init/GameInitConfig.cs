
using System;
using System.Collections.Generic;
using UnityEngine;
using XGame.Attr;
using XGame.Cam;
using XGame.Utils;

namespace XClient.GameInit
{
    /// <summary>
    /// 玩家寻路模式
    /// </summary>
    public enum PlayerPathfindingMode
    {
        //导航网格
        NavMeshAgent,                                          
    }

    /// <summary>
    /// 玩家控制输入方式
    /// </summary>
    public enum PlayerControlInputMode
    {
        //按键控制
        Key,

        //摇杆控制
        //Joystick,
    }

    /// <summary>
    /// 玩家初始化数据
    /// </summary>
    [Serializable]
    public class PlayerInitData
    {
        //金币数量
        public int goldNum;

        //钻石数量
        public int diamondNum;

        //[Label("玩家预制体")]
        public ResourceRef playerPrefab;

        //玩家名称
        public string name;

        //玩家等级
        public int level;
    }

    [Serializable]
    public class CameraControllSettings
    {
        [Label("是否启用")]
        public bool isEnable;

        [Label("相机控制模式")]
        public CameraControllMode camreaFollowMode;

        [Label("相机位置偏移")]
        public Vector3 positionOffset;
    }

    [Serializable]
    public class ServerInfo
    {
        [Label("服务器ID")]
        public long ID;

        [Label("服务器IP")]
        public string serverAddr;

        [Label("服务器端口")]
        public int serverPort;
    }

    /// <summary>
    /// 服务器配置
    /// </summary>
    [Serializable]
    public class ServerConfig
    {
        //服务器列表
        public List<ServerInfo> servers;

        [Label("当前服务器ID")]
        public long serverID;

        [Label("是否为房间模式")]
        public bool isRoomMode;

        [ALabel("房间ID")]
        [AVisibleChecker("isRoomMode", true)]
        [CustomDisplay]
        public int roomID;
    }

    [CreateAssetMenu(menuName = "XGame/Game Init/Create Game Init Asset")]
    public class GameInitConfig : ScriptableObject
    {
        [Label("是否为单机模式")]
        public bool isStandaloneMode;

        [ALabel("服务器设置")]
        [AVisibleChecker("isStandaloneMode", false)]
        [CustomDisplay]
        public ServerConfig serverConfig;

        [Label("玩家初始化数据")]
        public PlayerInitData playerInitData;


        [Label("是否启用玩家控制")]
        public bool enablePlayerControl;

        [ALabel("玩家控制输入模式")]
        [AVisibleChecker("enablePlayerControl", true)]
        [CustomDisplay]
        public PlayerControlInputMode playerControlInputMode;

        [ALabel("玩家寻路模式")]
        [AVisibleChecker("enablePlayerControl", true)]
        [CustomDisplay]
        public PlayerPathfindingMode playerrPathfindingMode;

        [ALabel("相机控制设置")]
        [AVisibleChecker("enablePlayerControl", true)]
        [CustomDisplay]
        public CameraControllSettings cameraFollowSettings;

        [Label("登录游戏后进入的场景")]
        public int gameStateBuildinSceneIndex = 1;


        [Label("登录游戏后进入的场景")]
        public string gameScenePath = "";

        [Label("项目的根目录")]
        public string projectRootDir;

        /// <summary>
        /// 获取当前服务器信息
        /// </summary>
        /// <returns></returns>
        public ServerInfo GetCurrentServerInfo()
        {
            foreach(var s in serverConfig.servers)
            {
                if(s.ID == serverConfig.serverID)
                {
                    return s;
                }
            }
            return null;
        }

        public static GameInitConfig s_Instance;
        public static GameInitConfig Instance
        {
            get
            {
                return s_Instance;
            }
        }

        private void OnEnable()
        {
            s_Instance = this;
        }

        private void OnDestroy()
        {
            s_Instance = null;
        }
    }
}

using UnityEngine;

namespace XClient.Login
{
    public class LoginDataManager
    {
        public class LoginData
        {
            public string userName;
            public string password = "123456";
            public int partnerID = (int)gamepol.EnLoginPartnerID.enLoginPartnerID_Test;  //游客账号
            public long serverSID;
            public long serverID;
            public string serverAddr;
            public int serverPort;
            public int roomID;

            //SDK登录的 session
            public string session;

            //session  生效时间
            public float sessionEffectiveTime = 0;

            public bool isDisableBuyGoodsConfirmBox = false;

            public void CopyTo(LoginData target)
            {
                target.userName = userName;
                target.password = password;
                target.partnerID = partnerID;
                target.serverSID = serverSID;
                target.serverID = serverID;
                target.serverAddr = serverAddr;
                target.serverPort = serverPort;
                target.session = session;   
                target.roomID = roomID;
                target.sessionEffectiveTime = sessionEffectiveTime; 
            }
            
            public bool IsValid()
            {
                if(string.IsNullOrEmpty(userName))
                {
                    return false;
                }

                //if(Time.realtimeSinceStartup- sessionEffectiveTime>5.5*3600)
                //测试版本， 先填一个5分钟
                if (Time.realtimeSinceStartup - sessionEffectiveTime > 5 * 60)
                {
                    return false;
                }


                return true;
            }

            public override string ToString()
            {
                return $"userName:{userName}, password:{password}, partnerID:{partnerID}, serverSID:{serverSID}, serverID:{serverID}, serverAddr:{serverAddr}, serverPort:{serverPort}";
            }
        }

        private LoginDataManager() { }

        public static LoginDataManager instance = new LoginDataManager();

        /// <summary>
        /// 当前的数据
        /// </summary>
        public LoginData current = new LoginData();

        /// <summary>
        /// 保存的数据
        /// </summary>
        public LoginData save = new LoginData();

        /// <summary>
        /// 将当前数据拷贝到保存数据
        /// </summary>
        public void SyncCurrentToSave()
        {
            current.isDisableBuyGoodsConfirmBox = false;
            current.CopyTo(save);

            Save();
        }

        /// <summary>
        /// 将保存数据同步到当前数据
        /// </summary>
        public void SyncSaveToCurrent()
        {
            save.CopyTo(current);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetString("Login-userName", save.userName);
            PlayerPrefs.SetString("Login-password", save.password);
            PlayerPrefs.SetInt("Login-partnerID", save.partnerID);
            PlayerPrefs.SetString("Login-serverID", save.serverID.ToString());
            PlayerPrefs.SetString("Login-serverSID", save.serverSID.ToString());
            PlayerPrefs.SetString("Login-serverAddr", save.serverAddr);
            PlayerPrefs.SetInt("Login-serverPort", save.serverPort);
            PlayerPrefs.SetInt("Login-roomID", save.roomID);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public void Load()
        {
            save.userName = PlayerPrefs.GetString("Login-userName");
            save.password = PlayerPrefs.GetString("Login-password");
            save.partnerID = PlayerPrefs.GetInt("Login-partnerID", 2);
            var serverID = PlayerPrefs.GetString("Login-serverID");
            if (string.IsNullOrEmpty(serverID))
                serverID = "0";
            save.serverID = System.Convert.ToInt64(serverID); 
            var serverSID = PlayerPrefs.GetString("Login-serverSID");
            if (string.IsNullOrEmpty(serverSID))
                serverSID = "0";
            save.serverSID = System.Convert.ToInt64(serverSID);
            save.serverAddr = PlayerPrefs.GetString("Login-serverAddr");
            save.serverPort = PlayerPrefs.GetInt("Login-serverPort", 0);
            save.roomID = PlayerPrefs.GetInt("Login-roomID", 0);

            SyncSaveToCurrent();
        }

        public void UpdateLoginInfo(string userName, string session)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                current.userName = userName;
            }
            else
            {
                Debug.Log("错误的用户名");
            }

            current.session = session;
            current.sessionEffectiveTime = Time.realtimeSinceStartup;
        }

        public string GetUserName()
        {
            if (!string.IsNullOrEmpty(current.userName))
            {
                return current.userName;
            }

            return $"XGame{Random.Range(1000, 10000)}";
        }

        //session 的生效时间
        public float GetSessionEffectiveTime()
        {
            return current.sessionEffectiveTime;
        }
    }
}
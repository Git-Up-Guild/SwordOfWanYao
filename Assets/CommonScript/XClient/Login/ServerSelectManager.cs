using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;
using XClient.Common;
using XClient.Game;
using XGame;
namespace XClient.Login
{
    public class ServerInfo
    {
        public int sid { get; set; }
        public int zid { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public int f { get; set; }
        public string ip { get; set; }
        public List<int> port { get; set; }
        public string mainten { get; set; }
        public int recommend { get; set; }
        public int state { get; set; }

        public ServerInfo(int sid, int zid, string name, int id, int f, string ip, List<int> port, string mainten, int recommend, int state)
        {
            
            this.sid = sid;
            this.zid = zid;
            this.name = name;
            this.id = id;
            this.f = f;
            this.ip = ip;
            this.port = port;
            this.mainten = mainten;
            this.recommend = recommend;
            this.state = state;
        }

        //过去随机端口
        public int GetRandomPort()
        {
            if(null== port|| port.Count==0)
            {
                Debug.LogError("没有找到端口配置,返回默认的端口 9001");
                return 9001;
            }

            int nCount = port.Count;
            return port[((int)Time.realtimeSinceStartup)% nCount]; 
        }
    }

    public class ServerGroup
    {
        public int id { get; set; }
        public string showname { get; set; }
        public int openchannel { get; set; }
        public List<ServerInfo> servers { get; set; }

        public ServerGroup(int id, string showname, int openchannel)
        {
            this.id = id;
            this.showname = showname;
            this.openchannel = openchannel;
            this.servers = new List<ServerInfo>();
        }
    }
    public class ServerSelectManager
    {
        #if UNITY_EDITOR
        private readonly bool isLocalServer = true;
        #else
        private readonly bool isLocalServer = false;
        #endif
        private readonly string SaveIdKey = "SelectId";
        private int selectZID = -1;
        private List<ServerGroup> groups = new List<ServerGroup>();
        private static readonly string xmlFile = "serverlist.xml";
        private readonly string xmldir = "serverlist/" + xmlFile; //https://aigirl.tj2016.com/immortalfamily/download/

        //是否成功拉取远程服务器列表
        private bool fectchServerListSucc = false;

        public void Init()
        {
            
            if(PlayerPrefs.HasKey(SaveIdKey))
            {
                selectZID = PlayerPrefs.GetInt(SaveIdKey);
            }
            //selectZID = 2016;

           // : GetDefaultSelectZID();

            //selectZID = GetDefaultSelectZID();

            if (!isLocalServer)
            {
                CGame.Instance.StartCoroutine(FetchAndParseXML());
            }else
            {
                CheckAndGetPath();
                fectchServerListSucc = true;
            }
        }

        private void CheckAndGetPath()
        {
            string fileName = "serverlist.xml";
            
            //取外部存储
            if(!isLocalServer)
            {
                var filePath = string.Format("{0}{1}", GamePath.GetAssetBundleRootPathExternal(), fileName);
                if (File.Exists(filePath))
                {

                    XGameApp.Log($"读取本地的server.ist.xml文件 path= {filePath}");
                    // 获取这个xml文件的文本信息
                    // 直接读取文件内容
                    string xmlContent = File.ReadAllText(filePath);

                    // 解析XML内容
                    groups = ParseGroupsFromXML(xmlContent);
                    return;
                }
            }
            //取老的一份
            {
                XGameApp.Log("[ServerSelectManager]获取原始服务器列表");
                var filePath = Path.Combine(Application.dataPath, "Service", fileName);
                if (File.Exists(filePath))
                {
                    XGameApp.Log($"读取包内server.ist.xml文件 path= {filePath}");
                    string xmlString = File.ReadAllText(filePath);
                    groups = ParseGroupsFromXML(xmlString);
                }
                else
                {
                    XGameApp.LogError("[ServerSelectManager]文件不存在，请检查" + filePath);
                }
            }
        }

        IEnumerator FetchAndParseXML()
        {
            string xmlURL = $"{UpdateConfig.webDownloadUrl}{xmldir}?ver={DateTime.Now.Ticks}";
            UnityWebRequest request = UnityWebRequest.Get(xmlURL);// xmldir + "?ver=" + DateTime.Now.Ticks);
        
            // Send the request and wait for it to complete
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                XGameApp.Log("[ServerSelectManager]获取线上xml成功");
                string xmlContent = request.downloadHandler.text;
                groups = ParseGroupsFromXML(xmlContent);
                
                SaveFile(xmlFile, request.downloadHandler.data);
               // GameGlobal.EventEgnine?.FireExecute(DGlobalEventEx.EVENT_SERVER_INFO_UPD, 0, 0, null);
            }
            else
            {
                XGameApp.LogError("[ServerSelectManager]取不到最新的服务器列表: " + request.error);
                CheckAndGetPath();
            }

            fectchServerListSucc = true;
        }

        public bool isFectchServerListSucc()
        {
            return fectchServerListSucc;
        }

        private void SaveFile(string fileName, byte[] xmlContent)
        {
            var szDesFilePath = string.Format("{0}{1}", GamePath.GetAssetBundleRootPathExternal(), fileName);
            string sz_Path = szDesFilePath.Substring(0, szDesFilePath.LastIndexOf("/"));
            // 判断下目标目录是否存在，不存在就创建
            if (!Directory.Exists(sz_Path))
            {
                try
                {
                    XGameApp.Log($"目标目录不存在，创建，path={sz_Path}");
                    Directory.CreateDirectory(sz_Path);
                }
                catch (Exception e)
                {
                    XGameApp.LogError("创建目录失败： error ex=" + e.Message + ",path=" + sz_Path);
                    return;
                }
            }

            XGameApp.Log($"[ServerSelectManager]拷贝文件, dstPath={szDesFilePath}");





            var data = xmlContent;
            File.WriteAllBytes(szDesFilePath, data);
        }

        private List<ServerGroup> ParseGroupsFromXML(string xmlContent)
        {
            XDocument doc = XDocument.Parse(xmlContent);
            List<ServerGroup> groupList = new List<ServerGroup>();

            var groupElements = doc.Descendants("Group");

            foreach (var groupElement in groupElements)
            {
                int groupId = int.Parse(groupElement.Attribute("id").Value);
                string showname = groupElement.Attribute("showname").Value;
                int openchannel = int.Parse(groupElement.Attribute("openchannel").Value);

                ServerGroup serverGroup = new ServerGroup(groupId, showname, openchannel);

                var serverElements = groupElement.Elements("Server");
                foreach (var serverElement in serverElements)
                {
                    int sid = int.Parse(serverElement.Attribute("sid").Value);
                    int zid = int.Parse(serverElement.Attribute("zid").Value);
                    string name = serverElement.Attribute("name").Value;
                    int id = int.Parse(serverElement.Attribute("id").Value);
                    int f = int.Parse(serverElement.Attribute("f").Value);
                    string ip = serverElement.Attribute("ip").Value;
                    string ports = serverElement.Attribute("port").Value;
                    List<int> listPorts = ParseItemList(ports);
                    //int port = int.Parse(ports);
                    string mainten = serverElement.Attribute("mainten").Value;
                    int recommend = int.Parse(serverElement.Attribute("recommend").Value);
                    int state = int.Parse(serverElement.Attribute("state").Value);

                    ServerInfo server = new ServerInfo(sid, zid, name, id, f, ip, listPorts, mainten, recommend, state);
                    serverGroup.servers.Add(server);
                }

                groupList.Add(serverGroup);
            }

            return groupList;
        }

        public void SetSelectId(int id)
        {
            selectZID = id;
            PlayerPrefs.SetInt(SaveIdKey, selectZID);
            PlayerPrefs.Save();
        }

        public ServerInfo GetCurrentServerInfo()
        {
            if (null == groups || groups.Count == 0)
            {
                return null;
            }

            if (selectZID == -1) return GetDefaultServerInfo();

         

            for (int i = 0; i < groups.Count; i++)
            {
                foreach (var VARIABLE in groups[i].servers)
                {
                    if (VARIABLE.zid == selectZID)
                    {
                        return VARIABLE;
                    }
                }
            }
            //找不到则选择默认
            return GetDefaultServerInfo();
        }

        public int GetDefaultRoomId()
        {
            return 2;
        }

        private ServerInfo GetDefaultServerInfo()
        {
            if (null == groups || groups.Count == 0)
            {
                return null;
            }

            //return groups[0].servers[0];
            ServerInfo defualtInfo = groups[0].servers[0];
            int nCount = groups.Count;
            for (int i = 0; i < nCount; ++i)
            {
                foreach (var serverInfo in groups[i].servers)
                {
                    if (defualtInfo.sid < serverInfo.sid)
                    {
                        defualtInfo = serverInfo;
                    }
                }
            }

           // Debug.LogError("选择的服务器 ZID="+ defualtInfo.zid);

            return defualtInfo;
        }
        public int GetSelectZId()
        {
            return selectZID;
        }

        public int GetDefaultSelectZID()
        {
            if (null == groups || groups.Count == 0)
            {
                return -1;
            }

            int minZID = groups[0].servers[0].zid;
            int minSID = groups[0].servers[0].sid;
            int nCount = groups.Count;
            for(int i=0;i<nCount;++i)
            {
               foreach(var serverInfo in groups[i].servers)
                {
                    if(minSID < serverInfo.sid)
                    {
                        minSID = serverInfo.sid;
                        minZID = serverInfo.zid;    
                    }
                }
            }

           // Debug.LogError("默认的 ZID=" + minZID);

            return minZID;
        }

        public int GetGruopDefaultZID(int groupIndex)
        {
            if (null == groups || groups.Count<= groupIndex|| groupIndex<0|| groups[groupIndex].servers.Count==0)
            {
                return -1;
            }

            int minZID = groups[groupIndex].servers[0].zid;
            int minSID = groups[groupIndex].servers[0].sid;
            foreach (var serverInfo in groups[groupIndex].servers)
            {
                if (minSID < serverInfo.sid)
                {
                    minSID = serverInfo.sid;
                    minZID = serverInfo.zid;
                }
            }

            return minZID;
        }

        public List<ServerGroup> GetAllServerInfo()
        {
            return groups;
        }

        
        private List<int> ParseItemList(string value)
        {
            List<int> list = new List<int>();
            if(value.IndexOf(',')<0)
            {
                list.Add(int.Parse(value));
                return list;
            }


            string[] items = value.Split(','); 
            for(int i=0;i< items.Length;++i)
            {
                list.Add(int.Parse(items[i]));  
            }

            return list;
        }
        

    }
}
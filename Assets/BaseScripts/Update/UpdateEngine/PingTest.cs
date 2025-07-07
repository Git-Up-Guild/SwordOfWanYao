using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using UnityEngine;

public class PingTest : MonoBehaviour
{
    //域名列表
    public List<string> listDomain;

    //超时时间
    public float timeout = 5;

    //ping对象列表
    private Dictionary<System.Net.NetworkInformation.Ping,string> m_dicPingObj;

    //时间 
    float m_startTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        m_startTime = Time.realtimeSinceStartup;
        int nCount = listDomain.Count;

        if(nCount>0 && CheckNetwork())
        {
            GameObject.DontDestroyOnLoad(this.gameObject);

            m_dicPingObj = new Dictionary<System.Net.NetworkInformation.Ping, string>();

            for (int i = 0; i < nCount; ++i)
            {
                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                m_dicPingObj.Add(p, listDomain[i]);
                p.PingCompleted += PingCompletedEventHandler;
                try
                {
                    p.SendAsync(listDomain[i], 5, p);
                }
                catch (Exception e)
                {
                    if(e!=null&&string.IsNullOrEmpty(e.Message)==false)
                    {
                        Debug.LogError(e.Message);
                    }
                    
                   // TalkingDataAPI.OnEvent("ping_错误", listDomain[i], "1");
                }


            }
        }
    }

       

    // Update is called once per frame
    void Update()
    {
        if(m_dicPingObj==null||m_dicPingObj.Count==0)
        {
            GameObject.DestroyImmediate(this.gameObject);
            return;
        }

        float curTime = Time.realtimeSinceStartup;
        if(curTime- m_startTime> timeout)
        {
            foreach(System.Net.NetworkInformation.Ping p in m_dicPingObj.Keys)
            {
                string domain = m_dicPingObj[p];
                Debug.LogError("ping 超时 domain= " + domain );
                //TalkingDataAPI.OnEvent("ping_超时", domain, "1");
                p.Dispose();
                
            }
            m_dicPingObj.Clear();

        }
    }

    private void OnDestroy()
    {
        //Debug.Log("Ping Test OnDestroy");
        m_dicPingObj = null;
    }

    public void PingCompletedEventHandler(object sender, PingCompletedEventArgs e)
    {
        System.Net.NetworkInformation.Ping pingObj = sender as System.Net.NetworkInformation.Ping;

        string domain = "";
        if (null != pingObj&&null!= m_dicPingObj)
        {
            m_dicPingObj.TryGetValue(pingObj,out domain);
            m_dicPingObj.Remove(pingObj);
            pingObj.Dispose();
            pingObj = null;
        }


        //Debug.Log("111111111");

        if(e!=null)
        {
            //Debug.Log("2222222222");
            PingReply Reply = e.Reply;

            //Debug.Log("3333333333");

            if(null!= Reply)
            {
                if (Reply.Status != IPStatus.Success)
                {
                    //Debug.Log("44444");

                    //错误打点
                  //  TalkingDataAPI.OnEvent("ping_错误", domain, "1");
                    Debug.LogError("domain=" + domain + ", ping error=" + Reply.Status);
                }

                if (Reply.RoundtripTime > 100)
                {
                    //Debug.Log("55555555555555");
                    Debug.LogError("domain=" + domain + ",延时 RoundtripTime=" + Reply.RoundtripTime);

                    //超时打点
                   // TalkingDataAPI.OnEvent("ping_延时", "RoundtripTime", "" + Reply.RoundtripTime);
                }
            }else
            {
                if(e.Error!=null)
                {
                    Debug.Log("Ping Error=" + e.Error.Message);
                }

                

                float curTime = Time.realtimeSinceStartup;
                float elpase = curTime - m_startTime;
                if (elpase>100)
                {
                    //Debug.Log("666666666666666666");
                    //错误打点
                   // TalkingDataAPI.OnEvent("ping_延时", domain, "1");
                    Debug.LogError("domain=" + domain + ", Reply= null");
                }
               
            }

            
        }


        //Debug.Log("7777");

        if (null!= m_dicPingObj&&m_dicPingObj.Count==0)
        {
            GameObject.DestroyImmediate(this.gameObject);
        }
    }

    public bool CheckNetwork()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                return false;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                return true;
        }
        return false;
    }
}

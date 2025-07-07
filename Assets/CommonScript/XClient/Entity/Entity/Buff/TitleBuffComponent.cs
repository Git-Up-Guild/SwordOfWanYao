/*******************************************************************
** 文件名:	TitleBuffComponent.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.23
** 版  本:	1.0
** 描  述:	
** 应  用:  buff头顶图标

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XClient.Common;
using XClient.Entity;
using XGame.Ini;
using XGame.UI;

namespace XGame.Entity
{

    //每个item保存现场
    public class IconItem
    {
        public uint iconKey;
        public int iconID;
        public int layer;
        public int buffId;

        //public Image img;
        //public TextMeshProUGUI txt;
        //public GameObject view;
        //public uint iconHandle;
    }
    public class BuffEventContext
    {
        private BuffEventContext()
        {
        }

        public static BuffEventContext Instance = new BuffEventContext();
        public ulong masterId;
        public List<IconItem> listHeadIcons;

        public void Reset()
        {
            masterId = 0;
            listHeadIcons = null;
        }
    }
    public class TitleBuffComponent : VisibleComponent
    {
        //图标对象原型
        public GameObject iconItemProto;

        //间隔
        public float space = 64;

        //高度偏移
        public float offsetY = 100;


        //当前显示的图标
        //private Dictionary<uint, IconItem> m_dicHeadIcons = new Dictionary<uint, IconItem>();

        //策划要求不要打乱buff icon的顺序
        private List<IconItem> m_listHeadIcons = new List<IconItem>();

        //头顶部分的根部 canvas
        // private Canvas m_headCanvas;

        //生物相机
        // private Camera m_creatureCamera;

        //当前title head 的根部
        //private RectTransform m_titleHeadRoot;

        //所有图标
        static private List<IconItem> m_itemsPool = new List<IconItem>();
        static uint s_maxIconKey = 0;

        // Start is called before the first frame update
        void Awake()
        {

            if (null != visibleRoot)
            {
                if (visibleRoot.activeSelf == false)
                {
                    visibleRoot.BetterSetActive(false);
                }
            } else
            {
                visibleRoot = this.gameObject;
            }


            /*
           // visibleRoot = new GameObject("TitleItem");
            m_titleHeadRoot = visibleRoot.AddComponent<RectTransform>();

            
            m_headCanvas = UITitleHeadCanvas.uiHeadCanvas;
            if(null!= m_headCanvas&&null!= visibleRoot)
            {
                visibleRoot.transform.BetterSetParent(m_headCanvas.transform);
            }
            

            m_titleHeadRoot.anchorMin = Vector2.zero;
            m_titleHeadRoot.anchorMax = Vector2.zero;
            m_titleHeadRoot.localRotation = Quaternion.identity;
            m_titleHeadRoot.transform.localScale = Vector3.one;
            m_titleHeadRoot.anchoredPosition3D = new Vector3(0, offsetY,0);

            //m_creatureCamera = Camera.main;

            /*
            AddIcon(1012);
            AddIcon(1013);
            AddIcon(1014);
            */

        }

        // Update is called once per frame
        void LateUpdate()
        {
            /*
            if(null== m_creatureCamera)
            {
                return;
            }

            Vector3 pos = this.transform.position;

            Vector3 screenPoint = m_creatureCamera.WorldToScreenPoint(pos);
            screenPoint.y += offsetY;
            screenPoint.z = 0;
            m_titleHeadRoot.anchoredPosition3D = screenPoint;
            */

        }

        private void OnDestroy()
        {
            Clear();

            if (null != visibleRoot && this.gameObject != visibleRoot)
            {
                GameObject.DestroyImmediate(visibleRoot);
                visibleRoot = null;
            }
            //if (null != m_iconRootObj )
            //{
            //    GameObject.DestroyImmediate(m_iconRootObj);
            //    m_iconRootObj = null;
            //}
            //if (null != m_textRootObj)
            //{
            //    GameObject.DestroyImmediate(m_textRootObj);
            //    m_textRootObj = null;
            //}
        }

        //private GameObject m_iconRootObj;
        //private GameObject iconRootObj
        //{
        //    get
        //    {
        //        if (m_iconRootObj == null)
        //        {
        //            m_iconRootObj = new GameObject("iconRootObj");
        //            bool isFaceLeft = false;
        //            IEntityManager manager = XGameComs.Get<IEntityManager>();
        //            if (manager != null)
        //            {
        //                Monster monster = manager.GetEntity(m_masterId) as Monster;
        //                if (monster != null)
        //                    isFaceLeft = monster.GetFaceLeft();
        //            }
        //            m_iconRootObj.transform.BetterSetParent(visibleRoot.transform);
        //            m_iconRootObj.transform.localPosition = Vector3.zero;
        //            m_iconRootObj.transform.localScale = isFaceLeft ? new Vector3(-1, 1, 1) : Vector3.one; ;
        //        }
        //        return m_iconRootObj;
        //    }
        //}
        //private GameObject m_textRootObj;
        //private GameObject textRootObj
        //{
        //    get
        //    {
        //        if (m_textRootObj == null)
        //        {
        //            m_textRootObj = new GameObject("textRootObj");
        //            m_textRootObj.transform.BetterSetParent(visibleRoot.transform);
        //            bool isFaceLeft = false;
        //            IEntityManager manager = XGameComs.Get<IEntityManager>();
        //            if (manager != null)
        //            {
        //                Monster monster = manager.GetEntity(m_masterId) as Monster;
        //                if (monster != null)
        //                    isFaceLeft = monster.GetFaceLeft();
        //            }
        //            m_textRootObj.transform.localPosition = Vector3.zero;
        //            m_textRootObj.transform.localScale = isFaceLeft ? new Vector3(-1, 1, 1) : Vector3.one; ;
        //        }
        //        return m_textRootObj;
        //    }
        //}

        private ulong m_masterId;
        //buffId是为了显示的逻辑
        public uint AddIcon(int iconID, ulong masterId, int bufferId)
        {
            uint key = ++s_maxIconKey;
            IconItem item = __AlocHeadItem();
            item.iconKey = key;
            item.iconID = iconID;
            item.layer = 1;
            item.buffId = bufferId;
            m_masterId = masterId;
            ////item.view.transform.BetterSetParent(visibleRoot.transform);
            //item.view.transform.BetterSetParent(iconRootObj.transform);
            //item.txt.transform.BetterSetParent(textRootObj.transform);
            //if(item.img != null)
            //{
            //    var cfgIcon = GameGlobal.GameScheme.Icon_0((uint)iconID);
            //    var strPath = GetIconSpriteAltasPath(cfgIcon);
            //    item.iconHandle = GameGlobal.SpriteAtlasManager.LoadSprite2Img(item.img, strPath, cfgIcon.strIconName);
            //}


            //RectTransform rt = item.view.transform as RectTransform;

            //rt.localRotation = Quaternion.identity;
            //rt.localScale = Vector3.one;
            //rt.anchoredPosition3D = Vector3.zero;
            //RectTransform txtRt = item.txt.transform as RectTransform;
            //if (txtRt != null)
            //{
            //    txtRt.localRotation = Quaternion.identity;
            //    txtRt.localScale = Vector3.one;
            //    txtRt.anchoredPosition3D = Vector3.zero;
            //}
            ////m_dicHeadIcons.Add(key, item);
            m_listHeadIcons.Add(item);

            //__RecalcPos();
            /*
            BuffEventContext.Instance.Reset();
            BuffEventContext.Instance.masterId = m_masterId;
            BuffEventContext.Instance.listHeadIcons = m_listHeadIcons;
            GameGlobal.EventEgnine?.FireExecute(DGlobalEventEx.EVENT_BATTLE_BUFF_REFRESH, 0, 0, BuffEventContext.Instance);
            */

            return key;
        }

        public void SetLayer(uint key, int layer)
        {
            int idx = m_listHeadIcons.FindIndex(item => item.iconKey == key);
            if (idx >= 0)
            {
                m_listHeadIcons[idx].layer = layer;
                //if (layer <= 1)
                //{
                //    m_listHeadIcons[idx].txt.text = string.Empty;
                //}
                //else
                //{
                //    m_listHeadIcons[idx].txt.text = layer.ToString();
                //}

            }

            /*
            BuffEventContext.Instance.Reset();
            BuffEventContext.Instance.masterId = m_masterId;
            BuffEventContext.Instance.listHeadIcons = m_listHeadIcons;
            GameGlobal.EventEgnine?.FireExecute(DGlobalEventEx.EVENT_BATTLE_BUFF_REFRESH, 0, 0, BuffEventContext.Instance);
            */

        }

        public void RemoveIcon(uint key)
        {
            //IconItem item;

            int idx = m_listHeadIcons.FindIndex(item=>item.iconKey == key);
            if (idx >= 0)
            {
                __RecycleHeadItem(m_listHeadIcons[idx]);
                m_listHeadIcons.RemoveAt(idx);
            }
            //if (m_dicHeadIcons.TryGetValue(key, out item))
            //{
            //    __RecycleHeadItem(item);
            //    m_dicHeadIcons.Remove(key); 
            //}

            /*
            BuffEventContext.Instance.Reset();
            BuffEventContext.Instance.masterId = m_masterId;
            BuffEventContext.Instance.listHeadIcons = m_listHeadIcons;
            GameGlobal.EventEgnine?.FireExecute(DGlobalEventEx.EVENT_BATTLE_BUFF_REFRESH, 0, 0, BuffEventContext.Instance);
            */


            //__RecalcPos();
        }

        public override void Clear()
        {
            //foreach(var item in m_dicHeadIcons.Values)
            //{
            //    __RecycleHeadItem(item);
            //}
            //m_dicHeadIcons.Clear();


            int count = m_listHeadIcons.Count;
            for (int i=0; i < count; i++)
            {
                __RecycleHeadItem(m_listHeadIcons[i]);
            }
            m_listHeadIcons.Clear();
        }


        public static string GetIconSpriteAltasPath(cfg_Icon config)
        {
            if (config == null)
                return string.Empty;
            return $"Game/ImmortalFamily/GameResources/Artist/Icon/{config.strAtlasPath}/{config.strAtlasPath}.spriteatlas";
        }

        private int m_rowCount = 5;
        private int spaceY = 48;
        void __RecalcPos()
        {
            //space = 48;
            //int nCount = m_listHeadIcons.Count;
            //int nRow = nCount / m_rowCount;
            //int nLastRowCount = nCount % m_rowCount;
            //int nIndex = 0;
            //for (int i = 0; i <= nRow; i++)
            //{
            //    int curRowItemCount = i == nRow ? nLastRowCount : m_rowCount;
            //    Vector3 pos = new Vector3(-curRowItemCount * space / 2 + space / 2, -spaceY *i, 0);
            //    //if(GameIni.Instance.enableDebug)
            //    //{
            //    //    Debug.Log($"curRow {i}, pos:{pos} nCount:{nCount} nRow:{nRow} curRowItemCount:{curRowItemCount}");
            //    //}
            //    for (int j = 0; j < curRowItemCount; j++)
            //    {
            //        RectTransform rtf = null;
            //        rtf = (RectTransform)m_listHeadIcons[nIndex].view.transform;
            //        if (rtf != null)
            //        {
            //            rtf.anchoredPosition = pos;
            //            pos.x += space;
            //        }
            //        RectTransform txtRt = (RectTransform)m_listHeadIcons[nIndex].txt.transform;
            //        if (txtRt != null)
            //        {
            //            txtRt.anchoredPosition = rtf.anchoredPosition;
            //        }
            //        ++nIndex;
            //    }
             
            //}
            //int nIndex = 0;
            //Vector3 pos = new Vector3(-nCount* space/2+ space/2, 0, 0);
            //RectTransform rtf = null;
            //foreach(var item in m_dicHeadIcons.Values)
            //{
               
            //    rtf = (RectTransform)item.view.transform;
            //    rtf.anchoredPosition = pos;
            //    pos.x += space;
            //    ++nIndex;
            //}
        }

        //分配一个item
        private IconItem __AlocHeadItem()
        {
            IconItem item = null;
            if (m_itemsPool.Count>0)
            {
                item = m_itemsPool[m_itemsPool.Count-1];
                m_itemsPool.RemoveAt(m_itemsPool.Count - 1);

                //if (item.view.activeSelf==false)
                //{
                //    item.view.BetterSetActive(true);
                //}

                return item;
            }

            item = new IconItem();
            //item.view = GameObject.Instantiate(iconItemProto);
            //item.img = item.view.GetComponent<Image>();
            //item.txt = item.view.GetComponentInChildren<TextMeshProUGUI>();
            return item;
        
        }

        private void __RecycleHeadItem(IconItem item)
        {
            if(null==item)
            {
                return;
            }

            //if(item.view.activeSelf)
            //{
            //    item.view.BetterSetActive(false);
            //}

            //GameGlobal.SpriteAtlasManager.UnloadSprite(item.iconHandle);
            //item.iconHandle = 0;

            //item.view.transform.BetterSetParent(null);
            m_itemsPool.Add(item);

        }


    }
}


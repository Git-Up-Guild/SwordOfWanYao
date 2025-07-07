using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Utils;


public class CanvasCull : MonoBehaviour
{
    //控制节点的根部
    public RectTransform root;

    //可视范围的宽度
    public float viewWidth = 300;

    private Camera m_UICamera;
   // private Canvas m_canvas = null;

   // private Animator[] m_animators = null;


    private float minX = 0;
    private float maxX = 0;
    private Vector3 m_lastPos = Vector3.zero;
    private bool m_forceUpdate = true;

    // Start is called before the first frame update
    void Start()
    {
        if(null== root)
        {
            root =   this.gameObject.transform.GetChild(0) as RectTransform;
        }

        
        //root = this.gameObject.transform as RectTransform;
        
        var m_canvas = this.GetComponent<Canvas>();
        if(null== m_canvas)
        {
            m_canvas = this.gameObject.AddComponent<Canvas>();
        }
        
        /*
        */

        if (null == m_UICamera)
        {
            GameObject go = GameObjectGlobalCollection.Instance.GetGameObject((int)GAME_OBJECT_TYPE.OBJECT_TYPE_MAIN_CAMERA);
            if (null != go)
            {
                m_UICamera = go.GetComponent<Camera>();
            }
        }


        //m_animators = GetComponentsInChildren<Animator>();

        minX = -viewWidth;
        maxX = Screen.width+viewWidth;
    }

    private void OnEnable()
    {
        m_forceUpdate = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(null== root|| m_UICamera==null)
        {
            Debug.LogError("null== root|| m_UICamera==null name=" + this.name);
            return;
        }

        Vector3 pos = root.position;

        if(false==m_forceUpdate&&m_lastPos == pos)
        {
            return;
        }

        m_lastPos = pos;
        m_forceUpdate = false;
        Vector3 screenPos = m_UICamera.WorldToScreenPoint(pos);

        bool visible = screenPos.x >= minX && screenPos.x <= maxX;

        /*
        if(m_canvas&& m_canvas.enabled!= visible)
        {
            m_canvas.enabled = visible;
        }

        int nCount = m_animators.Length;
        for(int i=0;i<nCount;++i)
        {
            if(m_animators[i]!=null)
            {
                m_animators[i].enabled = visible;
            }
        }
        */

        
        if(root.gameObject.activeSelf!= visible)
        {
            root.gameObject.BetterSetActive(visible);
        }
        
    }
}

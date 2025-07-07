using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Attr;

public class MaskScreenTime : MonoBehaviour
{
    [Label("激活状态持续时长")]
    public float showDuration = 2.0f;

    [SerializeField, Label("是否激活的时重新开始")]
    private bool m_isRestartWhenActive;

    //开始显示的时间
    private float m_startShowTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        m_startShowTime = Time.realtimeSinceStartup;

        if(m_isRestartWhenActive)
            gameObject.BetterSetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.realtimeSinceStartup - m_startShowTime >= showDuration)
        {
            gameObject.BetterSetActive(false);
        }
    }
}

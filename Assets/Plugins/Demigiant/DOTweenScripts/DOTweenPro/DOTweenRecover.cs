/**************************************************************************    
文　　件：DOTweenRecover
作　　者：郑秀程
创建时间：2022/11/29 14:38:44
描　　述： dotween恢复组件
***************************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTweenRecover : MonoBehaviour
{

    private Vector3 localPos;
    private Vector3 scale;
    private Vector2 offsetMax;
    private Vector2 offsetMin;
    private CanvasGroup cg = null;


    void Awake()
    {
        RecordState();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RecordState()
    {
       

        //if (recover)
        {
#if UNITY_EDITOR
            //    Debug.LogError("记录位置：RecordState: " + this.gameObject.name);
#endif

            RectTransform rectTrans = this.transform as RectTransform;
            if (null != rectTrans)
            {
                offsetMax = rectTrans.offsetMax;
                offsetMin = rectTrans.offsetMin;
                localPos = rectTrans.anchoredPosition3D;
            }
            else
            {
                localPos = this.transform.localPosition;
            }

            
            scale = this.transform.localScale;
            cg = this.GetComponent<CanvasGroup>();


        }
    }

    public void RecoverState()
    {
       // if (recover)
        {

#if UNITY_EDITOR
            //      Debug.LogError("恢复位置：RecoverState: " + this.gameObject.name);
#endif

            RectTransform rectTrans = this.transform as RectTransform;
            if (null != rectTrans)
            {
                rectTrans.offsetMax = offsetMax;
                rectTrans.offsetMin = offsetMin;
                rectTrans.anchoredPosition3D = localPos;
            }
            else
            {
                this.transform.localPosition = localPos;
            }

           
            this.transform.localScale = scale;




            if (cg)
            {
                cg.alpha = 1.0f;
            }
        }
    }
}

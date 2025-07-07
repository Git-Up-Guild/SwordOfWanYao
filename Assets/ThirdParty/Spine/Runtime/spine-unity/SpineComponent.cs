using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineComponent : MonoBehaviour
{
    //ani组件
    public SkeletonAnimation skeAni;

    //UI组件
    public SkeletonGraphic skeGra;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public SkeletonDataAsset skeletonDataAsset
    {
        get
        {
            if (skeAni != null)
            {
                return skeAni.skeletonDataAsset;
            }
            else if (skeGra != null)
            {
                return skeGra.skeletonDataAsset;
            }
            else
            {
                Debug.Log("没有指定动画资产文件");
                return null;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

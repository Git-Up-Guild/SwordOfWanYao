using UnityEditor;
using UnityEngine;
using XClient.Network;
using XGame.Common.Utils;
using XGame.Entity;

namespace XGameEditor.AddressableAsset
{
    public class MenuItemUtils : Editor
    {
        //[MenuItem("XGame/测试/NetID测试")]
        //static void TestNetID()
        //{
        //    var netID = new NetID(9, 19874, 15);
        //    var ID = netID.ID;
        //    var netID2 = new NetID(0, 0, 0);

        //    netID2.Set(ID);
        //    Debug.Log(netID2);

        //    netID2.Set(netID);
        //    Debug.Log(netID2);

        //    var entID = new EntityIDGenerator();
        //    entID.SetMasterID(netID2.ID);
        //    Debug.Log(entID);

        //    var newEntID = entID.Next();
        //    netID2.Set(newEntID);
        //    Debug.Log(netID2);

        //    entID.SetMasterID(netID2.ID);
        //    Debug.Log(entID);
        //}

        [MenuItem("XGame/配置资源/创建动画曲线配置")]
        static void CreateAnimationCurveCollection()
        {
            XGameEditorUtilityEx.CreateScriptableObject<AnimationCurveCollection>();
        }
    }
}
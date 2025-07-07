/**************************************************************************    
文　　件：GameLayers
作　　者：郑秀程
创建时间：2021/2/10 17:03:06
描　　述：
***************************************************************************/
using System;
using UnityEngine;
using XClient.Common;
using XGame;

namespace Game
{
    /// <summary>
    /// 记录各种根对象，便于在游戏中快速访问
    /// </summary>
    public class GameRoots : MonoBehaviour
    {
        public static GameRoots Instance;

        //游戏根对象
        public Transform gameRoot;

        //战斗场景根对象
        public Transform battleSceneRoot;

        //战斗飘字
        public Transform battleFlowTextRoot;

        //游戏场景根对象
        public Transform gameSceneRoot;

        //Camera空间UI根对象
        public Transform uiWithCameraRoot;

        //OverlayUI根对象
        public Transform uiWithOverlayRoot;

        //系统飘字根对象
        public Transform systemFlowTextRoot;

        //CameraUI窗口根对象
        public Transform uiWndWithCameraRoot;

        //OverlayUI窗口根对象
        public Transform uiWndWithOverlayRoot;

        //飞行特效根对象
        public Transform uiFlyEffectRoot;

        //UI加载对象
        public Transform uiWindowLoadingRoot;
        //引导层根对象
        public Transform guideLayerRoot;

        //特有的游戏配置
        public GameConfig gameConfig;



        private void Awake()
        {
            Instance = this;
        }
    }
}

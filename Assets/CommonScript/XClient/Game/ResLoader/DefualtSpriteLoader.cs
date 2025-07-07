using XGame;
using XGame.Asset;
using XGame.Def;
using XGame.UnityObjPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class DefualtSpriteLoader : XGame.I18N.ISpriteLoader
    {
        public uint LoadSprite(string path, Action<Sprite> loadCallback)
        {
            IUnityObjectPool unityObjectPool = XGame.XGameComs.Get<IUnityObjectPool>();
            uint handle = unityObjectPool.LoadRes< Sprite>(path, null, (res, handler, userData) =>
            {
                loadCallback?.Invoke(res as Sprite);
            }, true);
            return handle;
        }

        public void UnloadSprite(uint handle)
        {
            IUnityObjectPool unityObjectPool = XGame.XGameComs.Get<IUnityObjectPool>();
            unityObjectPool.UnloadRes(handle);
        }
    }
}

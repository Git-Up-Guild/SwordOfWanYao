
using minigame;
using System.Collections.Generic;
using XGame.Poolable;

namespace XClient.Common
{
    public class ModuleMessageHandler<T> where T : class, IModule
    {
        /// <summary>
        /// 共享的参数列表，在调用完成后立马会清除
        /// </summary>
        public static List<string> shareRpcParams = new List<string>();

        public T module { get; private set; }

        protected CommonNetMessageRegister netMessageRegister { get; set; }

        public virtual bool Create(T m)
        {
            module = m;
            netMessageRegister = LitePoolableObject.Instantiate<CommonNetMessageRegister>();
            return true;
        }

        protected virtual void OnSetupHandlers() { }

        public void Start()
        {
            OnSetupHandlers();
        }

        public void Stop()
        {
        }

        public void Release()
        {
            LitePoolableObject.Recycle(netMessageRegister);
            netMessageRegister = null;
        }

        public void SendMessage(gamepol.TCSMessage msg, byte dstPoint)
        {
            GameGlobal.NetModule.SendMessage_CS(NetDefine.ENDPOINT_NORMAL, dstPoint, msg);
        }

        public void SendMessage(minigame.TGameMessage msg, bool isSendToSelf)
        {
            GameGlobal.NetTransfer.SendMessage(msg, isSendToSelf);
        }

        public void SendMessage(string rpcName, List<string> lsRpcParams)
        {
            GameGlobal.RPC.CallServer(rpcName, lsRpcParams);

            if(lsRpcParams == shareRpcParams)
                lsRpcParams.Clear();
        }

    }
}

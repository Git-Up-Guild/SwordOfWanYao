using XClient.Client.Scheme;
using XClient.Client;
using XClient.Common;
using XClient.Entity;
using XClient.Login;
using XClient.Net;
using XClient.Reddot;
using XClient.RPC;

namespace XClient.Game
{
    internal class ModuleSetup
    {
        public static void SetupModules(IModule[] modules)
        {
            INetMonitor netMonitor = GameApp.Instance.GetMonitor((int)EtaMonitorType.Net) as INetMonitor;
            modules[DModuleID.MODULE_ID_NET] = new NetModule(netMonitor);
            modules[DModuleID.MODULE_ID_REDDOT] = new ReddotModule();
            modules[DModuleID.MODULE_ID_SCHEME] = new CSchemeModule();
            modules[DModuleID.MODULE_ID_AGENT] = new AgentModule();
            modules[DModuleID.MODULE_ID_ROOM] = new RoomModule();
            modules[DModuleID.MODULE_ID_NET_TRANSFER] = new NetTransferModule();
            modules[DModuleID.MODULE_ID_RPC] = new RPCModule();
            modules[DModuleID.MODULE_ID_LOGIN] = new LoginModule();
            modules[DModuleID.MODULE_ID_ENTITY] = new EntityModule();
        }
    }
}

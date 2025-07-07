using System;
using gamepol;
namespace XClient.Common.VirtualServer
{
    public interface IMessageHandler
    {
        void OnVirtualServerStarted(VirtualNet net);
        void OnVirtualServerStoped(VirtualNet net);
        void ON_EVENT_ROLE_PART_CREATED(VirtualNet virtualNet);
        void ON_EVENT_ROLE_CREATED(VirtualNet virtualNet);
    }
}
    

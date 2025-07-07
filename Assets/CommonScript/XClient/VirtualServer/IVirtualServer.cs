using System;
namespace XClient.Common.VirtualServer
{
     public interface IVirtualServer
     {
          void Initialize();
          void Start();
          void Stop();
          void RegMsgModule<T>() where T: IMessageHandler, new();
          void UnregMsgModule<T>() where T : IMessageHandler, new();
     }
}

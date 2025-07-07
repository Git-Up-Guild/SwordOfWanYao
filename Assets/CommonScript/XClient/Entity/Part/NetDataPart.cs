
using XClient.Network;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{
    public abstract class NetDataPart : NetObject, IEntityPart
    {
        public IEntity master { get; private set; }

        public int type { get; private set; }

        public override bool IsIndependent => false;

        public void Init(object context = null)
        {
            EntityPartBuildShareContext ctx = context as EntityPartBuildShareContext;

            master = ctx.master;
            type = ctx.type;

            SetupNetID(master.id, 0);
            Start();

            OnInit(ctx.context);
        }

        protected virtual void OnInit(object context) { }

        public virtual void OnReceiveEntityMessage(uint id, object data = null) { }

        public virtual void OnReceiveServerMessage(uint id, object data = null) { }

        public void Reset()
        {
            Stop();
            ClearVarsValue();
            OnReset();
            master = null;
        }

        protected virtual void OnReset() { }

        public virtual void OnUpdate() { }

        public void OnAfterEntityInit() { }

        public override void OnNetVarChange(INetVar var)
        {
            base.OnNetVarChange(var);

            master.SendEntityMessage(EntityMessageID.DataChange);
        }
        //断线重连的时候清理数据调用,注意只需要清理Moudle模块中Create后调用的数据,Create中创建的不要清理了
        public void Clear(int param)
        {

        }
    }
}

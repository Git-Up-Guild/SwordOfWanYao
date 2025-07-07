using gamepol;

namespace XGame
{

    //代理类的回调
    public interface IAgentSink
    {
        void OnPropIntChange(int id, long value);
    }

    //代理接口
    public interface IAgent
    {
        //创建
        bool Create(long id, int playerIdx);

        //销毁
        void Release();

        //初始化 long Prop()
        void SetBatchProp(TPropertySet stProperty,long  ver);

        //获取属性
        long GetProp(int id);

        //设置属性
        void SetProp(int id, long value);

        //设置一下属性监听的sink
        void SetSink(IAgentSink sink);

        //获取所有的属性
        long[] GetAllLongProp();

        //获取属性的版本号
        long GetPropVersion();

        //获取当前代理的角色ID
        long GetRoleID();

        //  获取玩家序号, (进入房间的次序)
        int GetPlayerIndex();
    }
}

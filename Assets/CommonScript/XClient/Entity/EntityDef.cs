
namespace XClient.Entity
{
    /// <summary>
    /// 内置实体类型，扩展类型应该继承此对象
    /// </summary>
    public class EntityInnerType
    {
        public readonly static int Role = 1;                                          //玩家自己
        public readonly static int Person = 2;                                       //其它玩家
        public readonly static int Goods = 3;                                       //物品
    }

    /// <summary>
    /// 内置部件类型，扩展类型应该继承此对象
    /// </summary>
    public class EntityPartInnerType
    {
        public readonly static int Data = 1;                                    //数据部件
        public readonly static int Visible = 2;                             //可视化组件
        public readonly static int Move = 3;                                //移动部件
        public readonly static int Prefab = 4;                                //预制体资源部件
        public readonly static int Lightn = 5;                              //光效部件
        public readonly static int InfoBar = 6;                             //信息条部件
        public readonly static int Audio = 7;                               //声音管理部件
        public readonly static int Anim = 8;                                //动画部件
        public readonly static int Skin = 9;                                //皮肤部件
    }
}

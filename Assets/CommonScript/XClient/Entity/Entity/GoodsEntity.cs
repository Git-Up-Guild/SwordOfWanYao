using XClient.Common;
using XGame.Entity;

namespace XClient.Entity
{
    public class GoodsEntity : BaseEntity, IGoodsEntity
    {
        public int num { get; private set; }

        protected override void OnInit(object context)
        {
            base.OnInit(context);

            GoodsEntityShareCreateContext ctx = context as GoodsEntityShareCreateContext;
            num = ctx.num;

            config = GameGlobal.GameScheme.Goods_0(configId);
        }

        protected override void OnUpdate(object updateContext)
        {
            base.OnUpdate(updateContext);

            var ctx = updateContext as GoodsEntityShareUpdateContext;
            num = ctx.num;
        }

        public void SetNum(int num)
        {
            this.num = num;
        }
    }
}

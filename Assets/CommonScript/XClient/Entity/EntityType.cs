namespace XClient.Entity
{
    public partial class EntityType : EntityInnerType
    {
        public readonly static int NetGameObject = 40; //网络GameObject对象
    }

    public partial class EntityPartType : EntityPartInnerType
    {
        public readonly static int Disciple = 101;

        public readonly static int Seedling = 102;

        public readonly static int TreasureBox = 103;

        public readonly static int Sect = 104;

        public readonly static int Wander = 105;

        //体力部件
        public readonly static int Stamina = 106;
        public readonly static int Antique = 107;//古宝    
        public readonly static int Task = 108; //任务

        //引导部件
        public readonly static int Guide = 109;
        //宗门大阵
        public readonly static int SectFormation = 110;
        //时间奖励物品
        public readonly static int PeriodPrize = 111;

        //邮件部件
        public readonly static int Mail = 112;

        //抽奖部件
        public const int Lottery = 113;

    }
}
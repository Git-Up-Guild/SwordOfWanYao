using XClient.Entity.Net;
using XGame.Entity.Part;
using XGame.MonoState;
using XGame.UI.State;

namespace XClient.Entity.Part
{
    public class CreatureInfoBarPart : BaseInfoBarPart
    {
        public ICreatureInfoProvider provider => master as ICreatureInfoProvider;

        public CreatureInfoBarDesigner designer { get; private set; }

        protected override void OnResLoadComplete()
        {
            designer = root.GetComponent<CreatureInfoBarDesigner>();
        }

        protected override void OnUpdateInfoBar()
        {
            if (provider == null || designer == null)
                return;

            int maxHp = provider.GetCreatureMaxHp();
            int curHp = provider.GetCreatureCurHp();

            if (maxHp <= 0)
                maxHp = 1;

            //血条
            if(designer.hpBar != null)
                designer.hpBar.fillAmount = curHp / (float)maxHp;

            //血量
            if(designer.tmpHpValue != null)
            {
                designer.tmpHpValue.text = $"{curHp}/{maxHp}";
            }

            //名称
            if(designer.tmpName != null)
            {
                designer.tmpName.text = provider.GetCreatureName();
            }

        }
    }
}

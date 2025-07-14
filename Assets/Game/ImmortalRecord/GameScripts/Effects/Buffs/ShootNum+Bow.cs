using UnityEngine;

[CreateAssetMenu(menuName = "Buff/ShootNum+Bow", order = 106)]
public class ShootNumBow : EffectBase
{   
    [Header("增加的弹道数量")]
    public int addCount = 1;

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        RuntimeSoldierAttributeHub.Instance.Modify(SoldierType.Archer, attr => attr.projectileCount += addCount);
        Debug.Log("弓兵弹道数量+1");
    }
}
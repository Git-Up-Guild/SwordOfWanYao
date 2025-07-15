using UnityEngine;

[CreateAssetMenu(menuName = "Buff/ShootNum+Light", order = 106)]
public class ShootNumLight : EffectBase
{   
    [Header("增加的弹道数量")]
    public int addCount = 1;

    public override void ApplyEffect()
    {
        RuntimeSoldierAttributeHub.Instance.Modify(SoldierType.LightMonk, attr => attr.projectileCount += addCount);
        Debug.Log("弓兵弹道数量+1");
    }
}
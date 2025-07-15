using UnityEngine;

[CreateAssetMenu(menuName = "Card/ChooseBlade", order = 200)]
public class ChooseBlade : EffectBase
{
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.AddToSpawnList(SoldierType.Blade);
            Debug.Log("刀兵已加入可选兵种列表");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance 未初始化，无法添加刀兵");
        }
    }
}

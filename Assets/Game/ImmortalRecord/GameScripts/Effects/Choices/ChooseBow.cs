using UnityEngine;

[CreateAssetMenu(menuName = "Card/ChooseBow", order = 200)]
public class ChooseBow : EffectBase
{
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.AddToSpawnList(SoldierType.Archer);
            Debug.Log("弓兵已加入可选兵种列表");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance 未初始化，无法添加弓兵");
        }
    }
}

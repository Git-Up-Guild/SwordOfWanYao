using UnityEngine;

[CreateAssetMenu(menuName = "Card/ChooseFire", order = 104)]
public class ChooseFire : EffectBase
{
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.AddToSpawnList(SoldierType.FireMage);
            Debug.Log("火法师");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance 未初始化，无法添加火法师");
        }


    }
}

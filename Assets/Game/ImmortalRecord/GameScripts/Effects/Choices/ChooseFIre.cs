using UnityEngine;
using SwordOfWanYao;
[CreateAssetMenu(menuName = "Card/ChooseFire", order = 200)]
public class ChooseFire : EffectBase
{ 
   

    public override void ApplyEffect(SoldierModel soldierModel)
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.AddToSpawnList(SoldierType.FireMage);
            Debug.Log("火法师已加入可选兵种列表");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance 未初始化，无法添加火法师");
        }

      
    }
}

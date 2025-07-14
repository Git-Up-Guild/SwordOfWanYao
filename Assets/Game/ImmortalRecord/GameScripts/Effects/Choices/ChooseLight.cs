using UnityEngine;

[CreateAssetMenu(menuName = "Card/ChooseLight", order = 200)]
public class ChooseLight : EffectBase
{
    public override void ApplyEffect(SoldierModel soldierModel)
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.AddToSpawnList(SoldierType.LightMonk);
            Debug.Log("光罗汉已加入可选兵种列表");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance 未初始化，无法添加光罗汉");
        }
    }
}

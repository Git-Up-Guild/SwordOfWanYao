using UnityEngine;
using SwordOfWanYao;

[CreateAssetMenu(menuName = "Card/ChooseWind", order = 200)]
public class ChooseWind : EffectBase
{
    public override void ApplyEffect()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.AddToSpawnList(SoldierType.WindPriest);
            Debug.Log("风祭司已加入可选兵种列表");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance 未初始化，无法添加风祭司");
        }
        
    }
}

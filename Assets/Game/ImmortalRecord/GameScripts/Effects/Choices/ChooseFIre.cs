using UnityEngine;

[CreateAssetMenu(menuName = "Card/ChooseFire", order = 200)]
public class ChooseFire : EffectBase
{
    public override void ApplyEffect()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.AddToSpawnList(SoldierType.FireMage);
            Debug.Log("��ʦ");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance δ��ʼ�����޷����ӻ�ʦ");
        }


    }
}

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
            Debug.Log("��ʦ�Ѽ����ѡ�����б�");
        }
        else
        {
            Debug.LogError("SpawnManager.Instance δ��ʼ�����޷���ӻ�ʦ");
        }

      
    }
}

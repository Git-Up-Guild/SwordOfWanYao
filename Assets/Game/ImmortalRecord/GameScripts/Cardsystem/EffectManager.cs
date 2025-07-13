using System.Collections.Generic;
using UnityEngine;
using SwordOfWanYao;

[CreateAssetMenu(menuName = "Buffmanager", order = 100)]
public class BuffManager : ScriptableObject
{
    //当前生效的所有增益
    private List<EffectBase> activeBuffs = new List<EffectBase>();
    //添加新增益
    public void AddBuff(EffectBase buff)
    {
        if (!activeBuffs.Contains(buff))
        {
            activeBuffs.Add(buff);
            Debug.Log($"Added buff: {buff.name}");
        }
    }
    //移除增益

    public void RemoveBuff(EffectBase buff)
    {
        if (activeBuffs.Contains(buff))
        {
            activeBuffs.Remove(buff);
            Debug.Log($"Removed buff: {buff.name}");
        }
    }
    //应用增益

    public void ApplyAllBuffs(SoldierController soldierController,SoldierModel soldierModel)
    {
        foreach (var buff in activeBuffs)
        {
            buff.ApplyEffect(soldierController,soldierModel);
        }
    }
}
//以下为草稿，等刷兵相关脚本写完后加入到其中
//SoldierModel生成时应用所有增益
// public class SoldierModel : MonoBehaviour
// {
//     public int BaseHealth;
//     public int Health;

//     public BuffManager buffManager; // 赋值为全局唯一的BuffManager

//     void Start()
//     {
//         buffManager.ApplyAllBuffs(this);
//     }
// }
//新buff生效时刷新到场上所有soldier
// buffManager.AddBuff(healthAllBuff);

// foreach (var soldier in FindObjectsOfType<SoldierModel>())
// {
//     buffManager.ApplyAllBuffs(soldier);
// }
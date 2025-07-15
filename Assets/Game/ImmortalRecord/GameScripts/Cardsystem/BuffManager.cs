using System.Collections.Generic;
using UnityEngine;
using SwordOfWanYao;

[CreateAssetMenu(menuName = "Buffmanager", order = 100)]
public class BuffManager : ScriptableObject
{
    // 当前生效的所有增益
    // 我们把它改成 public 但只在内部 set，方便外部查看但不允许直接修改列表
    public List<EffectBase> activeBuffs { get; private set; } = new List<EffectBase>();
    //添加新增益

    // --- 这是新的重置方法 ---
    /// <summary>
    /// 在新游戏开始时调用，清空所有已激活的Buff。
    /// </summary>
    public void ResetBuffs()
    {
        activeBuffs.Clear();
        Debug.Log("[BuffManager] 所有激活的Buff已被重置。");
    }


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

    public void ApplyAllBuffs()
    {
        foreach (var buff in activeBuffs)
        {
            buff.ApplyEffect();
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
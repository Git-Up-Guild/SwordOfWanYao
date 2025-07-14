using UnityEngine;
using System.Collections.Generic;

public class RuntimeSoldierAttributeHub : MonoBehaviour
{
    public static RuntimeSoldierAttributeHub Instance { get; private set; }

    private Dictionary<SoldierType, SoldierAttributeSO> m_runtimeAttributes;

    [SerializeField] private List<SoldierDataBase> m_allSoldierDataBases;

    public List<SoldierType> AllSoldierTypes => new List<SoldierType>(m_runtimeAttributes.Keys);

    public IEnumerable<(SoldierType type, SoldierAttributeSO attr)> GetAllAttributePairs()
    {
        foreach (var kv in m_runtimeAttributes)
            yield return (kv.Key, kv.Value);
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitRuntimeAttributes();
    }

    private void InitRuntimeAttributes()
    {
        m_runtimeAttributes = new Dictionary<SoldierType, SoldierAttributeSO>();

        foreach (var db in m_allSoldierDataBases)
        {
            var so = ScriptableObject.CreateInstance<SoldierAttributeSO>();
            so.CopyFrom(db.attributes);
            m_runtimeAttributes[db.soldierType] = so;
        }
    }

    public SoldierAttributeSO Get(SoldierType type)
    {
        return m_runtimeAttributes.TryGetValue(type, out var so) ? so : null;
    }

    public void Modify(SoldierType type, System.Action<SoldierAttributeSO> modifier)
    {
        if (m_runtimeAttributes.TryGetValue(type, out var so))
        {
            modifier?.Invoke(so);
        }
    }

    //示例：让神弓手的攻击力增加百分之50（无需修改局内+局外两遍，数据自动应用局内士兵）
    private void IncreaseArcherAttackPower()
    {
        RuntimeSoldierAttributeHub.Instance.Modify(
            SoldierType.Archer,
            attr => attr.attackPowerMutiplier *= 1.5f
        );

    }

}
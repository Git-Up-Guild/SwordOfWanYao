using UnityEngine;
using System.Collections.Generic;

public class RuntimeSoldierSkillHub : MonoBehaviour
{
    public static RuntimeSoldierSkillHub Instance { get; private set; }

    // 每种兵种对应的技能副本列表（共享）
    private Dictionary<SoldierType, List<SoldierSkillDataBase>> m_runtimeSkillMap;

    [SerializeField] private List<SoldierDataBase> m_allSoldierDataBases;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitRuntimeSkills();
    }

    private void InitRuntimeSkills()
    {
        m_runtimeSkillMap = new Dictionary<SoldierType, List<SoldierSkillDataBase>>();

        foreach (var db in m_allSoldierDataBases)
        {
            var clonedList = new List<SoldierSkillDataBase>();
            foreach (var skill in db.skills)
            {
                var copy = Instantiate(skill);
                clonedList.Add(copy);
            }
            m_runtimeSkillMap[db.soldierType] = clonedList;
        }
    }

    public List<SoldierSkillDataBase> GetSkills(SoldierType type)
    {
        return m_runtimeSkillMap.TryGetValue(type, out var list) ? list : null;
    }

    public void Modify(SoldierType type, System.Action<SoldierSkillDataBase> modifier)
    {
        if (m_runtimeSkillMap.TryGetValue(type, out var list))
        {
            foreach (var skill in list)
            {
                modifier?.Invoke(skill);
            }
        }
    }

    //实例：让神弓手的箭矢接触爆炸（无需修改局内+局外两遍，数据自动应用局内士兵）
    private void SetArrowCanExplode()
    {
        RuntimeSoldierSkillHub.Instance.Modify(SoldierType.Archer, skill =>
        {
            if (skill is ProjectileSkillData pd)
                pd.canExplode = true;
        });

    }

}

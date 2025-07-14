using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    private class SpawnData
    {
        public float Interval = 10f;      // 固定刷新间隔
        public int Cap;                   // 最大存量
        public float NextSpawnTime;       // 下一次触发时间
    }

    [SerializeField] private readonly HashSet<SoldierType> _runtimeSpawnList = new HashSet<SoldierType>();
    private Dictionary<SoldierType, SpawnData> _spawnConfigs = new Dictionary<SoldierType, SpawnData>();
    [SerializeField] private Dictionary<SoldierType, HashSet<SoldierModel>> m_aliveAllies = new Dictionary<SoldierType, HashSet<SoldierModel>>();
    private List<IAllySpawnPoint> _spawnPoints = new List<IAllySpawnPoint>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        foreach (var (type, attr) in RuntimeSoldierAttributeHub.Instance.GetAllAttributePairs())
        {

            if (type == SoldierType.Spear) continue;

            _spawnConfigs[type] = new SpawnData
            {
                Cap = attr.maxExistCount,
                NextSpawnTime = Time.time + 10f
            };
            m_aliveAllies[type] = new HashSet<SoldierModel>();
            
        }
    }

    private void Update()
    {
        float now = Time.time;
        foreach (var type in _runtimeSpawnList)
        {
            if (!_spawnConfigs.TryGetValue(type, out var cfg)) continue;

            if (now >= cfg.NextSpawnTime && m_aliveAllies[type].Count < cfg.Cap)
            {
                foreach (var pt in _spawnPoints)
                {
                    var ally = pt.Spawn(type);
                    if (ally != null)
                        RegisterAlly(ally);
                }
                cfg.NextSpawnTime = now + cfg.Interval;
            }
        }
    }

    public void RegisterSpawnPoint(IAllySpawnPoint pt)
    {
        if (!_spawnPoints.Contains(pt))
            _spawnPoints.Add(pt);
    }

    public void UnregisterSpawnPoint(IAllySpawnPoint pt)
    {
        _spawnPoints.Remove(pt);
    }

    public void AddToSpawnList(SoldierType type)
    {
        if (!_runtimeSpawnList.Contains(type))
            _runtimeSpawnList.Add(type);

        if (!_spawnConfigs.TryGetValue(type, out var cfg))
        {
            var attr = RuntimeSoldierAttributeHub.Instance.Get(type);
            if (attr == null)
            {
                Debug.LogWarning($"[SpawnManager] 未找到 {type} 的属性数据");
                return;
            }

            _spawnConfigs[type] = new SpawnData
            {
                Cap = attr.maxExistCount,
                NextSpawnTime = Time.time + 10f
            };
        }

        if (!m_aliveAllies.ContainsKey(type))
            m_aliveAllies[type] = new HashSet<SoldierModel>();
    }

    private void RegisterAlly(SoldierModel ally)
    {
        m_aliveAllies[ally.Type].Add(ally);
        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.Died,
            ally,
            OnAllyDeath
        );
    }

    private void OnAllyDeath(IEventData evt)
    {
        var data = (SoldierModel.SoldierStateChangeData)evt;
        var dead = (SoldierModel)data.Source;
        m_aliveAllies[dead.Type].Remove(dead);
        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.Died,
            dead,
            OnAllyDeath
        );
    }
}

public interface IAllySpawnPoint
{
    SoldierModel Spawn(SoldierType type);
}

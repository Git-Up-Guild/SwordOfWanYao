using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    // 按难度从易到难排列
    private static readonly SoldierType[] DifficultyGradient = new[]
    {
        SoldierType.Swiftbeak,     // 迅鸟怪
        SoldierType.DarkArcher,    // 魔弓手
        SoldierType.PlagueHealer,  // 魔疫医
        SoldierType.Exploder,      // 自爆怪
        SoldierType.IronWarlord    // 全甲魔将
    };

    private List<IEnemySpawnPoint> _points = new List<IEnemySpawnPoint>();
    private float _gameStartTime;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _gameStartTime = Time.time;
        // 初始化每个点的 NextSpawnTime
        foreach (var pt in _points)
            pt.NextSpawnTime = _gameStartTime + pt.SpawnDelay;
    }

    public void RegisterSpawnPoint(IEnemySpawnPoint pt)
    {
        if (!_points.Contains(pt))
            _points.Add(pt);
    }
    
    public void UnregisterSpawnPoint(IEnemySpawnPoint pt)
    {
        if (_points.Contains(pt))
            _points.Remove(pt);
    }

    private void Update()
    {
        float now = Time.time;
        float elapsed = now - _gameStartTime;

        foreach (var pt in _points)
        {
            if (now < pt.NextSpawnTime) continue;

            // 计算经过了多少个 40s 周期
            int wavesPassed = Mathf.Max(0, Mathf.FloorToInt((elapsed - pt.SpawnDelay) / 40f));
            int spawnCount = pt.BaseSpawnCount + wavesPassed * pt.SpawnIncrement;

            for (int i = 0; i < spawnCount; i++)
            {
                var type = PickRandomType(elapsed);
                var enemy = pt.Spawn(type);
                // 注册死亡、血量显示等
                RegisterEnemy(enemy);
            }

            pt.NextSpawnTime += pt.Interval;
        }
    }

    private SoldierType PickRandomType(float elapsed)
    {
        // 难度随时间提升，每 40s 解锁一个更难的
        int maxIndex = Mathf.Min(
            DifficultyGradient.Length - 1,
            Mathf.FloorToInt(elapsed / 40f)
        );
        int idx = Random.Range(0, maxIndex + 1);
        return DifficultyGradient[idx];
    }

    private void RegisterEnemy(SoldierModel enemy)
    {
        // 和之前的 RegisterAlly 类似，监听 Died 事件
        EventManager.Instance.Subscribe<IEventData>(
            SoldierEventNames.Died,
            enemy,
            _ => UnregisterEnemy(enemy)
        );
    }

    private void UnregisterEnemy(SoldierModel dead)
    {
        EventManager.Instance.Unsubscribe<IEventData>(
            SoldierEventNames.Died,
            dead,
            _ => UnregisterEnemy(dead)
        );
    }
}

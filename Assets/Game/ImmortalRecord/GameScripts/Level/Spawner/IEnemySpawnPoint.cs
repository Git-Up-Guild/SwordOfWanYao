public interface IEnemySpawnPoint
{
    float SpawnDelay { get; }           // 刷新延迟：最终防线 0s，防御塔 5s
    int BaseSpawnCount { get; }         // 基础刷怪数：最终防线 2，防御塔 1
    int SpawnIncrement { get; }         // 每 40s 增量：最终防线 +2，防御塔 +1
    float Interval { get; }             // 固定刷新间隔：10s
    float NextSpawnTime { get; set; }   // 下次触发时刻（Time.time）
    SoldierModel Spawn(SoldierType type); // 实际刷怪，返回生成的模型用来注册死亡等
}

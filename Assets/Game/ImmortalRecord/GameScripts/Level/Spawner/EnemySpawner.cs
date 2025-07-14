using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class EnemySpawner : MonoBehaviour, IEnemySpawnPoint
{
    public float Interval => 10f;
    public float SpawnDelay { get; private set; }
    public int BaseSpawnCount { get; private set; }
    public int SpawnIncrement { get; private set; }
    public float NextSpawnTime { get; set; }

    [Header("生成区域类型")]
    [SerializeField] private SpawnAreaType areaType;
    [SerializeField] private BoxCollider2D boxArea;
    [SerializeField] private CircleCollider2D circleArea;

    [Serializable]
    private struct SpawnInfo
    {
        public SoldierType type;
        public GameObject prefab;
    }
    [Header("敌人类型与预制体映射")]
    [SerializeField] private List<SpawnInfo> spawnInfos;

    private void Start()
    {
        // 区分防御塔（圆形）与最终防线（方形）
        if (GetComponent<CircleCollider2D>() != null)
        {
            SpawnDelay = 5f;
            BaseSpawnCount = 1;
            SpawnIncrement = 1;
        }
        else if (GetComponent<BoxCollider2D>() != null)
        {
            SpawnDelay = 0f;
            BaseSpawnCount = 2;
            SpawnIncrement = 2;
        }
        else
        {
            throw new Exception("EnemySpawner 必须挂 CircleCollider2D（防御塔）或 BoxCollider2D（最终防线）");
        }

        // 注册到管理器
        EnemySpawnManager.Instance.RegisterSpawnPoint(this);
    }

    private void OnDisable()
    {
        if (EnemySpawnManager.Instance != null)
            EnemySpawnManager.Instance.UnregisterSpawnPoint(this);
    }

    public SoldierModel Spawn(SoldierType type)
    {
        // 找到对应类型的预制体
        var info = spawnInfos.Find(x => x.type == type);
        if (info.prefab == null)
            return null;

        // 随机点生成
        Vector2 pos = GetRandomPointInArea();
        var go = Instantiate(info.prefab, pos, Quaternion.identity);

        // 初始化控制器/模型
        var controller = go.GetComponent<SoldierController>();
        if (controller != null)
            controller.Init();

        return go.GetComponent<SoldierModel>();
    }

    private Vector2 GetRandomPointInArea()
    {
        switch (areaType)
        {
            case SpawnAreaType.Rectangle:
                var b = boxArea.bounds;
                return new Vector2(
                    UnityEngine.Random.Range(b.min.x, b.max.x),
                    UnityEngine.Random.Range(b.min.y, b.max.y)
                );

            case SpawnAreaType.Circle:
                Vector2 c = circleArea.bounds.center;
                float r = circleArea.radius * circleArea.transform.lossyScale.x;
                float a = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float d = Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f)) * r;
                return c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d;

            default:
                return transform.position;
        }
    }
}

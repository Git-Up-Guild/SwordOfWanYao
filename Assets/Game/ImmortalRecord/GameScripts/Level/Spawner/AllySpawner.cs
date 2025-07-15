using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class AllySpawner : MonoBehaviour, IAllySpawnPoint
{
    [Header("生成区域类型")]
    [SerializeField] private SpawnAreaType areaType;
    [SerializeField] private BoxCollider2D boxArea;
    [SerializeField] private CircleCollider2D circleArea;

    [System.Serializable]
    private struct SpawnInfo
    {
        public SoldierType type;
        public GameObject prefab;
    }
    [SerializeField] private List<SpawnInfo> spawnInfos;

    private void Start()
    {
        SpawnManager.Instance.RegisterSpawnPoint(this);
    }

    private void OnDisable()
    {
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.UnregisterSpawnPoint(this);
    }

    public SoldierModel Spawn(SoldierType type)
    {
        var info = spawnInfos.Find(x => x.type == type);
        if (info.prefab == null) return null;

        Vector2 pos = GetRandomPointInArea();
        var go = Instantiate(info.prefab, pos, Quaternion.identity);
        var controller = go.GetComponent<SoldierController>();
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
                    Random.Range(b.min.x, b.max.x),
                    Random.Range(b.min.y, b.max.y)
                );
            case SpawnAreaType.Circle:
                Vector2 c = circleArea.bounds.center;
                float r = circleArea.radius * circleArea.transform.lossyScale.x;
                float a = Random.Range(0f, Mathf.PI * 2f);
                float d = Mathf.Sqrt(Random.Range(0f, 1f)) * r;
                return c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d;
            default:
                return (Vector2)transform.position;
        }
    }
}

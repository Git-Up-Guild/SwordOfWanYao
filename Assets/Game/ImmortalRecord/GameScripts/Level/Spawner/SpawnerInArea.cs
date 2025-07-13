using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SpawnAreaType { Rectangle, Circle }
public enum SpawnMode
{
    ByProbabilityAndLimit,  // 原逻辑
    SpawnAllThenStop        // 刷新所有士兵后停止
}

public class SpawnerInArea : MonoBehaviour
{

    [Header("生成区域配置")]
    [SerializeField] private SpawnAreaType m_areaType;
    [SerializeField] private BoxCollider2D m_boxArea;
    [SerializeField] private CircleCollider2D m_circleArea;

    [Header("生成模式")]
    [SerializeField] private SpawnMode m_spawnMode = SpawnMode.ByProbabilityAndLimit;


    [Header("生成参数")]
    [SerializeField] private List<GameObject> m_soldierPrefabs; // 士兵预制体列表
    [SerializeField] private float m_spawnInterval = 0.5f;
    [SerializeField] private float m_spawnMaxCount = 4;
    private float m_spawnCurCount = 0;

    private Coroutine m_spawnCoroutine;

    private void OnEnable()
    {
        StartSpawning();
    }

    private void OnDisable()
    {
        StopSpawning();
    }

    public void StartSpawning()
    {
        if (m_spawnCoroutine == null)
        {
            m_spawnCoroutine = StartCoroutine(SpawnRoutine());

        }
    }

    public void StopSpawning()
    {
        if (m_spawnCoroutine != null)
        {
            StopCoroutine(m_spawnCoroutine);
            m_spawnCoroutine = null;
        }
    }
    private IEnumerator SpawnRoutine()
    {
        if (m_spawnMode == SpawnMode.SpawnAllThenStop)
        {
            foreach (var prefab in m_soldierPrefabs)
            {
                Vector2 pos = GetRandomPointInArea();
                InstantiateSoldier(prefab, pos);
                yield return new WaitForSeconds(m_spawnInterval);
            }
        }
        else
        {
            while (m_spawnCurCount < m_spawnMaxCount)
            {
                Spawn();
                m_spawnCurCount++;
                yield return new WaitForSeconds(m_spawnInterval);
            }
        }
    }

    public void Spawn()
    {

        int index = Random.Range(0, m_soldierPrefabs.Count);
        GameObject selectedPrefab = m_soldierPrefabs[index];

        Vector2 pos = GetRandomPointInArea();
        InstantiateSoldier(selectedPrefab, pos);
    }

    private void InstantiateSoldier(GameObject prefab, Vector2 pos)
    {
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        SoldierController controller = obj.GetComponent<SoldierController>();
        controller.Init();
        
    }

    private Vector2 GetRandomPointInArea()
    {
        switch (m_areaType)
        {

            case SpawnAreaType.Rectangle:
                Bounds bounds = m_boxArea.bounds;
                return new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );

            case SpawnAreaType.Circle:
                Vector2 center = m_circleArea.bounds.center;
                float radius = m_circleArea.radius * m_circleArea.transform.lossyScale.x;

                float angle = Random.Range(0f, Mathf.PI * 2f);
                float r = Mathf.Sqrt(Random.Range(0f, 1f)) * radius;

                return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;

            default:
                return Vector2.zero;
        }
    }


}

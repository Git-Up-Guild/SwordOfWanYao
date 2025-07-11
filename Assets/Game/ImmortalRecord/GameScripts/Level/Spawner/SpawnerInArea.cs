using UnityEngine;
using System.Collections;

public enum SpawnAreaType { Rectangle, Circle }

public class SpawnerInArea : MonoBehaviour
{

    [SerializeField] SpawnAreaType m_areaType;
    [SerializeField] private GameObject m_soldierPrefab;
    [SerializeField] private BoxCollider2D m_boxArea;
    [SerializeField] private CircleCollider2D m_circleArea;
    [SerializeField] private float m_spawnInterval = 0.5f;
    [SerializeField] private float m_spawnMaxCount = 4;
    [SerializeField] private float m_spawnCurCount = 0;
    
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
        while (m_spawnCurCount < m_spawnMaxCount)
        {
            Spawn();
             m_spawnCurCount++;

            yield return new WaitForSeconds(m_spawnInterval); 

        }
    }

    public void Spawn()
    {
        Vector2 pos = GetRandomPointInArea();
        InstantiateSoldier(pos);
    }

    private void InstantiateSoldier(Vector2 pos)
    {
        GameObject obj = Instantiate(m_soldierPrefab, pos, Quaternion.identity);
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

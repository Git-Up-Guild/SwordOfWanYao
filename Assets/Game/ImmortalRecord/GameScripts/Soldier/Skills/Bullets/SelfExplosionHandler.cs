using UnityEngine;
using System.Collections;

public class SelfExplosionHandler : MonoBehaviour
{
    [Header("爆炸参数")]
    public GameObject explosionPrefab;
    public float explosionScaleMultiplier = 1f;
    public int explosionDamage = 30;

    private SoldierModel m_model;

    private void Awake()
    {
        m_model = GetComponent<SoldierModel>();
    }

    private void OnEnable()
    {
        if (m_model != null)
        {
            EventManager.Instance.Subscribe<IEventData>(SoldierEventNames.Died, m_model, OnDeath);
        }
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null && m_model != null)
        {
            EventManager.Instance.Unsubscribe<IEventData>(SoldierEventNames.Died, m_model, OnDeath);
        }
    }

    private void OnDeath(IEventData evt)
    {
        StartCoroutine(TriggerExplosion());
    }

    private IEnumerator TriggerExplosion()
    {
        // 生成爆炸体
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.transform.localScale *= explosionScaleMultiplier;

        // 初始化爆炸伤害
        var explosionDamageComp = explosion.GetComponent<ExplosionDamage>();
        if (explosionDamageComp != null)
        {
            explosionDamageComp.Init(m_model, explosionDamage);
        }

        yield return new WaitForSeconds(0.8f);
        Destroy(explosion);
    }
}

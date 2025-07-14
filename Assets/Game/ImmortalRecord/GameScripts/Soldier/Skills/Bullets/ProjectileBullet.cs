using UnityEngine;

public class ProjectileBullet : MonoBehaviour
{
    private SoldierModel attacker;
    private Vector2 direction;
    private float speed;
    private int damage;
    private int maxPierce;
    private int pierceCount;
    private Vector3 spawnPosition;
    private const float MaxDistance = 40f;
    private ProjectileExplosion exp;
    private ProjectileSplit split;
    private int explosionDamage;
    private float explosionScaleMutiplier;
    private GameObject explosionPrefab;
    private GameObject splitBulletPrefab;
    private bool canExplode;
    private bool canSplit;

    public void Init(
        SoldierModel attacker,
        Vector2 direction,
        float speed,
        int damage,
        int maxPierce,
        bool canExplode,
        int explosionDamage,
        float explosionScaleMutiplier,
        GameObject explosionPrefab,
        bool canSplit,
        GameObject splitBulletPrefab)
    {
        this.attacker = attacker;
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.maxPierce = maxPierce;
        this.pierceCount = 0;
        spawnPosition = transform.position;

        this.explosionDamage = explosionDamage;
        this.explosionScaleMutiplier = explosionScaleMutiplier;
        this.explosionPrefab = explosionPrefab;

        this.splitBulletPrefab = splitBulletPrefab;

        this.canExplode = canExplode;
        this.canSplit = canSplit;

        // 挂载爆炸
        if (canExplode)
        {
            exp = gameObject.AddComponent<ProjectileExplosion>();
        }
        // 挂载分裂
        if (canSplit)
        {
            split = gameObject.AddComponent<ProjectileSplit>();
        }
    }

    private void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, spawnPosition) > MaxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var targetModel = other.GetComponentInParent<SoldierModel>();
        if (targetModel == null || targetModel.Camp == attacker.Camp || targetModel.IsDead) return;

        DamageApplyer.ApplyDamage(attacker, targetModel, damage, targetModel.transform.position);

        if (canExplode)
            exp.Init(attacker, explosionDamage, explosionScaleMutiplier, explosionPrefab);

        if (canSplit)
            split.Init(attacker, direction, damage, speed, splitBulletPrefab);

        pierceCount++;
        if (pierceCount > maxPierce)
        {
            Destroy(gameObject);
        }
    }
}

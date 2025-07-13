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

    public void Init(SoldierModel attacker, Vector2 direction, float speed, int damage, int maxPierce)
    {
        this.attacker = attacker;
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;
        this.maxPierce = maxPierce;
        this.pierceCount = 0;
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

        DamageApplyer.ApplyDamage(attacker, targetModel, damage);

        pierceCount++;
        if (pierceCount > maxPierce)
        {
            Destroy(gameObject);
        }
    }
}

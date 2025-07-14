using UnityEngine;

public class SplitProjectileBullet : MonoBehaviour
{
    private SoldierModel attacker;
    private Vector2 direction;
    private float speed;
    private int damage;

    private Vector3 spawnPosition;
    private const float MaxDistance = 40f; // 飞行最大距离，超过后自动销毁

    public void Init(SoldierModel attacker, Vector2 dir, float speed, int damage)
    {
        this.attacker = attacker;
        this.direction = dir.normalized;
        this.speed = speed;
        this.damage = damage;

        spawnPosition = transform.position;
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
        var target = other.GetComponentInParent<SoldierModel>();
        if (target == null || target.Camp == attacker.Camp || target.IsDead)
            return;

        DamageApplyer.ApplyDamage(attacker, target, damage, target.transform.position);

        Destroy(gameObject);
    }
}

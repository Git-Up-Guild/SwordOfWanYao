using UnityEngine;

public class ProjectileSplit : MonoBehaviour
{
    // 初始化分裂：在当前位置生成 3 支子弹
    public void Init(
        SoldierModel attacker,
        Vector2 originalDirection,
        int damage,
        float speed,
        GameObject bulletPrefab)
    {

        float baseAngle = Mathf.Atan2(originalDirection.y, originalDirection.x) * Mathf.Rad2Deg;

        // 计算每支子弹的旋转偏移
        for (int i = 0; i < 3; i++)
        {
            float angleOffset = (i - 1) * 15f;
            float zAngle = baseAngle + angleOffset;

            Vector2 dir = Quaternion.Euler(0, 0, zAngle - 90) * Vector2.right;

            GameObject go = Instantiate(
                bulletPrefab,
                transform.position,
                Quaternion.Euler(0, 0, zAngle)
            );

            var b = go.GetComponent<SplitProjectileBullet>();
            b.Init(attacker, dir, speed, damage);
        }
    }
}

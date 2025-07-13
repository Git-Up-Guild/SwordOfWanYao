using System.Collections;
using UnityEngine;

public class ProjectileExplosion : MonoBehaviour
{

    //生成一个带圆形触发器的特效，检测一次周围敌人并伤害，0.8s后销毁
    public void Init(
        SoldierModel attacker,
        int damage,
        float scaleMultiplier,
        GameObject explosionPrefab)
    {

        GameObject vfx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        vfx.transform.localScale *= scaleMultiplier;

        var explosionDamage = explosionPrefab.GetComponent<ExplosionDamage>();
        explosionDamage.Init(attacker, damage);

        Destroy(vfx, 0.7f);
    }
}


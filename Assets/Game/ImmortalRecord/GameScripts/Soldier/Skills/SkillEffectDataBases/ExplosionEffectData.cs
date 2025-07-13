using UnityEngine;

[CreateAssetMenu(menuName = "Soldier System/SkillEffects/ExplosionEffect", order = 16)]
public class ExplosionEffectData : SkillEffectDataBase
{
    [Header("爆炸效果设置")]
    public GameObject explosionPrefab;
    public int damage;
    public float radius;
}
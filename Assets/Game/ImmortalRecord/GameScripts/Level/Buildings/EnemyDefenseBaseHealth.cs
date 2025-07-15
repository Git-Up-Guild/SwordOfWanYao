using System.Threading;
using UnityEngine;

public class EnemyDefenseBaseHealth : MonoBehaviour, IDestructible
{
    [SerializeField] public int maxHealth = 1000;
    [SerializeField] private SoldierCamp camp;
    [SerializeField] private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, SoldierModel attacker)
    {

        currentHealth -= damage;
        if (currentHealth <= 0)
        {

            Time.timeScale = 0;
            //调用结算ui窗口
            //xx.Instance.xx()...
            Destroy(gameObject);
        }
    }

    public SoldierCamp GetCamp() => camp;
    public int GetHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    
}
using UnityEngine;

public class AllyDefenseTowerHealth : MonoBehaviour, IDestructible
{
    [SerializeField] public int maxHealth = 1000;
    [SerializeField] private SoldierCamp camp;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, SoldierModel attacker)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);

        }
    }

    public SoldierCamp GetCamp() => camp;
    public int GetHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

}
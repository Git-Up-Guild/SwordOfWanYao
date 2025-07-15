using UnityEngine;

public class EnemyDefenseTowerHealth : MonoBehaviour, IDestructible
{
    [SerializeField] public int maxHealth = 1000;
    [SerializeField] private SoldierCamp camp;
    [SerializeField] private GameObject AllyDefenseTower;
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
            Instantiate(AllyDefenseTower, transform.position, Quaternion.identity);
            EnemySpawnManager.Instance.UnregisterSpawnPoint(GetComponent<IEnemySpawnPoint>());
            Destroy(gameObject);

        }
    }

    public SoldierCamp GetCamp() => camp;
    public int GetHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    
}
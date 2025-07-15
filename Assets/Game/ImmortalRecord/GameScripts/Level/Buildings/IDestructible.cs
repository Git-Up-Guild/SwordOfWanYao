public interface IDestructible
{
    void TakeDamage(int damage, SoldierModel attacker);
    SoldierCamp GetCamp();
    int GetHealth();
    int GetMaxHealth();
}
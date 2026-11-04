
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    // Kor nær fienden må vere for å bli angripen
    public float range = 1.5f;

    // Kor mykje skade kvart angrep gjer
    public int damage = 3;

    // Sekund mellom kvart angrep
    public float attackCooldown = 0.8f;

    // Tidspunktet me kan angripe neste gong
    private float nextAttackTime = 0f;

    void Update()
    {
        // Ventar til tårnet kan angripe igjen
        if (Time.time < nextAttackTime)
            return;

        // Finn alle fiendane som lever
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        EnemyHealth closestEnemy = null;
        float closestDistance = range;

        // Finn den næraste fienden innanfor rekkevidda
        foreach (EnemyHealth enemy in enemies)
        {
            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        // Angrip dersom me fann ein fiende
        if (closestEnemy != null)
        {
            // Gir fienden skade
            closestEnemy.TakeDamage(damage);

            // Bestemmer når neste angrep kan skje
            nextAttackTime = Time.time + attackCooldown;
        }
    }
}

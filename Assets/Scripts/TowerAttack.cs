
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public float range = 3f;
    public float fireRate = 2f;
    public GameObject bulletPrefab;

    private float cooldown = 0f;

    void Update()
    {
        cooldown -= Time.deltaTime;

        if (cooldown > 0)
            return;

        EnemyHealth target = FindTarget();

        if (target != null)
        {
            Debug.Log("Tower found an enemy!");
            
            GameObject bullet = Instantiate(
                bulletPrefab,
                transform.position,
                Quaternion.identity
            );

            bullet.GetComponent<Bullet>().SetTarget(target);

            cooldown = 1f / fireRate;
        }
    }

    EnemyHealth FindTarget()
    {
        EnemyHealth closest = null;
        float shortestDistance = range;

        EnemyHealth[] enemies =
            FindObjectsByType<EnemyHealth>(
                FindObjectsSortMode.None
            );

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy.currentHealth <= 0)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }
}

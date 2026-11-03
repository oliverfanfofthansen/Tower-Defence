
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;

    private EnemyHealth target;

    public void SetTarget(EnemyHealth enemy)
    {
        target = enemy;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            speed * Time.deltaTime
        );

        float distance = Vector3.Distance(
            transform.position,
            target.transform.position
        );

        if (distance < 0.1f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}


using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // Punkta fienden skal følgje
    public Transform[] waypoints;

    // Kor fort fienden bevegar seg
    public float speed = 2f;

    // Kva waypoint fienden er på veg mot
    private int currentWaypoint = 0;

    void Update()
    {
        // Stoppar dersom det ikkje finst waypoints
        if (waypoints == null || waypoints.Length == 0)
            return;

        // Finn neste punkt på ruta
        Transform target = waypoints[currentWaypoint];

        // Flyttar fienden mot punktet
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Sjekkar om fienden har nådd punktet
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            // Går vidare til neste waypoint
            currentWaypoint++;

            // Sjekkar om fienden har nådd slutten
            if (currentWaypoint >= waypoints.Length)
            {
                // Hentar helsa fienden har igjen
                EnemyHealth health = GetComponent<EnemyHealth>();

                // Standard skade dersom EnemyHealth manglar
                int damage = 1;

                if (health != null)
                {
                    // Fienden gir skade tilsvarande HP han har igjen
                    damage = health.currentHealth;
                }

                // Sjekkar om GameManager finst
                if (GameManager.Instance != null)
                {
                    // Trekkjer HP frå spelaren
                    GameManager.Instance.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("GameManager manglar!");
                }

                // Fjernar fienden frå kartet
                Destroy(gameObject);
            }
        }
    }
}

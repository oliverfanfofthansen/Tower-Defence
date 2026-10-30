
using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] waypoints;

    public int enemyCount = 5;
    public float spawnDelay = 1.5f;

    IEnumerator Start()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy = Instantiate(
                enemyPrefab,
                waypoints[0].position,
                Quaternion.identity
            );

            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
            movement.waypoints = waypoints;

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}

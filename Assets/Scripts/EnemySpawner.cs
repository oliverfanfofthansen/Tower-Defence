
using UnityEngine;
using System.Collections;

// Gjer at me kan endre kvar wave i Unity Inspector
[System.Serializable]
public class WaveData
{
    public int normalCount;
    public int mediumCount;
    public int tankCount;
}

public class EnemySpawner : MonoBehaviour
{
    // Dei tre fiendetypane våre
    public GameObject normalEnemy;
    public GameObject mediumEnemy;
    public GameObject tankEnemy;

    // Ruta som fiendane skal følgje
    public Transform[] waypoints;

    // Kor lenge me ventar mellom kvar fiende
    public float spawnDelay = 1.5f;

    // Liste over alle waves
    public WaveData[] waves;

    // Held styr på kva wave me er på
    public int currentWave = 0;

    // Hindrar fleire waves i å starte samtidig
    private bool waveRunning = false;

    // Denne funksjonen skal knytast til Start Wave-knappen
    public void StartNextWave()
    {
        // Ikkje start dersom ei wave allereie køyrer
        if (waveRunning || currentWave >= waves.Length)
            return;

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Missing waypoints!");
            return;
        }

        // Start neste wave
        StartCoroutine(SpawnWave(waves[currentWave]));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        waveRunning = true;
        currentWave++;

        Debug.Log("Starting Wave " + currentWave);

        // Spawn fiendane i denne bølgja
        yield return StartCoroutine(
            SpawnEnemies(normalEnemy, wave.normalCount));

        yield return StartCoroutine(
            SpawnEnemies(mediumEnemy, wave.mediumCount));

        yield return StartCoroutine(
            SpawnEnemies(tankEnemy, wave.tankCount));

        // Vent til alle fiendane er drepne
        // eller har nådd slutten av vegen
        while (FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }

        // Bølgja er ferdig
        waveRunning = false;

        Debug.Log("Wave " + currentWave + " completed!");
    }

    IEnumerator SpawnEnemies(GameObject prefab, int count)
    {
        // Ingen fiendar å lage av denne typen
        if (count <= 0)
            yield break;

        if (prefab == null)
        {
            Debug.LogError("Enemy prefab missing!");
            yield break;
        }

        // Lagar riktig antal fiendar
        for (int i = 0; i < count; i++)
        {
            // Opprett ein fiende ved starten av ruta
            GameObject enemy = Instantiate(
                prefab,
                waypoints[0].position,
                Quaternion.identity
            );

            // Gi fienden waypoint-lista
            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            if (movement != null)
            {
                movement.waypoints = waypoints;
            }

            // Vent før neste fiende blir oppretta
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}

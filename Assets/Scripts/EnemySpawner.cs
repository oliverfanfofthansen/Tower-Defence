
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

    // Ruta fiendane skal følgje
    public Transform[] waypoints;

    // Tid mellom kvar fiende
    public float spawnDelay = 1.5f;

    // Alle waves i spelet
    public WaveData[] waves;

    // Nummeret på siste wave som blei starta
    public int currentWave = 0;

    // Hindrar fleire waves i å køyre samtidig
    private bool waveRunning = false;

    // Belønninga for første wave
    public int startingReward = 50;

    // Kor mykje belønninga aukar for kvar wave
    public int rewardIncrease = 10;

    // Blir kalla når spelaren trykkjer Start Wave
    public void StartNextWave()
    {
        // Ikkje start dersom ei wave allereie køyrer
        // eller dersom alle waves er fullførte
        if (waveRunning || currentWave >= waves.Length)
            return;

        // Sjekkar at waypoint-lista finst
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Missing waypoints!");
            return;
        }

        // Startar neste wave
        StartCoroutine(SpawnWave(waves[currentWave]));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        // Marker at ei wave er aktiv
        waveRunning = true;

        // Aukar wave-nummeret
        currentWave++;

        Debug.Log("Starting Wave " + currentWave);

        // Spawner alle normale fiendar
        yield return StartCoroutine(
            SpawnEnemies(normalEnemy, wave.normalCount));

        // Spawner alle medium-fiendar
        yield return StartCoroutine(
            SpawnEnemies(mediumEnemy, wave.mediumCount));

        // Spawner alle tank-fiendar
        yield return StartCoroutine(
            SpawnEnemies(tankEnemy, wave.tankCount));

        // Ventar til alle fiendane anten er drepne
        // eller har nådd slutten av vegen
        while (FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }

        // Waven er no ferdig
        waveRunning = false;

        Debug.Log("Wave " + currentWave + " completed!");

        // REKNAR UT WAVE-BELØNNINGA
        // Wave 1 = 50$, wave 2 = 60$, wave 3 = 70$ osv.
        int reward = startingReward +
                     (currentWave - 1) * rewardIncrease;

        // Gir spelaren pengar dersom MoneyManager finst
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddCoins(reward);

            Debug.Log(
                "Wave " + currentWave +
                " fullført! Du fekk " + reward + "$!"
            );
        }
        else
        {
            Debug.LogWarning("MoneyManager manglar!");
        }
    }

    IEnumerator SpawnEnemies(GameObject prefab, int count)
    {
        // Stoppar dersom me ikkje skal lage fiendar
        if (count <= 0)
            yield break;

        // Sjekkar om fiende-prefaben finst
        if (prefab == null)
        {
            Debug.LogError("Enemy prefab missing!");
            yield break;
        }

        // Lagar riktig antal fiendar
        for (int i = 0; i < count; i++)
        {
            // Opprettar fienden ved første waypoint
            GameObject enemy = Instantiate(
                prefab,
                waypoints[0].position,
                Quaternion.identity
            );

            // Finn EnemyMovement på fienden
            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            // Gir fienden waypoint-lista
            if (movement != null)
            {
                movement.waypoints = waypoints;
            }

            // Ventar før neste fiende
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}

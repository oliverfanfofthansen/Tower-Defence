
using UnityEngine;
using System.Collections;

// Gjer at kvar wave kan ha forskjellige fiendar
[System.Serializable]
public class WaveData
{
    public int normalCount;
    public int mediumCount;
    public int tankCount;
}

public class EnemySpawner : MonoBehaviour
{
    // Fiendetypane våre
    public GameObject normalEnemy;
    public GameObject mediumEnemy;
    public GameObject tankEnemy;

    // Punkta fiendane følgjer
    public Transform[] waypoints;

    // Pause mellom kvar fiende
    public float spawnDelay = 1.5f;

    // Alle waves i spelet
    public WaveData[] waves;

    // Nummeret på siste wave som blei starta
    public int currentWave = 0;

    // Hindrar to waves i å køyre samtidig
    private bool waveRunning = false;

    // Belønning for første wave
    public int startingReward = 50;

    // Kor mykje belønninga aukar per wave
    public int rewardIncrease = 10;

    // Blir kalla frå Start Wave-knappen
    public void StartNextWave()
    {
        // Sjekkar at waves finst
        if (waves == null)
            return;

        // Ikkje start fleire waves samtidig
        if (waveRunning || currentWave >= waves.Length)
            return;

        // Sjekkar at waypoints er kopla til
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
        // Marker at wave er aktiv
        waveRunning = true;

        // Aukar wave-nummeret
        currentWave++;

        Debug.Log("Starting Wave " + currentWave);

        // Lagar normale fiendar
        yield return StartCoroutine(
            SpawnEnemies(normalEnemy, wave.normalCount));

        // Lagar medium-fiendar
        yield return StartCoroutine(
            SpawnEnemies(mediumEnemy, wave.mediumCount));

        // Lagar tank-fiendar
        yield return StartCoroutine(
            SpawnEnemies(tankEnemy, wave.tankCount));

        // Ventar til alle fiendane er borte
        while (FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }

        // Wave er ferdig
        waveRunning = false;

        Debug.Log("Wave " + currentWave + " completed!");

        // Reknar ut den vanlege wave-belønninga
        int waveReward =
            startingReward + (currentWave - 1) * rewardIncrease;

        // Finn alle Farms som er plasserte på kartet
        FarmIncome[] farms = FindObjectsByType<FarmIncome>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        // Reknar ut kor mykje alle Farms produserer
        int farmReward = 0;

        foreach (FarmIncome farm in farms)
        {
            // Legg til inntekta frå denne farmen
            farmReward += farm.coinsPerWave;
        }

        // Legg saman wave-belønning og Farm-inntekt
        int totalReward = waveReward + farmReward;

        // Betaler ut pengane til spelaren
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddCoins(totalReward);

            Debug.Log(
                "Wave " + currentWave + " completed!" +
                " Wave reward: " + waveReward + "$" +
                " | Farm income: " + farmReward + "$" +
                " | Total: " + totalReward + "$"
            );
        }
        else
        {
            Debug.LogWarning("MoneyManager manglar!");
        }
    }

    IEnumerator SpawnEnemies(GameObject prefab, int count)
    {
        // Ingen fiendar av denne typen
        if (count <= 0)
            yield break;

        // Sjekkar at prefab-en finst
        if (prefab == null)
        {
            Debug.LogError("Enemy prefab missing!");
            yield break;
        }

        // Lagar riktig antal fiendar
        for (int i = 0; i < count; i++)
        {
            // Opprettar ein fiende ved første waypoint
            GameObject enemy = Instantiate(
                prefab,
                waypoints[0].position,
                Quaternion.identity
            );

            // Finn bevegelsesscriptet på fienden
            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            // Gir fienden ruta han skal følgje
            if (movement != null)
            {
                movement.waypoints = waypoints;
            }

            // Ventar før neste fiende kjem
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}

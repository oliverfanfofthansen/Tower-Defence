
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
    // Dei tre fiendetypane våre
    public GameObject normalEnemy;
    public GameObject mediumEnemy;
    public GameObject tankEnemy;

    // Punkta fiendane følgjer
    public Transform[] waypoints;

    // Tid mellom kvar fiende
    public float spawnDelay = 1.5f;

    // Alle waves i spelet
    public WaveData[] waves;

    // Nummeret på siste wave som blei starta
    public int currentWave = 0;

    // Hindrar fleire waves i å starte samtidig
    private bool waveRunning = false;

    // Belønning for første wave
    public int startingReward = 50;

    // Kor mykje belønninga aukar per wave
    public int rewardIncrease = 10;

    // Blir kalla frå Start Wave-knappen
    public void StartNextWave()
    {
        // Sjekkar at wave-lista finst
        if (waves == null)
            return;

        // Ikkje start dersom ei wave allereie køyrer
        // eller alle waves har blitt starta
        if (waveRunning || currentWave >= waves.Length)
            return;

        // Hindrar nye waves dersom spelet er ferdig
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameFinished)
            return;

        // Sjekkar at waypoint-lista er kopla til
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

        // Opprettar normale fiendar
        yield return StartCoroutine(
            SpawnEnemies(normalEnemy, wave.normalCount));

        // Opprettar medium-fiendar
        yield return StartCoroutine(
            SpawnEnemies(mediumEnemy, wave.mediumCount));

        // Opprettar tank-fiendar
        yield return StartCoroutine(
            SpawnEnemies(tankEnemy, wave.tankCount));

        // Ventar til alle fiendane er drepne
        // eller har nådd slutten av vegen
        while (FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None).Length > 0)
        {
            yield return null;
        }

        // Marker at waven er ferdig
        waveRunning = false;

        Debug.Log("Wave " + currentWave + " completed!");

        // Dersom spelaren har tapt, skal han ikkje få
        // belønning eller vinne spelet
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameFinished)
        {
            yield break;
        }

        // Reknar ut vanleg wave-belønning
        // Wave 1 = 50$, wave 2 = 60$ osv.
        int waveReward =
            startingReward + (currentWave - 1) * rewardIncrease;

        // Finn alle Farms som står på kartet
        FarmIncome[] farms = FindObjectsByType<FarmIncome>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        // Reknar ut samla Farm-inntekt
        int farmReward = 0;

        foreach (FarmIncome farm in farms)
        {
            // Kvar Farm bidreg med si eiga inntekt
            farmReward += farm.coinsPerWave;
        }

        // Legg saman alle belønningane
        int totalReward = waveReward + farmReward;

        // Gir spelaren pengane
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddCoins(totalReward);

            Debug.Log(
                "Wave " + currentWave + " completed!" +
                " | Wave reward: " + waveReward + "$" +
                " | Farm income: " + farmReward + "$" +
                " | Total: " + totalReward + "$"
            );
        }
        else
        {
            Debug.LogWarning("MoneyManager manglar!");
        }

        // NYTT: Sjekkar om den siste waven er fullført
        if (currentWave >= waves.Length)
        {
            Debug.Log("Alle waves er fullførte!");

            // Gir spelaren siger dersom GameManager finst
            // og spelaren ikkje allereie har tapt
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameFinished)
            {
                GameManager.Instance.WinGame();
            }
        }
    }

    IEnumerator SpawnEnemies(GameObject prefab, int count)
    {
        // Ingen fiendar av denne typen
        if (count <= 0)
            yield break;

        // Sjekkar at fiende-prefaben finst
        if (prefab == null)
        {
            Debug.LogError("Enemy prefab missing!");
            yield break;
        }

        // Opprettar riktig antal fiendar
        for (int i = 0; i < count; i++)
        {
            // Stoppar spawning dersom spelaren har tapt
            if (GameManager.Instance != null &&
                GameManager.Instance.IsGameFinished)
                yield break;

            // Lagar ein fiende ved første waypoint
            GameObject enemy = Instantiate(
                prefab,
                waypoints[0].position,
                Quaternion.identity
            );

            // Finn EnemyMovement-komponenten
            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            // Gir fienden waypoint-lista
            if (movement != null)
            {
                movement.waypoints = waypoints;
            }

            // Ventar før neste fiende blir oppretta
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}

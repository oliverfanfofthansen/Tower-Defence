
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    // Kor lenge spelaren må vente frå starten av éi wave til den neste.
    [Min(1f)]
    public float nextWaveDelay = 10f;

    // Kor lenge eit tomt brett ventar før neste wave startar sjølv.
    private const float AutoStartDelay = 10f;
    private float nextWaveUnlockTime;
    private float nextWaveAutoStartTime;
    private int activeWaves;

    // Knappen les dei same reglane som StartNextWave brukar.
    public bool HasNextWave => waves != null && currentWave < waves.Length;
    public bool CanStartNextWave => HasNextWave && !IsGameFinished &&
        Time.time >= nextWaveUnlockTime;
    public int NextWaveSeconds
    {
        get
        {
            // Når brettet er tomt, viser vi tida fram til automatisk start.
            float deadline = nextWaveAutoStartTime > 0f ? nextWaveAutoStartTime : nextWaveUnlockTime;
            return Mathf.Max(0, Mathf.CeilToInt(deadline - Time.time));
        }
    }

    private bool IsGameFinished => GameManager.Instance != null &&
        GameManager.Instance.IsGameFinished;

    // Belønning for første wave
    public int startingReward = 50;

    // Kor mykje belønninga aukar per wave
    public int rewardIncrease = 10;

    void Update()
    {
        // Start berre automatisk dersom ei wave står klar og spelet framleis går.
        if (nextWaveAutoStartTime > 0f && Time.time >= nextWaveAutoStartTime && CanStartNextWave)
        {
            nextWaveAutoStartTime = 0f;
            StartNextWave();
        }
    }

    // Den eksisterande knappen kallar denne metoden.
    public void StartNextWave()
    {
        // Stopp òg raske dobbeltklikk og direkte kall medan knappen er låst.
        if (!CanStartNextWave)
            return;

        // Sjekkar at waypoint-lista er kopla til
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Missing waypoints!");
            return;
        }

        // Lås knappen før spawning. Første wave kan startast med ein gong.
        nextWaveAutoStartTime = 0f;
        WaveData wave = waves[currentWave];
        currentWave++;
        activeWaves++;
        if (HasNextWave)
            nextWaveUnlockTime = Time.time + Mathf.Max(1f, nextWaveDelay);

        StartCoroutine(SpawnWave(wave, currentWave));
    }

    IEnumerator SpawnWave(WaveData wave, int waveNumber)
    {
        // Kvar wave hugsar sine eigne fiendar og sitt eige nummer.
        List<GameObject> enemies = new List<GameObject>();
        Debug.Log("Starting Wave " + waveNumber);

        // Opprettar normale fiendar
        yield return StartCoroutine(
            SpawnEnemies(normalEnemy, wave.normalCount, enemies));

        // Opprettar medium-fiendar
        yield return StartCoroutine(
            SpawnEnemies(mediumEnemy, wave.mediumCount, enemies));

        // Opprettar tank-fiendar
        yield return StartCoroutine(
            SpawnEnemies(tankEnemy, wave.tankCount, enemies));

        // For belønninga ventar vi berre på fiendane frå denne waven.
        while (!IsGameFinished && enemies.Exists(enemy => enemy != null && enemy.activeInHierarchy))
            yield return null;

        // Denne waven er ferdig. Andre waves kan framleis køyre.
        activeWaves--;

        // Ved tap skal vi ikkje gi belønning eller utløysa siger.
        if (IsGameFinished)
            yield break;

        // Reknar ut vanleg wave-belønning
        // Wave 1 = 50$, wave 2 = 60$ osv.
        int waveReward =
            startingReward + (waveNumber - 1) * rewardIncrease;

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
                "Wave " + waveNumber + " completed!" +
                " | Wave reward: " + waveReward + "$" +
                " | Farm income: " + farmReward + "$" +
                " | Total: " + totalReward + "$"
            );
        }
        else
        {
            Debug.LogWarning("MoneyManager manglar!");
        }

        // Ved overlapp ventar vi til alle aktive waves er ferdige.
        if (activeWaves != 0)
            yield break;

        if (HasNextWave)
        {
            // Opne knappen med ein gong og gi spelaren ti sekund før automatisk start.
            nextWaveUnlockTime = 0f;
            nextWaveAutoStartTime = Time.time + AutoStartDelay;
        }
        else
        {
            Debug.Log("Alle waves er fullførte!");
            if (GameManager.Instance != null)
                GameManager.Instance.WinGame();
        }
    }

    IEnumerator SpawnEnemies(GameObject prefab, int count, List<GameObject> enemies)
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
            // Vent mellom fiendane, også ved byte av type. Etter den siste kan waven bli ferdig med ein gong.
            if (enemies.Count > 0)
                yield return new WaitForSeconds(spawnDelay);

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

            // Registrerer fienden slik at rett wave får belønninga når han er borte.
            enemies.Add(enemy);

            // Finn EnemyMovement-komponenten
            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            // Gir fienden waypoint-lista
            if (movement != null)
            {
                movement.waypoints = waypoints;
            }
        }
    }
}

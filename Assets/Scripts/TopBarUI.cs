using TMPro;
using UnityEngine;

// Viser speldata utan å endre HP, waves eller pengar.
[DisallowMultipleComponent]
public class TopBarUI : MonoBehaviour
{
    [Header("Eksisterande spelkomponentar")]
    public GameManager gameManager;
    public EnemySpawner enemySpawner;
    public MoneyManager moneyManager;

    [Header("Tekstfelt i toppbaren")]
    public TMP_Text healthText;
    public TMP_Text waveText;
    public TMP_Text coinsText;

    void Start()
    {
        // Varslar om manglande koplingar utan å stoppe spelet.
        if (gameManager == null || enemySpawner == null || moneyManager == null ||
            healthText == null || waveText == null || coinsText == null)
        {
            Debug.LogWarning("TopBarUI manglar referansar i Inspector.", this);
        }

        RefreshDisplay();
    }

    void LateUpdate()
    {
        // Les verdiane etter vanleg Update, også når Time.timeScale er null.
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        // Same data blir brukte av spelmekanikkane og den nye visinga.
        if (healthText != null)
        {
            string value = gameManager != null
                ? $"HP: {gameManager.currentHealth} / {gameManager.maxHealth}"
                : "HP: -- / --";
            SetTextIfChanged(healthText, value);
        }

        if (waveText != null)
        {
            string value = enemySpawner != null && enemySpawner.waves != null
                ? $"WAVE {enemySpawner.currentWave} / {enemySpawner.waves.Length}"
                : "WAVE -- / --";
            SetTextIfChanged(waveText, value);
        }

        if (coinsText != null)
        {
            string value = moneyManager != null
                ? $"Coins: {moneyManager.coins}$"
                : "Coins: --";
            SetTextIfChanged(coinsText, value);
        }
    }

    void SetTextIfChanged(TMP_Text textField, string value)
    {
        // Unngår å setje TMP-teksten på nytt når verdien er uendra.
        if (textField.text != value)
            textField.text = value;
    }
}

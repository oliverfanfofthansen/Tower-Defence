using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // Desse blir funne éin gong frå den eksisterande Canvas-knappen.
    private Button waveButton;
    private TMP_Text waveButtonText;

    void Start()
    {
        // Varslar om manglande koplingar utan å stoppe spelet.
        if (gameManager == null || enemySpawner == null || moneyManager == null ||
            healthText == null || waveText == null || coinsText == null)
        {
            Debug.LogWarning("TopBarUI manglar referansar i Inspector.", this);
        }

        FindWaveButton();
        RefreshDisplay();
        RefreshWaveButton();
    }

    void LateUpdate()
    {
        // Les verdiane etter vanleg Update, også når Time.timeScale er null.
        RefreshDisplay();
        RefreshWaveButton();
    }

    void FindWaveButton()
    {
        // Finn knappen via den lagra OnClick-koplinga, sjølv om han blir flytta i Canvas.
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || enemySpawner == null)
            return;

        foreach (Button button in canvas.rootCanvas.GetComponentsInChildren<Button>(true))
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentTarget(i) == enemySpawner &&
                    button.onClick.GetPersistentMethodName(i) == nameof(EnemySpawner.StartNextWave))
                {
                    waveButton = button;
                    waveButtonText = button.GetComponentInChildren<TMP_Text>(true);
                    return;
                }
            }
        }
    }

    void RefreshWaveButton()
    {
        if (waveButton == null || enemySpawner == null)
            return;

        // Knappen brukar same sperre som spawneren, også på den siste waven.
        string label;
        if (gameManager != null && gameManager.IsGameFinished)
            label = "GAME FINISHED";
        else if (!enemySpawner.HasNextWave)
            label = enemySpawner.currentWave > 0 ? "FINAL WAVE" : "NO WAVES";
        else if (enemySpawner.currentWave == 0)
            label = "START WAVE 1";
        else
        {
            // Knappen er klikkbar under nedteljinga til automatisk start.
            int seconds = enemySpawner.NextWaveSeconds;
            label = seconds > 0 ? $"NEXT WAVE - {seconds}s" : "NEXT WAVE";
        }

        waveButton.interactable = enemySpawner.CanStartNextWave;
        if (waveButtonText != null)
            SetTextIfChanged(waveButtonText, label);
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

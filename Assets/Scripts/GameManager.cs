
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Gjer GameManager tilgjengeleg for andre script
    public static GameManager Instance;

    // Kor mykje HP spelaren har
    public int maxHealth = 100;
    public int currentHealth;

    // Held styr på om spelaren har vunne eller tapt
    private bool gameOver = false;
    private bool victory = false;

    // Andre script kan sjekke om spelet er ferdig
    public bool IsGameFinished => gameOver || victory;

    void Awake()
    {
        // Lagrar referansen til GameManager
        Instance = this;

        // Startar med full HP
        currentHealth = maxHealth;

        // Sørgjer for normal spelefart
        Time.timeScale = 1f;
    }

    // Blir kalla når ein fiende når slutten
    public void TakeDamage(int damage)
    {
        // Ikkje ta skade etter at spelet er ferdig
        if (IsGameFinished)
            return;

        // Trekkjer HP utan å gå under null
        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log("Spelaren mista " + damage +
                  " HP! HP igjen: " + currentHealth);

        // Sjekkar om spelaren har tapt
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    // Blir kalla når spelaren mistar all HP
    void GameOver()
    {
        gameOver = true;

        Debug.Log("GAME OVER!");

        // Stoppar spelverda
        Time.timeScale = 0f;
    }

    // NYTT: Blir kalla etter den siste waven
    public void WinGame()
    {
        // Hindrar siger dersom spelet allereie er ferdig
        if (IsGameFinished)
            return;

        victory = true;

        Debug.Log("YOU WIN!");

        // Stoppar spelverda
        Time.timeScale = 0f;
    }

    // NYTT: Startar spelet heilt på nytt
    public void RestartGame()
    {
        // Normal spelefart før scena blir lasta inn igjen
        Time.timeScale = 1f;

        // Lastar inn den aktive scena på nytt
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    void OnGUI()
    {
        // Viser ikkje sluttmenyen før spelet er ferdig
        if (!IsGameFinished)
            return;

        // Stil for Game Over og You Win
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 48;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        // Grøn tekst ved siger, raud ved tap
        titleStyle.normal.textColor =
            victory ? Color.green : Color.red;

        string message = victory ? "YOU WIN!" : "GAME OVER";

        // Viser resultatet midt på skjermen
        GUI.Label(
            new Rect(
                0,
                Screen.height / 2 - 90,
                Screen.width,
                80
            ),
            message,
            titleStyle
        );

        // Restart-knapp som fungerer sjølv om tida er stoppa
        if (GUI.Button(
            new Rect(
                Screen.width / 2 - 90,
                Screen.height / 2,
                180,
                50
            ),
            "Restart"
        ))
        {
            RestartGame();
        }
    }
}

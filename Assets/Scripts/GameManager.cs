
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Gjer GameManager tilgjengeleg for andre script
    public static GameManager Instance;

    // Kor mykje HP spelaren startar med
    public int maxHealth = 100;

    // Kor mykje HP spelaren har akkurat no
    public int currentHealth;

    // Er spelet over?
    private bool gameOver = false;

    void Awake()
    {
        // Lagrar referansen til GameManager
        Instance = this;

        // Startar med full HP
        currentHealth = maxHealth;

        // Sørgjer for at spelet ikkje er pausa ved start
        Time.timeScale = 1f;
    }

    // Blir kalla når ein fiende når slutten av vegen
    public void TakeDamage(int damage)
    {
        // Ikkje ta meir skade etter Game Over
        if (gameOver)
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

    void GameOver()
    {
        // Marker at spelet er ferdig
        gameOver = true;

        Debug.Log("GAME OVER!");

        // Stoppar spelet
        Time.timeScale = 0f;
    }

    void OnGUI()
    {
        // Lager tekststil for HP-teljaren
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.black;

        // Viser HP øvst til venstre
        GUI.Label(
            new Rect(10, 10, 250, 40),
            "HP: " + currentHealth + "/" + maxHealth,
            style
        );

        // Viser Game Over midt på skjermen
        if (gameOver)
        {
            GUIStyle gameOverStyle = new GUIStyle(GUI.skin.label);
            gameOverStyle.fontSize = 48;
            gameOverStyle.fontStyle = FontStyle.Bold;
            gameOverStyle.normal.textColor = Color.red;
            gameOverStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(
                new Rect(0, Screen.height / 2 - 50,
                         Screen.width, 100),
                "GAME OVER",
                gameOverStyle
            );
        }
    }
}

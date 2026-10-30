using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int maxHealth = 100;
    public int currentHealth;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
        Time.timeScale = 1f;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("GAME OVER!");
            Time.timeScale = 0f;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 200, 30),
            "HP: " + currentHealth + "/" + maxHealth);
    }
}

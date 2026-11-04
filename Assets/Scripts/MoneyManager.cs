
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    // Gjer at andre script kan finne MoneyManager
    public static MoneyManager Instance;

    // Pengane spelaren startar med
    public int coins = 150;

    void Awake()
    {
        // Lagrar ein referanse til denne MoneyManager
        Instance = this;
    }

    // Prøver å betale for noko
    public bool TrySpend(int amount)
    {
        // Returnerer false dersom spelaren ikkje har råd
        if (coins < amount)
        {
            Debug.Log("Du har ikkje nok pengar!");
            return false;
        }

        // Trekkjer pengar frå spelaren
        coins -= amount;
        return true;
    }

    // Denne skal me bruke når fiendar gir belønning
    public void AddCoins(int amount)
    {
        coins += amount;
    }

}

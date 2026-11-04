
using UnityEngine;

public class WaveCounter : MonoBehaviour
{
    // Referanse til EnemySpawner
    private EnemySpawner spawner;

    void Awake()
    {
        // Finn EnemySpawner på same GameObject
        spawner = GetComponent<EnemySpawner>();
    }

    void OnGUI()
    {
        // Gjer ingenting dersom EnemySpawner manglar
        if (spawner == null || spawner.waves == null)
            return;

        // Hentar wave-nummeret og totalt antal waves
        int current = spawner.currentWave;
        int total = spawner.waves.Length;

        // Bestemmer korleis teksten skal sjå ut
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.black;
        style.alignment = TextAnchor.MiddleRight;

        // Viser wave-teljaren øvst til høgre
        GUI.Label(
            new Rect(Screen.width - 220, 10, 200, 40),
            "Wave: " + current + "/" + total,
            style
        );
    }
}

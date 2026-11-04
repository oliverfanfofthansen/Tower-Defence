using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Verktøyet køyrer berre når brukaren vel menykommandoen i Unity Editor.
public static class TopBarSetup
{
    private const string MenuPath = "Tools/Tower Defence/Opprett toppbar";
    private const float BarHeight = 72f;

    [MenuItem(MenuPath)]
    public static void CreateTopBar()
    {
        // Scena skal endrast i redigeringsmodus, aldri under Play eller i ein prefab.
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            Debug.LogWarning("Opne spel-scena utanfor Play Mode før du opprettar toppbaren.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        // Brukar berre det eksisterande Canvas-objektet i den aktive scena.
        Canvas canvas = FindSingleInScene<Canvas>(scene);
        if (canvas == null)
            return;

        // Beheld ein eksisterande toppbar, inkludert eventuelle manuelle justeringar.
        Transform existing = canvas.transform.Find("TopBar");
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("TopBar finst allereie. Ingen objekt eller koplingar er endra.", existing);
            return;
        }

        GameManager gameManager = FindSingleInScene<GameManager>(scene);
        EnemySpawner spawner = FindSingleInScene<EnemySpawner>(scene);
        MoneyManager moneyManager = FindSingleInScene<MoneyManager>(scene);
        if (gameManager == null || spawner == null || moneyManager == null)
            return;

        // Gjenbruker ein TMP-font frå Canvas utan å endre dei eksisterande tekstfelta.
        TMP_FontAsset font = FindFont(canvas);
        if (font == null)
        {
            Debug.LogError("Toppbaren treng ein TMP-font. Kontroller Text (TMP) på ein eksisterande knapp.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Opprett toppbar");

        RectTransform bar = CreateRect("TopBar", canvas.transform);
        bar.anchorMin = new Vector2(0f, 1f);
        bar.anchorMax = new Vector2(1f, 1f);
        bar.pivot = new Vector2(0.5f, 1f);
        bar.anchoredPosition = Vector2.zero;
        bar.sizeDelta = new Vector2(0f, BarHeight);

        Image background = bar.gameObject.AddComponent<Image>();
        background.color = new Color(0.12f, 0.09f, 0.07f, 0.97f);
        // Hindrar at eit klikk på den synlege toppbaren byggjer eit tårn bak han.
        background.raycastTarget = true;

        RectTransform healthPanel = CreatePanel("HealthPanel", bar, 0f, 0.3f,
            new Color(0.25f, 0.10f, 0.09f, 1f));
        RectTransform wavePanel = CreatePanel("WavePanel", bar, 0.3f, 0.7f,
            new Color(0.20f, 0.15f, 0.10f, 1f));
        RectTransform coinsPanel = CreatePanel("CoinsPanel", bar, 0.7f, 1f,
            new Color(0.26f, 0.21f, 0.08f, 1f));

        TMP_Text healthText = CreateText("HealthText", healthPanel, font,
            TextAlignmentOptions.MidlineLeft, new Color(1f, 0.88f, 0.85f));
        TMP_Text waveText = CreateText("WaveText", wavePanel, font,
            TextAlignmentOptions.Midline, new Color(1f, 0.95f, 0.83f));
        TMP_Text coinsText = CreateText("CoinsText", coinsPanel, font,
            TextAlignmentOptions.MidlineRight, new Color(1f, 0.85f, 0.35f));

        // Forhandsvising før Awake fyller currentHealth ved spelstart.
        healthText.text = $"HP: {gameManager.maxHealth} / {gameManager.maxHealth}";
        waveText.text = $"WAVE {spawner.currentWave} / {(spawner.waves != null ? spawner.waves.Length : 0)}";
        coinsText.text = $"Coins: {moneyManager.coins}$";

        RectTransform border = CreateRect("BottomBorder", bar);
        border.anchorMin = Vector2.zero;
        border.anchorMax = new Vector2(1f, 0f);
        border.pivot = new Vector2(0.5f, 0f);
        border.sizeDelta = new Vector2(0f, 3f);
        Image borderImage = border.gameObject.AddComponent<Image>();
        borderImage.color = new Color(0.60f, 0.42f, 0.20f, 1f);
        borderImage.raycastTarget = false;

        // Koplar dei nye felta direkte til komponentane som allereie finst i scena.
        TopBarUI controller = bar.gameObject.AddComponent<TopBarUI>();
        controller.gameManager = gameManager;
        controller.enemySpawner = spawner;
        controller.moneyManager = moneyManager;
        controller.healthText = healthText;
        controller.waveText = waveText;
        controller.coinsText = coinsText;

        // Heile det nye hierarkiet kan fjernast med éin Undo. Scena blir ikkje lagra automatisk.
        Undo.RegisterCreatedObjectUndo(bar.gameObject, "Opprett toppbar");
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = bar.gameObject;
        Debug.Log("TopBar er oppretta. Kontroller Game-visinga og lagre scena før du testar Restart.", bar);
    }

    private static T FindSingleInScene<T>(Scene scene) where T : Component
    {
        // Søker òg i barn, slik at GameManager (1) blir funnen utan å endre hierarkiet.
        List<T> components = new List<T>();
        foreach (GameObject root in scene.GetRootGameObjects())
            components.AddRange(root.GetComponentsInChildren<T>(true));

        if (components.Count == 1)
            return components[0];

        Debug.LogError($"Venta éin {typeof(T).Name} i aktiv scene, fann {components.Count}. Ingen toppbar er oppretta.");
        return null;
    }

    private static TMP_FontAsset FindFont(Canvas canvas)
    {
        foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.font != null)
                return text.font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject created = new GameObject(name, typeof(RectTransform));
        created.layer = parent.gameObject.layer;
        RectTransform rect = created.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static RectTransform CreatePanel(string name, RectTransform parent,
        float leftAnchor, float rightAnchor, Color color)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = new Vector2(leftAnchor, 0f);
        rect.anchorMax = new Vector2(rightAnchor, 1f);
        rect.offsetMin = new Vector2(8f, 8f);
        rect.offsetMax = new Vector2(-8f, -8f);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static TMP_Text CreateText(string name, RectTransform parent,
        TMP_FontAsset font, TextAlignmentOptions alignment, Color color)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(16f, 0f);
        rect.offsetMax = new Vector2(-16f, 0f);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.color = color;
        text.fontStyle = FontStyles.Bold;
        text.fontSize = 30f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = 30f;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }
}

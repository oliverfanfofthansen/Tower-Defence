using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Viser den valde bygningen. TowerUpgrade står framleis for sjølve kjøpet.
public class TowerUpgradeUI : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform hotbar;
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text buildingName;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text stat1Label;
    [SerializeField] private TMP_Text stat1Value;
    [SerializeField] private RectTransform secondRow;
    [SerializeField] private TMP_Text stat2Label;
    [SerializeField] private TMP_Text stat2Value;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeText;

    private TowerUpgrade shownTower;
    private int shownLevel;
    private float shownRange;
    private LineRenderer rangeLine;
    private Material rangeMaterial;

    internal bool ContainsScreenPoint(Vector2 screenPoint)
    {
        // Canvas er Screen Space - Overlay, så funksjonen treng ikkje eit kamera.
        return panel.gameObject.activeInHierarchy &&
            RectTransformUtility.RectangleContainsScreenPoint(panel, screenPoint);
    }

    void LateUpdate()
    {
        TowerUpgrade tower = TowerUpgrade.SelectedTower;
        bool hasSelection = tower != null;
        if (panel.gameObject.activeSelf != hasSelection)
            panel.gameObject.SetActive(hasSelection);

        if (!hasSelection)
        {
            shownTower = null;
            if (rangeLine != null)
                rangeLine.enabled = false;
            return;
        }

        // Bygg opp teksten på nytt når spelaren byter tårn eller nivå.
        if (shownTower != tower || shownLevel != tower.Level)
        {
            RefreshPanel(tower);
            shownTower = tower;
            shownLevel = tower.Level;
        }

        // Spelaren kan få nye pengar medan panelet er ope.
        upgradeButton.interactable = tower.Level < tower.maxLevel &&
            MoneyManager.Instance != null && MoneyManager.Instance.coins >= tower.GetUpgradeCost();

        PositionPanel();
        ShowRange(tower);
    }

    public void UpgradeSelectedTower()
    {
        // Same kjøpsfunksjon og pengesjekk som U-tasten.
        TowerUpgrade tower = TowerUpgrade.SelectedTower;
        if (tower != null)
            tower.UpgradeBuilding();
    }

    void RefreshPanel(TowerUpgrade tower)
    {
        TowerAttack ranged = tower.RangedAttack;
        MeleeAttack melee = tower.MeleeAttack;
        FarmIncome farm = tower.Farm;
        bool maxLevel = tower.Level >= tower.maxLevel;

        // Bruk den vanlege bygning-spriten, også medan tårnet spelar ein animasjon.
        SpriteRenderer renderer = tower.GetComponentInChildren<SpriteRenderer>();
        Sprite sprite = renderer != null ? renderer.sprite : null;
        if (ranged != null && ranged.idleSprite != null)
            sprite = ranged.idleSprite;
        else if (melee != null && melee.idleSprite != null)
            sprite = melee.idleSprite;

        portrait.sprite = sprite;
        portrait.enabled = sprite != null;
        portrait.color = renderer != null ? renderer.color : Color.white;
        buildingName.text = ranged != null ? "CANNON" : tower.GetBuildingName().ToUpperInvariant();
        levelText.text = $"Level {tower.Level} / {tower.maxLevel}";
        upgradeText.text = maxLevel ? "MAX LEVEL" : $"UPGRADE – {tower.GetUpgradeCost()}$";

        // Farm har berre éi rad. Tårna brukar begge radene.
        secondRow.gameObject.SetActive(ranged != null || melee != null);
        if (ranged != null)
        {
            stat1Label.text = "Range";
            stat1Value.text = Stat(ranged.range, TowerUpgrade.RangedRangeIncrease, maxLevel, "0.0#");
            stat2Label.text = "Fire rate";
            stat2Value.text = Stat(ranged.fireRate, TowerUpgrade.FireRateIncrease, maxLevel, "0.0#");
        }
        else if (melee != null)
        {
            stat1Label.text = "Damage";
            stat1Value.text = Stat(melee.damage, TowerUpgrade.MeleeDamageIncrease, maxLevel, "0");
            stat2Label.text = "Range";
            stat2Value.text = Stat(melee.range, TowerUpgrade.MeleeRangeIncrease, maxLevel, "0.0#");
        }
        else if (farm != null)
        {
            stat1Label.text = "Coins per wave";
            stat1Value.text = Stat(farm.coinsPerWave, TowerUpgrade.FarmIncomeIncrease, maxLevel, "0", "$");
        }
    }

    static string Stat(float current, float increase, bool maxLevel, string format, string unit = "")
    {
        // På maksnivå viser vi berre verdien bygningen faktisk har.
        string value = current.ToString(format) + unit;
        return maxLevel ? value : $"{value} → {(current + increase).ToString(format)}{unit}";
    }

    void PositionPanel()
    {
        // Dersom det er for trongt ved sida av hotbaren, legg vi panelet over han og pila.
        float freeWidth = (((RectTransform)transform).rect.width - hotbar.rect.width) * 0.5f;
        float bottom = freeWidth >= panel.rect.width + 36f ? 20f : hotbar.rect.height + 52f;
        panel.anchoredPosition = new Vector2(-20f, bottom);
    }

    void ShowRange(TowerUpgrade tower)
    {
        // Ein farm har inga angrepsrekkevidd. Tårn brukar verdien frå angrepsscriptet.
        float radius = tower.RangedAttack != null ? tower.RangedAttack.range :
            tower.MeleeAttack != null ? tower.MeleeAttack.range : 0f;
        if (radius <= 0f)
        {
            if (rangeLine != null)
                rangeLine.enabled = false;
            return;
        }

        if (rangeLine == null)
            CreateRangeLine();
        if (rangeLine == null)
            return;

        rangeLine.enabled = true;
        rangeLine.transform.position = tower.transform.position;

        // Rekn ut sirkelen på nytt berre når den faktiske radiusen endrar seg.
        if (Mathf.Approximately(shownRange, radius))
            return;

        shownRange = radius;
        for (int i = 0; i < rangeLine.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / rangeLine.positionCount;
            rangeLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    void CreateRangeLine()
    {
        // Bruk same shader som rekkeviddesirkelen ved tårnplassering.
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            return;

        rangeLine = new GameObject("SelectedTowerRange").AddComponent<LineRenderer>();
        rangeMaterial = new Material(shader);
        rangeLine.sharedMaterial = rangeMaterial;
        rangeLine.useWorldSpace = false;
        rangeLine.loop = true;
        rangeLine.positionCount = 64;
        rangeLine.startWidth = rangeLine.endWidth = 0.04f;
        rangeLine.startColor = rangeLine.endColor = new Color(0.71f, 1f, 0.49f, 0.86f);
        rangeLine.sortingOrder = 9;
    }

    void OnDisable()
    {
        // Skjul sirkelen dersom Canvas blir skrudd av.
        if (rangeLine != null)
            rangeLine.enabled = false;
    }

    void OnDestroy()
    {
        // Rydd opp når scena blir lasta på nytt.
        if (rangeLine != null)
            Destroy(rangeLine.gameObject);
        if (rangeMaterial != null)
            Destroy(rangeMaterial);
    }
}

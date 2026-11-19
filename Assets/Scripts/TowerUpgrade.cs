
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// Alle bygningar med dette scriptet må ha ein BoxCollider2D
[RequireComponent(typeof(BoxCollider2D))]
public class TowerUpgrade : MonoBehaviour
{
    // Hugsar kva bygning spelaren har valt
    private static TowerUpgrade selectedTower;
    internal static TowerUpgrade SelectedTower => selectedTower;

    // Førehandsvisinga og kjøpet brukar dei same oppgraderingstala.
    internal const float RangedRangeIncrease = 0.5f;
    internal const float FireRateIncrease = 0.5f;
    internal const int MeleeDamageIncrease = 2;
    internal const float MeleeRangeIncrease = 0.3f;
    internal const int FarmIncomeIncrease = 25;

    // Nivået til denne bygningen
    private int level = 1;
    public int maxLevel = 3;

    // Referansar til dei ulike bygningstypane
    private TowerAttack rangedAttack;
    private MeleeAttack meleeAttack;
    private FarmIncome farmIncome;

    // Menyen les nivået og dei faktiske komponentane på den valde bygningen.
    internal int Level => level;
    internal TowerAttack RangedAttack => rangedAttack;
    internal MeleeAttack MeleeAttack => meleeAttack;
    internal FarmIncome Farm => farmIncome;
    private TowerUpgradeUI upgradeMenu;

    // Brukast til å registrere klikk på bygningen
    private BoxCollider2D clickCollider;

    void Awake()
    {
        // Finn komponentane på bygningen
        rangedAttack = GetComponent<TowerAttack>();
        meleeAttack = GetComponent<MeleeAttack>();
        farmIncome = GetComponent<FarmIncome>();

        clickCollider = GetComponent<BoxCollider2D>();

        // NYTT: Brukar den oppdaterte Unity-funksjonen
        // til å finne oppgraderingsmenyen.
        upgradeMenu = FindAnyObjectByType<TowerUpgradeUI>();
    }

    void Update()
    {
        // Sjekkar om spelaren venstreklikkar
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();

            // Behald valet ved klikk inne i oppgraderingspanelet
            if (upgradeMenu != null &&
                upgradeMenu.ContainsScreenPoint(mouse))
                return;

            // Sjekkar om spelaren klikkar på UI
            bool clickedUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();

            if (clickedUI)
            {
                // Lukkar menyen ved andre UI-klikk
                if (selectedTower == this)
                    selectedTower = null;
            }
            else
            {
                Camera cam = Camera.main;

                if (cam != null)
                {
                    // Konverterer museposisjonen til spelverda
                    Vector3 world = cam.ScreenToWorldPoint(
                        new Vector3(
                            mouse.x,
                            mouse.y,
                            -cam.transform.position.z
                        )
                    );

                    // Sjekkar om spelaren klikka på bygningen
                    if (clickCollider.OverlapPoint(world))
                    {
                        selectedTower = this;

                        Debug.Log(
                            GetBuildingName() +
                            " valt! Level " + level
                        );
                    }
                    else if (selectedTower == this)
                    {
                        // Klikk utanfor lukkar menyen
                        selectedTower = null;
                    }
                }
            }
        }

        // U-tasten oppgraderer den valde bygningen
        if (selectedTower == this &&
            Keyboard.current != null &&
            Keyboard.current.uKey.wasPressedThisFrame)
        {
            UpgradeBuilding();
        }

        // Escape lukkar oppgraderingsmenyen
        if (selectedTower == this &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            selectedTower = null;
        }
    }

    // Finn namnet på bygningen
    internal string GetBuildingName()
    {
        if (rangedAttack != null)
            return "Ranged Tower";

        if (meleeAttack != null)
            return "Melee Tower";

        if (farmIncome != null)
            return "Farm";

        return "Unknown Building";
    }

    // Reknar ut prisen på neste oppgradering
    internal int GetUpgradeCost()
    {
        // Ranged: 60$ og 100$
        if (rangedAttack != null)
            return level == 1 ? 60 : 100;

        // Melee: 80$ og 120$
        if (meleeAttack != null)
            return level == 1 ? 80 : 120;

        // Farm: 100$ og 150$
        if (farmIncome != null)
            return level == 1 ? 100 : 150;

        return 0;
    }

    // Oppgraderer bygningen
    internal void UpgradeBuilding()
    {
        // Hindrar oppgradering over maks level
        if (level >= maxLevel)
        {
            Debug.Log("Bygningen er allereie max level!");
            return;
        }

        // Sjekkar at bygningen har ein funksjon
        if (rangedAttack == null &&
            meleeAttack == null &&
            farmIncome == null)
        {
            Debug.LogWarning(
                "Bygningen manglar angrep eller inntekt!"
            );
            return;
        }

        // Sjekkar at MoneyManager finst
        if (MoneyManager.Instance == null)
        {
            Debug.LogWarning("MoneyManager manglar!");
            return;
        }

        int cost = GetUpgradeCost();

        // Trekkjer pengar dersom spelaren har råd
        if (!MoneyManager.Instance.TrySpend(cost))
            return;

        // Ranged får betre rekkevidde og skotfart
        if (rangedAttack != null)
        {
            rangedAttack.range += RangedRangeIncrease;
            rangedAttack.fireRate += FireRateIncrease;
        }

        // Melee får meir skade og betre rekkevidde
        else if (meleeAttack != null)
        {
            meleeAttack.damage += MeleeDamageIncrease;
            meleeAttack.range += MeleeRangeIncrease;
        }

        // Farm får 25$ meir per wave
        else if (farmIncome != null)
        {
            farmIncome.coinsPerWave += FarmIncomeIncrease;
        }

        // Aukar nivået
        level++;

        Debug.Log(
            GetBuildingName() +
            " oppgradert til level " + level +
            " for " + cost + "$!"
        );
    }

    void OnDisable()
    {
        // Fjern valet dersom bygningen blir deaktivert
        if (selectedTower == this)
            selectedTower = null;
    }
}

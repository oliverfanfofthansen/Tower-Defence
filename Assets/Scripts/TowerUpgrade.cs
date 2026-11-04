
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// Alle bygningar med dette scriptet må ha ein BoxCollider2D
[RequireComponent(typeof(BoxCollider2D))]
public class TowerUpgrade : MonoBehaviour
{
    // Hugsar kva bygning spelaren har valt
    private static TowerUpgrade selectedTower;

    // Nivået til denne bygningen
    private int level = 1;
    public int maxLevel = 3;

    // Referansar til dei ulike bygningstypane
    private TowerAttack rangedAttack;
    private MeleeAttack meleeAttack;
    private FarmIncome farmIncome;

    // Brukast til å registrere klikk på bygningen
    private BoxCollider2D clickCollider;

    void Awake()
    {
        // Finn komponentane på bygningen
        rangedAttack = GetComponent<TowerAttack>();
        meleeAttack = GetComponent<MeleeAttack>();
        farmIncome = GetComponent<FarmIncome>();

        clickCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Sjekkar om spelaren venstreklikkar
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Sjekkar om spelaren klikkar på ein UI-knapp
            bool clickedUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();

            if (clickedUI)
            {
                // Lukkar oppgraderingsmenyen ved UI-klikk
                if (selectedTower == this)
                    selectedTower = null;
            }
            else
            {
                Camera cam = Camera.main;

                if (cam != null)
                {
                    // Hentar museposisjonen
                    Vector2 mouse =
                        Mouse.current.position.ReadValue();

                    // Konverterer museposisjonen til spelverda
                    Vector3 world = cam.ScreenToWorldPoint(
                        new Vector3(
                            mouse.x,
                            mouse.y,
                            -cam.transform.position.z
                        )
                    );

                    // Sjekkar om spelaren klikka på denne bygningen
                    if (clickCollider.OverlapPoint(world))
                    {
                        // Vel denne bygningen
                        selectedTower = this;

                        Debug.Log(
                            GetBuildingName() +
                            " valt! Level " + level
                        );
                    }
                    else if (selectedTower == this)
                    {
                        // NYTT: Klikk utanfor bygningen lukkar menyen
                        selectedTower = null;
                    }
                }
            }
        }

        // Trykk U for å oppgradere den valde bygningen
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
    string GetBuildingName()
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
    int GetUpgradeCost()
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
    void UpgradeBuilding()
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
            Debug.LogWarning("Bygningen manglar angrep eller inntekt!");
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
            rangedAttack.range += 0.5f;
            rangedAttack.fireRate += 0.5f;
        }

        // Melee får meir skade og betre rekkevidde
        else if (meleeAttack != null)
        {
            meleeAttack.damage += 2;
            meleeAttack.range += 0.3f;
        }

        // Farm får 25$ meir per wave
        else if (farmIncome != null)
        {
            farmIncome.coinsPerWave += 25;
        }

        // Aukar nivået
        level++;

        Debug.Log(
            GetBuildingName() +
            " oppgradert til level " + level +
            " for " + cost + "$!"
        );
    }

    void OnGUI()
    {
        // Viser berre menyen for den valde bygningen
        if (selectedTower != this)
            return;

        string message =
            GetBuildingName() + " - LEVEL " + level;

        if (level < maxLevel)
        {
            message += "\nTrykk U for oppgradering: " +
                       GetUpgradeCost() + "$";
        }
        else
        {
            message += "\nMAX LEVEL!";
        }

        // Mellombels oppgraderingsmeny
        GUI.Box(
            new Rect(Screen.width / 2 - 150, 10, 300, 70),
            message
        );
    }

    void OnDisable()
    {
        // Fjern valet dersom bygningen blir deaktivert
        if (selectedTower == this)
            selectedTower = null;
    }
}

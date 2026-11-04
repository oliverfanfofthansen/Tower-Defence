
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// Sørgjer for at tårnet har ein collider som kan klikkast på
[RequireComponent(typeof(BoxCollider2D))]
public class RangedTowerUpgrade : MonoBehaviour
{
    // Innstillingar for oppgraderingar
    public int maxLevel = 3;
    public int firstUpgradeCost = 60;
    public int costIncrease = 40;

    // Kor mykje betre tårnet blir per oppgradering
    public float rangeIncrease = 0.5f;
    public float fireRateIncrease = 0.5f;

    // Alle nye tårn startar på level 1
    private int level = 1;

    // Referansar til angrepsscriptet og collidaren
    private TowerAttack towerAttack;
    private BoxCollider2D clickCollider;

    // Hugsar kva tårn spelaren har valt
    private static RangedTowerUpgrade selectedTower;

    void Awake()
    {
        // Hentar komponentane på dette tårnet
        towerAttack = GetComponent<TowerAttack>();
        clickCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // Sjekkar om spelaren venstreklikkar
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Ignorer klikk på UI-knappar
            if (EventSystem.current == null ||
                !EventSystem.current.IsPointerOverGameObject())
            {
                Camera cam = Camera.main;

                if (cam != null)
                {
                    // Finn museposisjonen i spelverda
                    Vector2 mouse = Mouse.current.position.ReadValue();

                    Vector3 world = cam.ScreenToWorldPoint(
                        new Vector3(
                            mouse.x,
                            mouse.y,
                            -cam.transform.position.z
                        )
                    );

                    // Sjekkar om spelaren klikka på dette tårnet
                    if (clickCollider.OverlapPoint((Vector2)world))
                    {
                        selectedTower = this;

                        Debug.Log("Ranged Tower valt! Level " + level);
                    }
                }
            }
        }

        // Oppgrader når det valde tårnet får U-kommandoen
        if (selectedTower == this &&
            Keyboard.current != null &&
            Keyboard.current.uKey.wasPressedThisFrame)
        {
            UpgradeTower();
        }
    }

    // Reknar ut prisen for neste oppgradering
    int GetUpgradeCost()
    {
        return firstUpgradeCost + (level - 1) * costIncrease;
    }

    void UpgradeTower()
    {
        // Stoppar dersom tårnet er på maks level
        if (level >= maxLevel)
        {
            Debug.Log("Tårnet er allereie max level!");
            return;
        }

        // Sjekkar at alle nødvendige system finst
        if (towerAttack == null || MoneyManager.Instance == null)
        {
            Debug.LogWarning("TowerAttack eller MoneyManager manglar!");
            return;
        }

        int cost = GetUpgradeCost();

        // Trekkjer pengar berre dersom spelaren har råd
        if (!MoneyManager.Instance.TrySpend(cost))
            return;

        // Gjer berre dette tårnet sterkare
        towerAttack.range += rangeIncrease;
        towerAttack.fireRate += fireRateIncrease;

        // Aukar level
        level++;

        Debug.Log("Tårn oppgradert til level " + level + "!");
    }

    void OnGUI()
    {
        // Viser berre informasjon om det valde tårnet
        if (selectedTower != this)
            return;

        string message = "RANGED TOWER - LEVEL " + level;

        if (level < maxLevel)
        {
            message += "\nTrykk U for upgrade: " +
                       GetUpgradeCost() + "$";
        }
        else
        {
            message += "\nMAX LEVEL!";
        }

        // Mellombels meny øvst i midten
        GUI.Box(
            new Rect(Screen.width / 2 - 140, 10, 280, 65),
            message
        );
    }

    void OnDisable()
    {
        // Fjern valet dersom tårnet blir deaktivert
        if (selectedTower == this)
            selectedTower = null;
    }
}

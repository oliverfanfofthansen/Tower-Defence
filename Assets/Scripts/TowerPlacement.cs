
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    // Kameraet og kartet vårt
    public Camera gameCamera;
    public Tilemap groundTilemap;
    public TileBase grassTile;

    // Dei tre bygningane spelaren kan kjøpe
    public GameObject rangedTowerPrefab;
    public GameObject meleeTowerPrefab;
    public GameObject farmTowerPrefab;

    // Prisen på kvar bygning
    public int rangedTowerCost = 50;
    public int meleeTowerCost = 75;
    public int farmTowerCost = 100;

    // Lagrar kva bygning spelaren har valt
    private GameObject selectedTowerPrefab;
    private int selectedTowerCost;

    // Fortel om spelaren er i byggjemodus
    private bool isPlacing = false;

    // Lagrar ruter som allereie er opptatte
    private HashSet<Vector3Int> occupiedCells =
        new HashSet<Vector3Int>();

    // Vel Ranged Tower frå hotbaren
    public void SelectRangedTower()
    {
        SelectBuilding(rangedTowerPrefab, rangedTowerCost);
    }

    // Vel Melee Tower frå hotbaren
    public void SelectMeleeTower()
    {
        SelectBuilding(meleeTowerPrefab, meleeTowerCost);
    }

    // Vel Farm frå hotbaren
    public void SelectFarmTower()
    {
        SelectBuilding(farmTowerPrefab, farmTowerCost);
    }

    // Felles funksjon for å velje ein bygning
    void SelectBuilding(GameObject prefab, int cost)
    {
        // Sjekkar at prefab-en finst
        if (prefab == null)
        {
            Debug.LogWarning("Building prefab manglar!");
            return;
        }

        // Hugsar kva me vil plassere
        selectedTowerPrefab = prefab;
        selectedTowerCost = cost;
        isPlacing = true;

        Debug.Log("Selected building: " + prefab.name);
    }

    void Update()
    {
        // Stoppar dersom me ikkje byggjer
        if (!isPlacing || Mouse.current == null)
            return;

        // Høgreklikk avbryt plasseringa
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isPlacing = false;
            return;
        }

        // Ventar på venstre museklikk
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Hindrar bygging når spelaren klikkar på UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        // Kontrollerer referansane frå Inspector
        if (gameCamera == null ||
            groundTilemap == null ||
            grassTile == null ||
            selectedTowerPrefab == null)
        {
            Debug.LogWarning("TowerPlacement manglar referansar!");
            return;
        }

        // Finn museposisjonen på skjermen
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // Gjer skjermposisjonen om til verdsposisjon
        Vector3 worldPosition = gameCamera.ScreenToWorldPoint(
            new Vector3(
                mousePosition.x,
                mousePosition.y,
                -gameCamera.transform.position.z
            )
        );

        // Finn ruta me har klikka på
        Vector3Int cell =
            groundTilemap.WorldToCell(worldPosition);

        // Det er berre lov å byggje på gras
        if (groundTilemap.GetTile(cell) != grassTile)
        {
            Debug.Log("Du kan berre byggje på gras!");
            return;
        }

        // Hindrar fleire bygningar på same rute
        if (occupiedCells.Contains(cell))
        {
            Debug.Log("Ruta er opptatt!");
            return;
        }

        // Sjekkar at pengesystemet finst
        if (MoneyManager.Instance == null)
        {
            Debug.LogWarning("MoneyManager manglar!");
            return;
        }

        // Prøver å betale for bygningen
        // Ugyldige plasseringar kostar ingenting
        if (!MoneyManager.Instance.TrySpend(selectedTowerCost))
            return;

        // Finn midten av ruta
        Vector3 position =
            groundTilemap.GetCellCenterWorld(cell);

        position.z = 0f;

        // Plasserer den valde bygningen
        Instantiate(
            selectedTowerPrefab,
            position,
            Quaternion.identity
        );

        // Marker ruta som opptatt
        occupiedCells.Add(cell);

        Debug.Log(
            selectedTowerPrefab.name +
            " kjøpt for " + selectedTowerCost + "$!"
        );

        // Avsluttar byggjemodus
        isPlacing = false;
    }
}


using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    // Kameraet som viser spelet
    public Camera gameCamera;

    // Tilemap-et som inneheld gras og veg
    public Tilemap groundTilemap;

    // Tile-typen det er lov å byggje på
    public TileBase grassTile;

    // Tårnet me skal plassere
    public GameObject rangedTowerPrefab;

    // Er spelaren i byggjemodus?
    private bool isPlacing = false;

    // Hugsar rutene som allereie har eit tårn
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    // Blir kalla når spelaren trykkjer Ranged i hotbaren
    public void SelectRangedTower()
    {
        isPlacing = true;
        Debug.Log("Ranged Tower selected!");
    }

    void Update()
    {
        // Gjer ingenting dersom me ikkje byggjer
        if (!isPlacing || Mouse.current == null)
            return;

        // Høgreklikk avbryt bygginga
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isPlacing = false;
            return;
        }

        // Vent på venstre museklikk
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Ikkje plasser tårn når spelaren klikkar på UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        // Sjekk at alt er kopla riktig i Inspector
        if (gameCamera == null || groundTilemap == null ||
            grassTile == null || rangedTowerPrefab == null)
        {
            Debug.LogWarning("TowerPlacement manglar referansar!");
            return;
        }

        // Finn museposisjonen på skjermen
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Konverterer museposisjonen til posisjon i spelverda
        Vector3 worldPosition = gameCamera.ScreenToWorldPoint(
            new Vector3(
                mousePosition.x,
                mousePosition.y,
                -gameCamera.transform.position.z
            )
        );

        // Finn kva rute spelaren klikka på
        Vector3Int cell = groundTilemap.WorldToCell(worldPosition);

        // Sjekk om ruta er gras
        if (groundTilemap.GetTile(cell) != grassTile)
        {
            Debug.Log("Du kan berre byggje på gras!");
            return;
        }

        // Sjekk om det allereie står eit tårn her
        if (occupiedCells.Contains(cell))
        {
            Debug.Log("Ruta er opptatt!");
            return;
        }

        // Finn midten av ruta
        Vector3 position = groundTilemap.GetCellCenterWorld(cell);
        position.z = 0f;

        // Opprett tårnet på ruta
        Instantiate(rangedTowerPrefab, position, Quaternion.identity);

        // Registrer ruta som opptatt
        occupiedCells.Add(cell);

        // Avslutt byggjemodus etter plassering
        isPlacing = false;
    }
}

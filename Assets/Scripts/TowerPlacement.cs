
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    // Kameraet og kartet
    public Camera gameCamera;
    public Tilemap groundTilemap;
    public TileBase grassTile;

    // Tårnet spelaren kan kjøpe
    public GameObject rangedTowerPrefab;

    // Prisen på eitt Ranged Tower
    public int rangedTowerCost = 50;

    // Om spelaren er i byggjemodus
    private bool isPlacing = false;

    // Hugsar rutene som har tårn
    private HashSet<Vector3Int> occupiedCells =
        new HashSet<Vector3Int>();

    // Blir kalla frå Ranged-knappen i hotbaren
    public void SelectRangedTower()
    {
        isPlacing = true;
        Debug.Log("Ranged Tower selected!");
    }

    void Update()
    {
        // Stoppar dersom spelaren ikkje byggjer
        if (!isPlacing || Mouse.current == null)
            return;

        // Høgreklikk avbryt bygginga
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isPlacing = false;
            return;
        }

        // Ventar på venstreklikk
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Hindrar plassering når me klikkar på UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        // Kontrollerer at referansane er kopla til
        if (gameCamera == null ||
            groundTilemap == null ||
            grassTile == null ||
            rangedTowerPrefab == null)
        {
            Debug.LogWarning("TowerPlacement manglar referansar!");
            return;
        }

        // Hentar museposisjonen
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // Gjer skjermposisjonen om til verdsposisjon
        Vector3 worldPosition =
            gameCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    -gameCamera.transform.position.z
                )
            );

        // Finn ruta spelaren klikka på
        Vector3Int cell =
            groundTilemap.WorldToCell(worldPosition);

        // Berre grastilen er lov å byggje på
        if (groundTilemap.GetTile(cell) != grassTile)
        {
            Debug.Log("Du kan berre byggje på gras!");
            return;
        }

        // Hindrar bygging på opptatte ruter
        if (occupiedCells.Contains(cell))
        {
            Debug.Log("Ruta er opptatt!");
            return;
        }

        // Sjekkar om MoneyManager finst
        if (MoneyManager.Instance == null)
        {
            Debug.LogWarning("MoneyManager manglar!");
            return;
        }

        // Prøver å betale 50$ for tårnet
        // Dersom spelaren ikkje har råd, blir det ikkje plassert
        if (!MoneyManager.Instance.TrySpend(rangedTowerCost))
        {
            return;
        }

        // Finn midten av ruta
        Vector3 position =
            groundTilemap.GetCellCenterWorld(cell);

        position.z = 0f;

        // Opprettar tårnet
        Instantiate(
            rangedTowerPrefab,
            position,
            Quaternion.identity
        );

        // Marker ruta som opptatt
        occupiedCells.Add(cell);

        // Avsluttar byggjemodus
        isPlacing = false;

        Debug.Log("Tårn kjøpt for " + rangedTowerCost + "$!");
    }
}

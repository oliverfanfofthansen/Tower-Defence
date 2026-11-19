
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    [Header("Kart og kamera")]
    public Camera gameCamera;
    public Tilemap groundTilemap;
    public TileBase grassTile;

    [Header("Bygningar")]
    public GameObject rangedTowerPrefab;
    public GameObject meleeTowerPrefab;
    public GameObject farmTowerPrefab;

    [Header("Prisar")]
    public int rangedTowerCost = 50;
    public int meleeTowerCost = 75;
    public int farmTowerCost = 100;

    [Header("Preview")]
    // Kor gjennomsiktig bygningen skal vere før plassering
    [Range(0.1f, 1f)]
    public float previewOpacity = 0.6f;

    // Breidda på rekkeviddesirkelen
    public float circleWidth = 0.035f;

    // Hugsar kva bygning spelaren vil plassere
    private GameObject selectedTowerPrefab;
    private int selectedTowerCost;
    private bool isPlacing = false;

    // Ruter som allereie har ein bygning
    private HashSet<Vector3Int> occupiedCells =
        new HashSet<Vector3Int>();

    // Preview-objekt som berre er visuelle
    private GameObject previewObject;
    private SpriteRenderer previewRenderer;

    // Objektet som viser rekkevidda
    private GameObject rangeObject;
    private LineRenderer rangeLine;
    private Material rangeMaterial;

    // Vel Ranged frå hotbaren
    public void SelectRangedTower()
    {
        SelectBuilding(rangedTowerPrefab, rangedTowerCost);
    }

    // Vel Melee frå hotbaren
    public void SelectMeleeTower()
    {
        SelectBuilding(meleeTowerPrefab, meleeTowerCost);
    }

    // Vel Farm frå hotbaren
    public void SelectFarmTower()
    {
        SelectBuilding(farmTowerPrefab, farmTowerCost);
    }

    // Felles funksjon for val av bygning
    void SelectBuilding(GameObject prefab, int cost)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Building prefab manglar!");
            return;
        }

        selectedTowerPrefab = prefab;
        selectedTowerCost = cost;
        isPlacing = true;

        // Opprettar ein visuell kopi og rekkeviddesirkel
        CreatePreview(prefab);

        Debug.Log("Vald bygning: " + prefab.name);
    }

    void CreatePreview(GameObject prefab)
    {
        // Fjern eventuelle gamle preview-objekt
        ClearPreview();

        // Finn sprite på den valde bygningen
        SpriteRenderer original =
            prefab.GetComponentInChildren<SpriteRenderer>();

        if (original != null)
        {
            // Lagar eit objekt utan angrepsscript eller collider
            previewObject = new GameObject("TowerPreview");

            previewRenderer =
                previewObject.AddComponent<SpriteRenderer>();

            // Kopierer utsjånaden til bygningen
            previewRenderer.sprite = original.sprite;
            previewRenderer.flipX = original.flipX;
            previewRenderer.sortingLayerID =
                original.sortingLayerID;
            previewRenderer.sortingOrder =
                original.sortingOrder + 20;

            // Brukar same skala som bygningen
            previewObject.transform.localScale =
                prefab.transform.localScale;
        }

        // Finn rekkevidda til bygningen
        float range = 0f;

        TowerAttack ranged =
            prefab.GetComponent<TowerAttack>();

        MeleeAttack melee =
            prefab.GetComponent<MeleeAttack>();

        if (ranged != null)
            range = ranged.range;
        else if (melee != null)
            range = melee.range;

        // Farm har inga angrepsrekkevidde
        if (range <= 0f)
            return;

        // Opprettar rekkeviddesirkelen
        rangeObject = new GameObject("RangePreview");

        rangeLine = rangeObject.AddComponent<LineRenderer>();

        // Brukar ein shader som kan vise ein farga strek
        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogWarning("Fann ikkje Sprites/Default!");
            Destroy(rangeObject);
            rangeObject = null;
            rangeLine = null;
            return;
        }

        rangeMaterial = new Material(shader);
        rangeLine.sharedMaterial = rangeMaterial;

        // Sirkelen ligg i spelverda
        rangeLine.useWorldSpace = false;

        // 64 punkt gir ein jamn sirkel
        int points = 64;

        rangeLine.positionCount = points;
        rangeLine.loop = true;

        // Tydeleg, men tynn strek
        rangeLine.startWidth = circleWidth;
        rangeLine.endWidth = circleWidth;

        rangeLine.sortingOrder = 100;

        // Reknar ut punkta rundt sirkelen
        for (int i = 0; i < points; i++)
        {
            float angle =
                i * Mathf.PI * 2f / points;

            float x = Mathf.Cos(angle) * range;
            float y = Mathf.Sin(angle) * range;

            rangeLine.SetPosition(
                i,
                new Vector3(x, y, 0f)
            );
        }
    }

    void Update()
    {
        // Ingen byggjemodus
        if (!isPlacing || Mouse.current == null)
            return;

        // Høgreklikk avbryt
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            StopPlacing();
            return;
        }

        if (gameCamera == null ||
            groundTilemap == null ||
            grassTile == null ||
            selectedTowerPrefab == null)
        {
            return;
        }

        // Sjekkar om musa er over UI
        bool overUI =
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject();

        if (overUI)
        {
            // Skjul preview når musa er over UI
            SetPreviewVisible(false);
            return;
        }

        // Finn museposisjonen
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // Gjer skjermposisjon om til verdsposisjon
        Vector3 worldPosition =
            gameCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    -gameCamera.transform.position.z
                )
            );

        // Finn ruta under musa
        Vector3Int cell =
            groundTilemap.WorldToCell(worldPosition);

        // Dersom ruta er heilt utanfor kartet
        if (!groundTilemap.HasTile(cell))
        {
            SetPreviewVisible(false);
            return;
        }

        // Finn midten av ruta
        Vector3 position =
            groundTilemap.GetCellCenterWorld(cell);

        position.z = 0f;

        // Sjekkar om ruta er gyldig
        bool validPlacement =
            groundTilemap.GetTile(cell) == grassTile &&
            !occupiedCells.Contains(cell);

        // Flytt preview til ruta
        UpdatePreview(position, validPlacement);

        // Vent på venstreklikk
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Hindrar plassering på ugyldig rute
        if (!validPlacement)
        {
            Debug.Log("Du kan ikkje byggje her!");
            return;
        }

        // Sjekkar at pengesystemet finst
        if (MoneyManager.Instance == null)
        {
            Debug.LogWarning("MoneyManager manglar!");
            return;
        }

        // Trekkjer pengar dersom spelaren har råd
        if (!MoneyManager.Instance.TrySpend(selectedTowerCost))
            return;

        // Plasserer den ekte bygningen
        Instantiate(
            selectedTowerPrefab,
            position,
            Quaternion.identity
        );

        // Registrerer ruta som opptatt
        occupiedCells.Add(cell);

        Debug.Log(
            selectedTowerPrefab.name +
            " kjøpt for " + selectedTowerCost + "$!"
        );

        // Fjern preview etter plassering
        StopPlacing();
    }

    void UpdatePreview(Vector3 position, bool valid)
    {
        SetPreviewVisible(true);

        // Grøn dersom ruta er gyldig, elles raud
        Color previewColor = valid
            ? new Color(0.6f, 1f, 0.6f, previewOpacity)
            : new Color(1f, 0.35f, 0.35f, previewOpacity);

        if (previewObject != null)
        {
            // Flyttar den gjennomsiktige bygningen
            previewObject.transform.position = position;

            if (previewRenderer != null)
                previewRenderer.color = previewColor;
        }

        if (rangeObject != null)
        {
            // Flyttar rekkeviddesirkelen saman med bygningen
            rangeObject.transform.position = position;

            if (rangeLine != null)
            {
                // Grøn sirkel ved gyldig plassering
                Color circleColor = valid
                    ? new Color(0.3f, 1f, 0.4f, 0.8f)
                    : new Color(1f, 0.2f, 0.2f, 0.8f);

                rangeLine.startColor = circleColor;
                rangeLine.endColor = circleColor;
            }
        }
    }

    void SetPreviewVisible(bool visible)
    {
        if (previewObject != null)
            previewObject.SetActive(visible);

        if (rangeObject != null)
            rangeObject.SetActive(visible);
    }

    void StopPlacing()
    {
        // Avsluttar byggjemodus
        isPlacing = false;
        selectedTowerPrefab = null;

        // Fjernar den visuelle markeringa
        ClearPreview();
    }

    void ClearPreview()
    {
        // Fjernar den gjennomsiktige bygningen
        if (previewObject != null)
            Destroy(previewObject);

        // Fjernar rekkeviddesirkelen
        if (rangeObject != null)
            Destroy(rangeObject);

        // Ryddar opp i materialet
        if (rangeMaterial != null)
            Destroy(rangeMaterial);

        previewObject = null;
        previewRenderer = null;
        rangeObject = null;
        rangeLine = null;
        rangeMaterial = null;
    }

    void OnDisable()
    {
        // Ryddar opp dersom objektet blir deaktivert
        ClearPreview();
    }
}

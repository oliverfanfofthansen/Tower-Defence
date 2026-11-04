using UnityEngine;
using UnityEngine.Tilemaps;

// Held toppbaren over kamerabiletet og tilpassar utsnittet utan å flytte kartet.
[ExecuteAlways]
[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class BoardViewportLayout : MonoBehaviour
{
    [Header("Eksisterande sceneobjekt")]
    public Tilemap groundTilemap;
    public RectTransform topBar;

    [Header("Plass til kartet")]
    [Range(0.5f, 0.99f)]
    public float viewportHeight = 0.93f;
    [Min(0f)]
    public float padding = 0.25f;

    private Camera boardCamera;
    private Bounds boardBounds;
    private bool boundsNeedRefresh = true;
    private bool hasBoardBounds;

    void OnEnable()
    {
        boardCamera = GetComponent<Camera>();
        boundsNeedRefresh = true;
        ApplyLayout();
    }

    void OnValidate()
    {
        // Reknar grensene på nytt i neste Update når Inspector-verdiar blir endra.
        boundsNeedRefresh = true;
    }

    void Update()
    {
        // Køyrer før tårnplasseringa, slik at kamera og museklikk brukar same utsnitt.
        // Dette fangar òg opp endra vindaugsstorleik og fungerer medan spelet er pausa.
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (boardCamera == null)
            boardCamera = GetComponent<Camera>();

        if (boardCamera == null || !boardCamera.orthographic)
            return;

        float height = Mathf.Clamp(viewportHeight, 0.5f, 0.99f);
        Rect viewport = new Rect(0f, 0f, 1f, height);
        if (boardCamera.rect != viewport)
            boardCamera.rect = viewport;

        // Toppbaren fyller nettopp skjermområdet over kameraet, uavhengig av Canvas-skalering.
        // Ingen barn, TMP-felt eller andre menyar blir endra.
        if (topBar != null)
        {
            Vector2 lowerAnchor = new Vector2(0f, height);
            if (topBar.anchorMin != lowerAnchor || topBar.anchorMax != Vector2.one ||
                topBar.offsetMin != Vector2.zero || topBar.offsetMax != Vector2.zero)
            {
                topBar.anchorMin = lowerAnchor;
                topBar.anchorMax = Vector2.one;
                topBar.offsetMin = Vector2.zero;
                topBar.offsetMax = Vector2.zero;
            }
        }

        if (boundsNeedRefresh)
            CacheBoardBounds();

        if (!hasBoardBounds)
            return;

        // Orthographic Size er halve den synlege høgda i spelverda.
        // Begge utrekningane må få plass: kartet si høgd og breidda delt på kameraformatet.
        float margin = Mathf.Max(0f, padding);
        float aspect = Mathf.Max(0.01f, boardCamera.aspect);
        float sizeForHeight = boardBounds.extents.y + margin;
        float sizeForWidth = (boardBounds.extents.x + margin) / aspect;
        float size = Mathf.Max(sizeForHeight, sizeForWidth);
        if (!Mathf.Approximately(boardCamera.orthographicSize, size))
            boardCamera.orthographicSize = size;

        // Sentrerer berre kameraet i XY. Avstanden til kartet i Z blir behalden.
        Vector3 position = boardCamera.transform.position;
        Vector3 target = new Vector3(boardBounds.center.x, boardBounds.center.y, position.z);
        if (position != target)
            boardCamera.transform.position = target;
    }

    private void CacheBoardBounds()
    {
        boundsNeedRefresh = false;
        hasBoardBounds = false;
        if (groundTilemap == null)
            return;

        Vector3Int minimum = Vector3Int.zero;
        Vector3Int maximum = Vector3Int.zero;

        // Finn berre ruter med tiles. Tomme randruter skal ikkje gjere kameraet skeivt.
        // Vi les kartet utan å kalle CompressBounds eller endre Tilemap-data.
        foreach (Vector3Int cell in groundTilemap.cellBounds.allPositionsWithin)
        {
            if (!groundTilemap.HasTile(cell))
                continue;

            if (!hasBoardBounds)
            {
                minimum = maximum = cell;
                hasBoardBounds = true;
            }
            else
            {
                minimum = Vector3Int.Min(minimum, cell);
                maximum = Vector3Int.Max(maximum, cell);
            }
        }

        if (!hasBoardBounds)
            return;

        // Øvre cellegrense ligg etter siste tile. Ta med hjørna i verdskoordinatar.
        maximum += Vector3Int.one;
        boardBounds = new Bounds(groundTilemap.CellToWorld(minimum), Vector3.zero);
        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                Vector3Int corner = new Vector3Int(
                    x == 0 ? minimum.x : maximum.x,
                    y == 0 ? minimum.y : maximum.y,
                    minimum.z);
                boardBounds.Encapsulate(groundTilemap.CellToWorld(corner));
            }
        }
    }
}

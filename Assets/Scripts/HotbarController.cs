
using UnityEngine;
using TMPro;
using System.Collections;

public class HotbarController : MonoBehaviour
{
    // Tekstfeltet på knappen med pila
    public TMP_Text arrowText;

    // Kor raskt menyen skal gli opp og ned
    public float animationTime = 0.3f;

    private RectTransform panel;
    private bool isOpen = false;
    private Coroutine animation;

    void Awake()
    {
        // Hentar Rect Transform frå HotbarPanel
        panel = GetComponent<RectTransform>();

        // Startar med menyen skjult under skjermen
        panel.anchoredPosition =
            new Vector2(0, -panel.rect.height);

        UpdateArrow();
    }

    public void ToggleHotbar()
    {
        // Byter mellom open og lukka meny
        isOpen = !isOpen;

        // Stoppar eventuell tidlegare animasjon
        if (animation != null)
            StopCoroutine(animation);

        // Startar animasjonen
        animation = StartCoroutine(SlideHotbar());

        UpdateArrow();
    }

    IEnumerator SlideHotbar()
    {
        // Hugsar kor panelet står no
        Vector2 start = panel.anchoredPosition;

        // Open = 0, lukka = minus panelhøgda
        float targetY = isOpen ? 0 : -panel.rect.height;
        Vector2 target = new Vector2(0, targetY);

        float elapsed = 0f;

        while (elapsed < animationTime)
        {
            // Brukar faktisk tid, også om spelet er pausa
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / animationTime);

            // Gjer rørsla mjukare
            t = t * t * (3f - 2f * t);

            // Flyttar panelet gradvis
            panel.anchoredPosition = Vector2.Lerp(start, target, t);

            yield return null;
        }

        // Sørgjer for at panelet endar heilt riktig
        panel.anchoredPosition = target;
        animation = null;
    }

    void UpdateArrow()
    {
        // Pila viser kva retning menyen kan flyttast
        if (arrowText != null)
            arrowText.text = isOpen ? "▼" : "▲";
    }
}

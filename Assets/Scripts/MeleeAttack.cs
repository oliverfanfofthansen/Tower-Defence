
using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    [Header("Angrep")]

    // Kor nær fienden må vere for å bli angripen
    public float range = 1.5f;

    // Kor mykje skade kvart slag gjer
    public int damage = 3;

    // Sekund mellom kvart angrep
    public float attackCooldown = 0.8f;

    [Header("Melee-animasjon")]

    // Sprite Renderer på muskelmannen
    public SpriteRenderer meleeRenderer;

    // Vanleg stilling
    public Sprite idleSprite;

    // Han hevar armen før slaget
    public Sprite windupSprite;

    // Han slår fienden
    public Sprite punchSprite;

    // Han går tilbake etter slaget
    public Sprite recoverySprite;

    [Header("Animasjonstid")]

    // Tid før slaget
    public float windupTime = 0.12f;

    // Kor lenge slaget blir vist
    public float punchTime = 0.12f;

    // Kor lenge han brukar på å komme tilbake
    public float recoveryTime = 0.12f;

    // Når neste angrep kan starte
    private float nextAttackTime = 0f;

    // Hindrar fleire angrep samtidig
    private bool isAttacking = false;

    void Awake()
    {
        // Finn Sprite Renderer automatisk
        if (meleeRenderer == null)
        {
            meleeRenderer = GetComponent<SpriteRenderer>();
        }

        // Brukar eksisterande sprite dersom Idle ikkje er valt
        if (idleSprite == null && meleeRenderer != null)
        {
            idleSprite = meleeRenderer.sprite;
        }

        // Startar med normal stilling
        ShowSprite(idleSprite);
    }

    void Update()
    {
        // Finn næraste fiende innanfor rekkevidda
        EnemyHealth target = FindTarget();

        // Snur seg mot fienden når han ikkje angrip
        if (target != null && !isAttacking)
        {
            FaceTarget(target);
        }

        // Vent dersom han angrip eller har cooldown
        if (isAttacking || Time.time < nextAttackTime)
            return;

        // Ikkje angrip dersom ingen fiende er nær
        if (target == null)
            return;

        // Bestemmer når neste angrep kan starte
        nextAttackTime =
            Time.time + Mathf.Max(0.01f, attackCooldown);

        // Startar slag-animasjonen
        StartCoroutine(AttackAnimation(target));
    }

    // Finn næraste levande fiende
    EnemyHealth FindTarget()
    {
        EnemyHealth closestEnemy = null;
        float closestDistance = range;

        // Finn alle aktive fiendar
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            // Ignorer fiendar som ikkje lever
            if (enemy.currentHealth <= 0)
                continue;

            // Reknar ut avstanden
            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            // Finn den næraste fienden innanfor rekkevidda
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    // Snur muskelmannen mot fienden
    void FaceTarget(EnemyHealth target)
    {
        if (meleeRenderer == null || target == null)
            return;

        // Finn den horisontale avstanden til fienden
        float differenceX =
            target.transform.position.x - transform.position.x;

        // Unngår at han snur fram og tilbake
        // når fienden står nesten rett over eller under
        if (Mathf.Abs(differenceX) > 0.05f)
        {
            // Spegelvender figuren når fienden er til venstre
            meleeRenderer.flipX = differenceX < 0f;
        }
    }

    // Viser riktig animasjonsbilete
    void ShowSprite(Sprite sprite)
    {
        if (meleeRenderer != null && sprite != null)
        {
            meleeRenderer.sprite = sprite;
        }
    }

    // Spelar av eit heilt slag
    IEnumerator AttackAnimation(EnemyHealth target)
    {
        // Marker at han angrip
        isAttacking = true;

        // STEG 1: Hevar armen
        ShowSprite(windupSprite);

        yield return new WaitForSeconds(windupTime);

        // Sjekkar om den gamle fienden framleis
        // er levande og innanfor rekkevidda
        if (target == null ||
            target.currentHealth <= 0 ||
            Vector2.Distance(
                transform.position,
                target.transform.position
            ) > range)
        {
            // Finn ein ny fiende dersom den gamle er borte
            target = FindTarget();
        }

        // STEG 2: Slår
        if (target != null)
        {
            // Snur seg mot fienden før slaget
            FaceTarget(target);

            // Viser slag-animasjonen
            ShowSprite(punchSprite);

            // Gir fienden skade
            target.TakeDamage(damage);
        }
        else
        {
            // Viser framleis slaget om fienden nettopp forsvann
            ShowSprite(punchSprite);
        }

        yield return new WaitForSeconds(punchTime);

        // STEG 3: Går tilbake etter slaget
        ShowSprite(recoverySprite);

        yield return new WaitForSeconds(recoveryTime);

        // STEG 4: Tilbake til vanleg stilling
        ShowSprite(idleSprite);

        // Klar til neste angrep
        isAttacking = false;
    }
}

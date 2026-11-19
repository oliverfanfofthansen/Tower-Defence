
using UnityEngine;
using System.Collections;

public class TowerAttack : MonoBehaviour
{
    // Kor langt kanonen kan skyte
    public float range = 3f;

    // Kor mange skot kanonen fyrer per sekund
    public float fireRate = 2f;

    // Kanonkula som skal bli skoten
    public GameObject bulletPrefab;

    [Header("Kanonanimasjon")]

    // Sprite Renderer som viser kanonen
    public SpriteRenderer cannonRenderer;

    // Vanleg kanon
    public Sprite idleSprite;

    // Kanonen trekkjer seg saman før skotet
    public Sprite compressedSprite;

    // Kanonen skyt, gjerne med flamme
    public Sprite firingSprite;

    // Kor lenge kanonen er trekt saman
    public float chargeTime = 0.10f;

    // Kor lenge skytebiletet blir vist
    public float firingTime = 0.12f;

    // Held styr på tida mellom skota
    private float cooldown = 0f;

    // Hindrar at fleire animasjonar startar samtidig
    private bool isFiring = false;

    void Awake()
    {
        // Finn Sprite Renderer automatisk
        if (cannonRenderer == null)
        {
            cannonRenderer = GetComponent<SpriteRenderer>();
        }

        // Bruk eksisterande sprite dersom Idle ikkje er valt
        if (idleSprite == null && cannonRenderer != null)
        {
            idleSprite = cannonRenderer.sprite;
        }
    }

    void Update()
    {
        // Tel ned til neste skot
        cooldown -= Time.deltaTime;

        // Finn næraste fiende innanfor rekkevidda
        EnemyHealth target = FindTarget();

        if (target == null)
            return;

        // Får kanonen til å vende seg mot fienden
        AimAtTarget(target);

        // Vent dersom kanonen ladar eller nettopp har skote
        if (cooldown > 0f || isFiring)
            return;

        // Sjekkar at me har ein kanonkule
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet Prefab manglar!");
            return;
        }

        // Set tida før neste skot
        cooldown = 1f / Mathf.Max(0.01f, fireRate);

        // Startar skyteanimasjonen
        StartCoroutine(FireAnimation());
    }

    void AimAtTarget(EnemyHealth target)
    {
        if (cannonRenderer == null || target == null)
            return;

        // Sjekkar om fienden er til venstre for kanonen
        bool enemyOnLeft =
            target.transform.position.x < transform.position.x;

        // Spegelvender kanonen mot venstre
        cannonRenderer.flipX = enemyOnLeft;
    }

    IEnumerator FireAnimation()
    {
        // Marker at kanonen er i gang med eit skot
        isFiring = true;

        // STEG 1: Kanonen trekkjer seg saman
        if (cannonRenderer != null && compressedSprite != null)
        {
            cannonRenderer.sprite = compressedSprite;
        }

        // Ventar litt før kanonen fyrer
        yield return new WaitForSeconds(chargeTime);

        // Finn ein fiende på nytt
        // Den gamle kan ha blitt drepen medan me lada
        EnemyHealth target = FindTarget();

        if (target != null)
        {
            // Oppdaterer retninga før skotet
            AimAtTarget(target);

            // STEG 2: Viser skyteanimasjonen
            if (cannonRenderer != null && firingSprite != null)
            {
                cannonRenderer.sprite = firingSprite;
            }

            // Opprettar kanonkula
            GameObject bullet = Instantiate(
                bulletPrefab,
                transform.position,
                Quaternion.identity
            );

            // Finn Bullet-scriptet på kanonkula
            Bullet bulletScript = bullet.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                // Gir kanonkula fienden ho skal følgje
                bulletScript.SetTarget(target);
            }
            else
            {
                Debug.LogWarning("Kanonkula manglar Bullet-script!");
                Destroy(bullet);
            }

            // Held skytebiletet synleg ei kort stund
            yield return new WaitForSeconds(firingTime);
        }

        // STEG 3: Kanonen går tilbake til normal
        if (cannonRenderer != null && idleSprite != null)
        {
            cannonRenderer.sprite = idleSprite;
        }

        // Kanonen er klar til neste skot
        isFiring = false;
    }

    EnemyHealth FindTarget()
    {
        EnemyHealth closest = null;

        // Startar med maksimal rekkevidde
        float shortestDistance = range;

        // Finn alle aktive fiendar
        EnemyHealth[] enemies =
            FindObjectsByType<EnemyHealth>(
                FindObjectsSortMode.None
            );

        foreach (EnemyHealth enemy in enemies)
        {
            // Ignorer fiendar som er døde
            if (enemy.currentHealth <= 0)
                continue;

            // Reknar ut avstanden til fienden
            float distance = Vector2.Distance(
                transform.position,
                enemy.transform.position
            );

            // Sjekkar om denne fienden er nærare
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }
}

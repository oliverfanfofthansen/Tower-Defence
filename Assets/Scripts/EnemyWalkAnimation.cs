
using UnityEngine;

public class EnemyWalkAnimation : MonoBehaviour
{
    // Sprite Renderer på fienden
    public SpriteRenderer spriteRenderer;

    // To bilete for kvar gangretning
    public Sprite[] walkDown;
    public Sprite[] walkUp;
    public Sprite[] walkSide;

    // Kor lenge kvart bilete blir vist
    public float frameDuration = 0.2f;

    // Hugsar kvar fienden var førre frame
    private Vector3 lastPosition;

    // Held styr på animasjonen
    private float timer = 0f;
    private int frameIndex = 0;

    // 0 = ned, 1 = opp, 2 = side
    private int direction = -1;

    void Awake()
    {
        // Finn Sprite Renderer automatisk
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (spriteRenderer == null)
            return;

        // Finn kor mykje fienden har flytta seg
        Vector3 movement = transform.position - lastPosition;
        lastPosition = transform.position;

        // Ikkje animer dersom fienden står stille
        if (movement.sqrMagnitude < 0.00000001f)
            return;

        int newDirection;

        // Finn ut om fienden går sidevegs
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            newDirection = 2;

            // Spegelvender figuren når han går til venstre
            spriteRenderer.flipX = movement.x < 0;
        }
        else
        {
            // Vel framfrå eller bakfrå
            newDirection = movement.y > 0 ? 1 : 0;
            spriteRenderer.flipX = false;
        }

        // Start animasjonen på nytt når retninga endrar seg
        if (newDirection != direction)
        {
            direction = newDirection;
            frameIndex = 0;
            timer = 0f;
        }

        // Tel tid mellom animasjonsbileta
        timer += Time.deltaTime;

        if (timer >= frameDuration)
        {
            timer = 0f;

            // Vekslar mellom bilete 0 og 1
            frameIndex = 1 - frameIndex;
        }

        // Vel riktig spritesamling
        Sprite[] frames;

        if (direction == 0)
            frames = walkDown;
        else if (direction == 1)
            frames = walkUp;
        else
            frames = walkSide;

        // Oppdaterer biletet dersom det finst
        if (frames != null &&
            frames.Length > frameIndex &&
            frames[frameIndex] != null)
        {
            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}

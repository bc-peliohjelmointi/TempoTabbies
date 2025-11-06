using UnityEngine;

public class JudgmentDisplay : MonoBehaviour
{
    [Header("Judgment Sprites")]
    public Sprite MarvelousSprite;
    public Sprite PerfectSprite;
    public Sprite GreatSprite;
    public Sprite GoodSprite;
    public Sprite BadSprite;
    public Sprite MissSprite;

    [Header("Animation Settings")]
    public float bounceDuration = 0.15f;
    public float bounceScale = 1.3f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private float bounceTimer;
    private bool isBouncing;

    public static JudgmentDisplay Instance { get; private set; }
    void Awake()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        spriteRenderer.enabled = false;
    }

    void Update()
    {
        if (isBouncing)
        {
            bounceTimer += Time.deltaTime;
            float t = bounceTimer / bounceDuration;

            // Smooth scaling curve (ease out)
            float scale = Mathf.Lerp(bounceScale, 1f, Mathf.SmoothStep(0, 1, t));
            transform.localScale = originalScale * scale;

            if (t >= 1f)
            {
                transform.localScale = originalScale;
                isBouncing = false;
            }
        }
    }

    public void Show(string label)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.enabled = true;

        switch (label)
        {
            case "MARVELOUS": spriteRenderer.sprite = MarvelousSprite; break;
            case "PERFECT": spriteRenderer.sprite = PerfectSprite; break;
            case "GREAT": spriteRenderer.sprite = GreatSprite; break;
            case "GOOD": spriteRenderer.sprite = GoodSprite; break;
            case "BAD": spriteRenderer.sprite = BadSprite; break;
            case "MISS": spriteRenderer.sprite = MissSprite; break;
            default: spriteRenderer.enabled = false; return;
        }

        // Restart bounce
        transform.localScale = originalScale * bounceScale;
        bounceTimer = 0f;
        isBouncing = true;
    }
}

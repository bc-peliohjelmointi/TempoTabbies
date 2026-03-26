using UnityEngine;
using UnityEngine.UI;

public class JudgmentDisplay : MonoBehaviour
{
    [Header("Judgment Sprites")]
    public Sprite MarvelousSprite;
    public Sprite PerfectSprite;
    public Sprite GreatSprite;
    public Sprite GoodSprite;
    public Sprite BadSprite;
    public Sprite MissSprite;

    [Header("Early/Late Indicator")]
    public GameObject DirectionIndicator;
    public Sprite EarlySprite;
    public Sprite LateSprite;

    [Header("Animation Settings")]
    public float bounceDuration = 0.15f;
    public float bounceScale = 1.3f;

    public Image judgmentSpriteRenderer;
    private SpriteRenderer directionSpriteRenderer;
    private Vector3 judgmentOriginalScale;
    private Vector3 directionOriginalScale;
    private float bounceTimer;
    private bool isBouncing;

    public static JudgmentDisplay Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        if (DirectionIndicator != null)
        {
            directionSpriteRenderer = DirectionIndicator.GetComponent<SpriteRenderer>();
        }

        judgmentOriginalScale = judgmentSpriteRenderer.transform.localScale;
        if (DirectionIndicator != null)
        {
            directionOriginalScale = DirectionIndicator.transform.localScale;
        }

        judgmentSpriteRenderer.enabled = false;
        if (directionSpriteRenderer != null)
        {
            directionSpriteRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (isBouncing)
        {
            bounceTimer += Time.deltaTime;
            float t = bounceTimer / bounceDuration;

            // Smooth scaling curve (ease out)
            float scale = Mathf.Lerp(bounceScale, 1f, Mathf.SmoothStep(0, 1, t));
            judgmentSpriteRenderer.transform.localScale = judgmentOriginalScale * scale;

            // Scale direction indicator along with judgment
            if (DirectionIndicator != null)
            {
                DirectionIndicator.transform.localScale = directionOriginalScale * scale;
            }

            if (t >= 1f)
            {
                judgmentSpriteRenderer.transform.localScale = judgmentOriginalScale;
                if (DirectionIndicator != null)
                {
                    DirectionIndicator.transform.localScale = directionOriginalScale;
                }
                isBouncing = false;
            }
        }
    }

    public void Show(string label, bool isEarly = false, bool isLate = false)
    {
        if (judgmentSpriteRenderer == null) return;

        // Show judgment
        judgmentSpriteRenderer.enabled = true;

        switch (label)
        {
            case "MARVELOUS": judgmentSpriteRenderer.sprite = MarvelousSprite; break;
            case "PERFECT": judgmentSpriteRenderer.sprite = PerfectSprite; break;
            case "GREAT": judgmentSpriteRenderer.sprite = GreatSprite; break;
            case "GOOD": judgmentSpriteRenderer.sprite = GoodSprite; break;
            case "BAD": judgmentSpriteRenderer.sprite = BadSprite; break;
            case "MISS": judgmentSpriteRenderer.sprite = MissSprite; break;
            default: judgmentSpriteRenderer.enabled = false; break;
        }

        // Handle direction indicator
        if (directionSpriteRenderer != null && DirectionIndicator != null)
        {
            // Only show direction indicator if NOT MARVELOUS
            if (label != "MARVELOUS")
            {
                if (isEarly)
                {
                    directionSpriteRenderer.enabled = true;
                    directionSpriteRenderer.sprite = EarlySprite;
                    DirectionIndicator.SetActive(true);
                }
                else if (isLate)
                {
                    directionSpriteRenderer.enabled = true;
                    directionSpriteRenderer.sprite = LateSprite;
                    DirectionIndicator.SetActive(true);
                }
                else
                {
                    directionSpriteRenderer.enabled = false;
                    DirectionIndicator.SetActive(false);
                }
            }
            else
            {
                // Always hide direction indicator for MARVELOUS
                directionSpriteRenderer.enabled = false;
                DirectionIndicator.SetActive(false);
            }
        }

        // Restart bounce animation
        judgmentSpriteRenderer.transform.localScale = judgmentOriginalScale * bounceScale;
        if (DirectionIndicator != null)
        {
            DirectionIndicator.transform.localScale = directionOriginalScale * bounceScale;
        }
        bounceTimer = 0f;
        isBouncing = true;
    }
}
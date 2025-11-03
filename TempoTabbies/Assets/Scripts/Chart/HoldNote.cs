using UnityEngine;

public class HoldNote : MonoBehaviour
{
    public float StartTime;
    public float EndTime;
    public float ScrollSpeed;
    public AudioSource Music;
    public Transform HitLine;

    [Header("Hold Components")]
    public GameObject Body; // stretchable sprite
    public GameObject End;  // hold end cap

    private float initialX;
    private float initialZ;
    private SpriteRenderer bodyRenderer;
    private float baseBodyHeight;

    [Header("Body Settings")]
    public float BodyWidth = 0.25f; // fixed X scale for body and end

    void Start()
    {
        initialX = transform.position.x;
        initialZ = transform.position.z;

        // Set up body
        if (Body != null)
        {
            bodyRenderer = Body.GetComponent<SpriteRenderer>();
            if (bodyRenderer != null && bodyRenderer.sprite != null)
                baseBodyHeight = bodyRenderer.bounds.size.y;

            Vector3 scale = Body.transform.localScale;
            scale.x = BodyWidth;
            Body.transform.localScale = scale;
        }

        // Set up end cap width
        if (End != null)
        {
            Vector3 scale = End.transform.localScale;
            scale.x = BodyWidth;
            scale.y = BodyWidth;
            End.transform.localScale = scale;
        }
    }

    void Update()
    {
        if (!Music || !HitLine) return;

        float songTime = Music.time;
        float timeUntilStart = StartTime - songTime;
        float timeUntilEnd = EndTime - songTime;

        float startY = HitLine.position.y + timeUntilStart * ScrollSpeed;
        float endY = HitLine.position.y + timeUntilEnd * ScrollSpeed;

        // Move the head
        transform.position = new Vector3(initialX, startY, initialZ);

        // Move the end cap
        if (End != null)
            End.transform.position = new Vector3(initialX, endY, initialZ);

        // Stretch the body
        if (Body != null && bodyRenderer != null)
        {
            float worldDistance = Mathf.Abs(startY - endY);
            float midY = (startY + endY) * 0.5f;
            Body.transform.position = new Vector3(initialX, midY, initialZ);

            float spriteHeight = bodyRenderer.sprite.bounds.size.y;
            float scaleY = worldDistance / spriteHeight;

            Vector3 bodyScale = Body.transform.localScale;
            bodyScale.x = BodyWidth; // fixed width
            bodyScale.y = scaleY;    // stretch vertically
            Body.transform.localScale = bodyScale;
        }

        // Cleanup
        if (songTime > EndTime + 1f)
        {
            if (Body) Destroy(Body);
            if (End) Destroy(End);
            Destroy(gameObject);
        }
    }
}

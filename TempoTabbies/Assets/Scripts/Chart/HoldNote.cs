using UnityEngine;

public class HoldNote : MonoBehaviour
{
    public float StartTime;
    public float EndTime;
    public float ScrollSpeed;
    public int Lane;
    public AudioSource Music;
    public Transform HitLine;

    [Header("Hold Components")]
    public GameObject Head; // assigned in NoteSpawner
    public GameObject Body;
    public GameObject End;

    private float initialX;
    private float initialZ;
    private SpriteRenderer bodyRenderer;
    private float baseBodyHeight;

    [Header("Body Settings")]
    public float BodyWidth = 0.25f;

    void Start()
    {
        initialX = transform.position.x;
        initialZ = transform.position.z;

        if (Body != null)
        {
            bodyRenderer = Body.GetComponent<SpriteRenderer>();
            if (bodyRenderer != null && bodyRenderer.sprite != null)
                baseBodyHeight = bodyRenderer.bounds.size.y;

            Vector3 scale = Body.transform.localScale;
            scale.x = BodyWidth;
            Body.transform.localScale = scale;
        }

        if (End != null)
        {
            Vector3 scale = End.transform.localScale;
            scale.x = BodyWidth;
            scale.y = BodyWidth;
            End.transform.localScale = scale;
        }

        if (Head != null)
        {
            Vector3 scale = Head.transform.localScale;
            scale.x = BodyWidth;
            Head.transform.localScale = scale;
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

        // move the WHOLE hold note root (includes Head/Body/End)
        transform.position = new Vector3(initialX, startY, initialZ);

        // adjust End position (local relative to head)
        if (End != null)
        {
            float localEndY = (endY - startY); // local offset
            End.transform.localPosition = new Vector3(0f, localEndY, 0f);
        }

        // stretch body between Head and End
        if (Body != null && bodyRenderer != null && End != null)
        {
            float localEndY = End.transform.localPosition.y;
            float worldDistance = Mathf.Abs(localEndY);
            float midY = localEndY * 0.5f;

            Body.transform.localPosition = new Vector3(0f, midY, 0f);

            float spriteHeight = bodyRenderer.sprite.bounds.size.y;
            float scaleY = worldDistance / spriteHeight;

            Vector3 bodyScale = Body.transform.localScale;
            bodyScale.x = BodyWidth;
            bodyScale.y = scaleY;
            Body.transform.localScale = bodyScale;
        }

        // Cleanup
        if (songTime > EndTime + 1f)
        {
            if (Head) Destroy(Head);
            if (Body) Destroy(Body);
            if (End) Destroy(End);
            Destroy(gameObject);
        }
    }
}

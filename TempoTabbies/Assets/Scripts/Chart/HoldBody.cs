using UnityEngine;

public class HoldBody : MonoBehaviour
{
    public float StartTime;
    public float EndTime;
    public float ScrollSpeed;
    public AudioSource Music;
    public Transform HitLine;

    void Update()
    {
        if (!Music || !HitLine) return;

        float current = Music.time;
        float startOffset = StartTime - current;
        float endOffset = EndTime - current;

        Vector3 startPos = HitLine.position + Vector3.up * (startOffset * ScrollSpeed);
        Vector3 endPos = HitLine.position + Vector3.up * (endOffset * ScrollSpeed);

        transform.position = (startPos + endPos) * 0.5f;
        transform.localScale = new Vector3(
            transform.localScale.x,
            Mathf.Abs(startPos.y - endPos.y),
            transform.localScale.z
        );

        if (current > EndTime + 0.5f)
            Destroy(gameObject);
    }
}

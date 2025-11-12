using UnityEngine;

public class Note : MonoBehaviour
{
    public float TargetTime;
    public float ScrollSpeed = 6f;
    public AudioSource Music;
    public Transform HitLine;
    public int Lane;
    public bool Hit;

    private float initialX;
    private float initialZ;
    private bool started = false;

    void Start()
    {
        initialX = transform.position.x;
        initialZ = transform.position.z;
        started = true;
    }

    void Update()
    {
        if (!started) return;
        if (Music == null || HitLine == null) return;

        // Use the corrected song time
        float songTime = GameManager.SongTime;
        float timeUntilHit = TargetTime - songTime;
        float y = HitLine.position.y + (timeUntilHit * ScrollSpeed);
        transform.position = new Vector3(initialX, y, initialZ);

        if (timeUntilHit < -0.5f) Destroy(gameObject);
    }
}

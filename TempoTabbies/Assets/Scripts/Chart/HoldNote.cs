using UnityEngine;
using UnityEngine.InputSystem;

public class HoldNote : MonoBehaviour
{
    public float StartTime;
    public float EndTime;
    public float ScrollSpeed;
    public int Lane;
    public AudioSource Music;
    public Transform HitLine;

    [Header("Hold Components")]
    public GameObject Head;
    public GameObject Body;
    public GameObject End;

    [Header("Body Settings")]
    public float BodyWidth = 0.25f;

    private SpriteRenderer bodyRenderer;
    private Gamepad gamepad;

    private bool hasStartedHold;
    private bool hasEnded;
    private bool releaseChecked;

    private const float ReleaseLeniency = 1.5f; // 1.5× timing window leniency

    void Start()
    {
        if (Body != null)
        {
            bodyRenderer = Body.GetComponent<SpriteRenderer>();
            var s = Body.transform.localScale;
            s.x = BodyWidth;
            Body.transform.localScale = s;
        }

        if (End != null)
        {
            var s = End.transform.localScale;
            s.x = BodyWidth;
            s.y = BodyWidth;
            End.transform.localScale = s;
        }

        if (Head != null)
        {
            var s = Head.transform.localScale;
            s.x = BodyWidth;
            Head.transform.localScale = s;
        }
    }

    void Update()
    {
        if (!Music || !HitLine || hasEnded) return;

        gamepad = Gamepad.current;
        float songTime = GameManager.SongTime;

        float timeUntilStart = StartTime - songTime;
        float timeUntilEnd = EndTime - songTime;

        float startY = HitLine.position.y + timeUntilStart * ScrollSpeed;
        float endY = HitLine.position.y + timeUntilEnd * ScrollSpeed;

        // --- BEFORE HOLD START ---
        if (!hasStartedHold)
        {
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);

            // start holding if pressed near receptor
            if (songTime >= StartTime - 0.05f && songTime <= StartTime + 0.1f && IsPressedForLane(Lane))
            {
                hasStartedHold = true;
                transform.position = new Vector3(transform.position.x, HitLine.position.y, transform.position.z);
            }

            UpdateBodyWorld(startY, endY);
            return;
        }

        // --- WHILE HOLDING ---
        // keep head locked to receptor
        transform.position = new Vector3(transform.position.x, HitLine.position.y, transform.position.z);

        // end keeps scrolling up as song plays
        float remaining = Mathf.Max(EndTime - songTime, 0f);
        float localEndY = remaining * ScrollSpeed;
        if (End != null)
            End.transform.localPosition = new Vector3(0f, localEndY, 0f);

        UpdateBodyLocal(localEndY);

        // released early
        if (!IsPressedForLane(Lane) && songTime < EndTime)
        {
            RegisterReleaseJudgment(songTime);
            DestroyHold();
            return;
        }

        // natural end
        if (songTime >= EndTime && !releaseChecked)
        {
            RegisterReleaseJudgment(songTime);
            hasEnded = true;
            DestroyHold();
        }
    }

    // --- updates body before hold starts (note still falling) ---
    private void UpdateBodyWorld(float startY, float endY)
    {
        if (Body == null || bodyRenderer == null || End == null) return;

        float worldDistance = Mathf.Abs(endY - startY);
        float midY = (endY + startY) * 0.5f;

        End.transform.position = new Vector3(transform.position.x, endY, transform.position.z);
        Body.transform.position = new Vector3(transform.position.x, midY, transform.position.z);

        float spriteHeight = bodyRenderer.sprite.bounds.size.y;
        float scaleY = worldDistance / spriteHeight;

        Vector3 s = Body.transform.localScale;
        s.x = BodyWidth;
        s.y = scaleY;
        Body.transform.localScale = s;
    }

    // --- updates body while holding (note frozen at receptor) ---
    private void UpdateBodyLocal(float localEndY)
    {
        if (Body == null || bodyRenderer == null || End == null) return;

        float worldDistance = Mathf.Abs(localEndY);
        float midY = localEndY * 0.5f;

        Body.transform.localPosition = new Vector3(0f, midY, 0f);

        float spriteHeight = bodyRenderer.sprite.bounds.size.y;
        float scaleY = worldDistance / spriteHeight;

        Vector3 s = Body.transform.localScale;
        s.x = BodyWidth;
        s.y = scaleY;
        Body.transform.localScale = s;
    }

    private void RegisterReleaseJudgment(float currentTime)
    {
        if (releaseChecked) return;
        releaseChecked = true;

        // Measure the *offset* from the correct release time.
        float diff = currentTime - EndTime; // positive if late, negative if early
        float absDiff = Mathf.Abs(diff);

        float marv = TimingWindows.Marvelous * ReleaseLeniency;
        float perf = TimingWindows.Perfect * ReleaseLeniency;
        float great = TimingWindows.Great * ReleaseLeniency;
        float good = TimingWindows.Good * ReleaseLeniency;
        float bad = TimingWindows.Bad * ReleaseLeniency;

        string result =
            absDiff <= marv ? "MARVELOUS" :
            absDiff <= perf ? "PERFECT" :
            absDiff <= great ? "GREAT" :
            absDiff <= good ? "GOOD" :
            absDiff <= bad ? "BAD" : "MISS";

        if (JudgmentDisplay.Instance != null)
            JudgmentDisplay.Instance.Show(result);

        Debug.Log($"[HoldNote] Release Judgment: {result} (?={diff * 1000f:F1} ms)");

        // Optional: add a small delay so the player can see the judgment
        Invoke(nameof(DestroyHold), 0.05f);
    }



    private void DestroyHold()
    {
        if (Head) Destroy(Head);
        if (Body) Destroy(Body);
        if (End) Destroy(End);
        Destroy(gameObject);
    }

    private bool IsPressedForLane(int lane)
    {
        if (gamepad == null) return false;

        return lane switch
        {
            0 => gamepad.leftTrigger.isPressed,
            1 => gamepad.leftShoulder.isPressed,
            2 => gamepad.rightShoulder.isPressed,
            3 => gamepad.rightTrigger.isPressed,
            4 => gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f,
            5 => gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f,
            _ => false,
        };
    }
}

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

    private bool initialPressScored = false;
    private bool initialPressMissed = false;
    private bool releaseJudgmentGiven = false;

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

            // Check for MISS if we passed the hit window without pressing
            if (!initialPressScored && !initialPressMissed && songTime > StartTime + TimingWindows.Bad)
            {
                MissInitialPress();
                initialPressMissed = true;
            }

            // start holding if pressed near receptor
            if (songTime >= StartTime - 0.05f && songTime <= StartTime + 0.1f && IsPressedForLane(Lane))
            {
                hasStartedHold = true;
                transform.position = new Vector3(transform.position.x, HitLine.position.y, transform.position.z);

                // ADD INITIAL PRESS SCORING
                if (!initialPressScored)
                {
                    ScoreInitialPress(songTime);
                    initialPressScored = true;
                }
            }

            UpdateBodyWorld(startY, endY);
            return;
        }

        // --- WHILE HOLDING ---
        transform.position = new Vector3(transform.position.x, HitLine.position.y, transform.position.z);

        // Check if player is still holding the note
        bool stillHolding = IsPressedForLane(Lane);

        // If player let go TOO EARLY, count as miss
        if (!stillHolding && songTime < EndTime - TimingWindows.Bad)
        {
            EarlyReleaseMiss();
            return;
        }

        float remaining = Mathf.Max(EndTime - songTime, 0f);
        float localEndY = remaining * ScrollSpeed;
        if (End != null)
            End.transform.localPosition = new Vector3(0f, localEndY, 0f);

        UpdateBodyLocal(localEndY);

        // FORCE RELEASE JUDGMENT when we pass the end time (normal case)
        if (songTime >= EndTime && !releaseJudgmentGiven)
        {
            float releaseTime = stillHolding ? EndTime : songTime;
            RegisterReleaseJudgment(releaseTime);
            releaseJudgmentGiven = true;
            hasEnded = true;
            DestroyHold();
        }
    }


    private void MissInitialPress()
    {
        Debug.Log($"[HoldNote] MISSED initial press");

        if (JudgmentDisplay.Instance != null)
            JudgmentDisplay.Instance.Show("MISS");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddJudgment("MISS");
        }

        // Destroy the hold note since initial press was missed
        DestroyHold();
    }

    private void EarlyReleaseMiss()
    {
        Debug.Log($"[HoldNote] EARLY RELEASE - MISS");

        if (JudgmentDisplay.Instance != null)
            JudgmentDisplay.Instance.Show("MISS");

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddJudgment("MISS");
        }

        DestroyHold();
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

    private void ScoreInitialPress(float currentTime)
    {
        float diff = currentTime - StartTime;
        float absDiff = Mathf.Abs(diff);

        string result;

        if (absDiff <= TimingWindows.Marvelous) result = "MARVELOUS";
        else if (absDiff <= TimingWindows.Perfect) result = "PERFECT";
        else if (absDiff <= TimingWindows.Great) result = "GREAT";
        else if (absDiff <= TimingWindows.Good) result = "GOOD";
        else if (absDiff <= TimingWindows.Bad) result = "BAD";
        else result = "MISS";

        if (JudgmentDisplay.Instance != null)
            JudgmentDisplay.Instance.Show(result);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddJudgment(result);
        }

        Debug.Log($"[HoldNote] Initial Press: {result} (?={diff * 1000f:F1} ms)");
    }

    private void RegisterReleaseJudgment(float currentTime)
    {
        if (releaseChecked) return;
        releaseChecked = true;

        // Only score release if initial press was successful
        if (!initialPressScored || initialPressMissed)
        {
            DestroyHold();
            return;
        }

        // Measure the *offset* from the correct release time.
        float diff = currentTime - EndTime; // Positive if late, negative if early
        float absDiff = Mathf.Abs(diff);

        // USE LENIENCY for release timing
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

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddJudgment(result);
        }

        Debug.Log($"[HoldNote] Release Judgment: {result} (?={diff * 1000f:F1} ms)");

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

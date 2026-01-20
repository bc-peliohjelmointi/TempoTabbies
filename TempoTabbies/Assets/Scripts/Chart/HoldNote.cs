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

    [Header("Hit Effects")]
    public SimpleHitSprite hitEffectManager; // Add this line - same as HitManager uses

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

    // Previous-frame held states for gamepad buttons/sticks (used to detect press-this-frame)
    private bool prevLeftTriggerHeld = false;
    private bool prevRightTriggerHeld = false;
    private bool prevLeftShoulderHeld = false;
    private bool prevRightShoulderHeld = false;
    private bool prevStickLeftHeld = false;
    private bool prevStickRightHeld = false;

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
        var keyboard = Keyboard.current;

        // Current song time
        float songTime = GameManager.SongTime;

        float timeUntilStart = StartTime - songTime;
        float timeUntilEnd = EndTime - songTime;

        float startY = HitLine.position.y + timeUntilStart * ScrollSpeed;
        float endY = HitLine.position.y + timeUntilEnd * ScrollSpeed;

        // Compute current held states (gamepad OR keyboard)
        bool curLeftTriggerHeld = (gamepad != null && gamepad.leftTrigger.isPressed) || (keyboard != null && keyboard.sKey.isPressed);
        bool curLeftShoulderHeld = (gamepad != null && gamepad.leftShoulder.isPressed) || (keyboard != null && keyboard.dKey.isPressed);
        bool curRightShoulderHeld = (gamepad != null && gamepad.rightShoulder.isPressed) || (keyboard != null && keyboard.commaKey.isPressed);
        bool curRightTriggerHeld = (gamepad != null && gamepad.rightTrigger.isPressed) || (keyboard != null && keyboard.periodKey.isPressed);

        bool curStickLeftHeld = false;
        bool curStickRightHeld = false;
        if (gamepad != null)
        {
            curStickLeftHeld = gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f;
            curStickRightHeld = gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f;
        }

        // --- BEFORE HOLD START ---
        if (!hasStartedHold)
        {
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);

            // Check for MISS if we passed the hit window without pressing
            if (!initialPressScored && !initialPressMissed && songTime > StartTime + TimingWindows.Bad)
            {
                MissInitialPress();
                initialPressMissed = true;

                // update prev states and exit
                UpdatePrevHeldStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftShoulderHeld, curRightShoulderHeld, curStickLeftHeld, curStickRightHeld);
                return;
            }

            // Determine press-this-frame for the lane (require an edge/wasPressedThisFrame)
            bool pressedThisFrame = IsPressedThisFrameForLane(Lane, keyboard,
                curLeftTriggerHeld, curRightTriggerHeld, curLeftShoulderHeld, curRightShoulderHeld, curStickLeftHeld, curStickRightHeld);

            // start holding only if the input was pressed THIS FRAME within the timing window
            if (songTime >= StartTime - 0.05f && songTime <= StartTime + 0.1f && pressedThisFrame)
            {
                hasStartedHold = true;
                transform.position = new Vector3(transform.position.x, HitLine.position.y, transform.position.z);

                // ADD INITIAL PRESS SCORING
                if (!initialPressScored)
                {
                    ScoreInitialPress(songTime);
                    initialPressScored = true;
                }

                // update prev states and continue (we'll handle body in next block)
                UpdatePrevHeldStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftShoulderHeld, curRightShoulderHeld, curStickLeftHeld, curStickRightHeld);
                return;
            }

            UpdateBodyWorld(startY, endY);

            // update prev states for edge detection next frame
            UpdatePrevHeldStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftShoulderHeld, curRightShoulderHeld, curStickLeftHeld, curStickRightHeld);
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
            UpdatePrevHeldStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftShoulderHeld, curRightShoulderHeld, curStickLeftHeld, curStickRightHeld);
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

        // update prev states for edge detection next frame
        UpdatePrevHeldStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftShoulderHeld, curRightShoulderHeld, curStickLeftHeld, curStickRightHeld);
    }


    private void MissInitialPress()
    {
        Debug.Log($"[HoldNote] MISSED initial press");

        if (JudgmentDisplay.Instance != null)
            JudgmentDisplay.Instance.Show("MISS", false, false); // No direction for misses

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
            JudgmentDisplay.Instance.Show("MISS", false, false); // No direction for misses

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

    // In the ScoreInitialPress method of HoldNote:
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

        // Pass timing direction information
        bool isEarly = diff < 0;
        bool isLate = diff > 0;

        if (JudgmentDisplay.Instance != null)
            JudgmentDisplay.Instance.Show(result, isEarly, isLate);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddJudgment(result);
        }


        Debug.Log($"[HoldNote] Initial Press: {result} (?={diff * 1000f:F1} ms)");
    }

    // In the RegisterReleaseJudgment method of HoldNote:
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

        // Pass timing direction information
        bool isEarly = diff < 0;
        bool isLate = diff > 0;

        if (JudgmentDisplay.Instance != null)
            JudgmentDisplay.Instance.Show(result, isEarly, isLate);

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

    // Detect a press that occurred this frame for the given lane (supports keyboard and gamepad)
    private bool IsPressedThisFrameForLane(int lane, Keyboard keyboard,
        bool curLeftTriggerHeld, bool curRightTriggerHeld, bool curLeftShoulderHeld, bool curRightShoulderHeld,
        bool curStickLeftHeld, bool curStickRightHeld)
    {
        // Keyboard mapping: S -> left trigger (lane 0), D -> left bumper (lane 1), , -> right bumper (lane 2), . -> right trigger (lane 3)
        bool kbLeftTriggerPressed = keyboard != null && keyboard.sKey.wasPressedThisFrame;
        bool kbLeftBumperPressed = keyboard != null && keyboard.dKey.wasPressedThisFrame;
        bool kbRightBumperPressed = keyboard != null && keyboard.commaKey.wasPressedThisFrame;
        bool kbRightTriggerPressed = keyboard != null && keyboard.periodKey.wasPressedThisFrame;

        return lane switch
        {
            0 => (gamepad != null && curLeftTriggerHeld && !prevLeftTriggerHeld) || kbLeftTriggerPressed,
            1 => (gamepad != null && curLeftShoulderHeld && !prevLeftShoulderHeld) || kbLeftBumperPressed,
            2 => (gamepad != null && curRightShoulderHeld && !prevRightShoulderHeld) || kbRightBumperPressed,
            3 => (gamepad != null && curRightTriggerHeld && !prevRightTriggerHeld) || kbRightTriggerPressed,
            4 => (gamepad != null && curStickLeftHeld && !prevStickLeftHeld), // stick presses only from gamepad
            5 => (gamepad != null && curStickRightHeld && !prevStickRightHeld), // stick presses only from gamepad
            _ => false,
        };
    }

    private void UpdatePrevHeldStates(bool curLeftTriggerHeld, bool curRightTriggerHeld, bool curLeftShoulderHeld, bool curRightShoulderHeld, bool curStickLeftHeld, bool curStickRightHeld)
    {
        prevLeftTriggerHeld = (gamepad != null) && curLeftTriggerHeld;
        prevRightTriggerHeld = (gamepad != null) && curRightTriggerHeld;
        prevLeftShoulderHeld = (gamepad != null) && curLeftShoulderHeld;
        prevRightShoulderHeld = (gamepad != null) && curRightShoulderHeld;
        prevStickLeftHeld = (gamepad != null) && curStickLeftHeld;
        prevStickRightHeld = (gamepad != null) && curStickRightHeld;
    }
}

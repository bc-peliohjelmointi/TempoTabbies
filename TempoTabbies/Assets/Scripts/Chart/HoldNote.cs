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

    [Header("Per-note score manager (set by spawner)")]
    public ScoreManager scoreManager;

    [Header("Owner HitManager (set by spawner / HitManager when starting hold)")]
    public HitManager OwnerHitManager;

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

        // --- Robust fallback binding for singleplayer ---
        // If OwnerHitManager wasn't provided by the spawner, try to bind a sensible HitManager now.
        if (OwnerHitManager == null)
        {
            // Prefer HitManager referenced on the parent NoteSpawner (if any)
            var parentSpawner = GetComponentInParent<NoteSpawner>();
            if (parentSpawner != null && parentSpawner.hitManager != null)
            {
                OwnerHitManager = parentSpawner.hitManager;
            }
            else
            {
                // Last resort: pick any HitManager in the scene (singleplayer fallback)
                OwnerHitManager = FindObjectOfType<HitManager>();
            }

            if (OwnerHitManager != null)
            {
                Debug.Log($"[HoldNote] Bound to HitManager '{OwnerHitManager.name}' via fallback in Start().");
                // Copy missing managers from owner so this hold uses the same Score/Judgment/Effects
                if (scoreManager == null)
                    scoreManager = OwnerHitManager.scoreManager;
                if (hitEffectManager == null)
                    hitEffectManager = OwnerHitManager.hitEffectManager;
            }
            else
            {
                Debug.LogWarning("[HoldNote] No HitManager found for fallback binding; keyboard/gamepad fallback will still be used.");
            }
        }
    }

    [System.Obsolete]
    void Update()
    {
        if (!Music || !HitLine || hasEnded) return;

        // Use owner hit manager's assigned pad when available.
        // FIX: fall back to Gamepad.current when AssignedGamepad is null (singleplayer)
        gamepad = OwnerHitManager?.AssignedGamepad ?? Gamepad.current;

        // Decide keyboard usage:
        // - if no OwnerHitManager -> allow keyboard
        // - if OwnerHitManager.AcceptKeyboard -> allow keyboard
        // - otherwise, if there is no gamepad present/assigned -> allow keyboard as fallback
        bool allowKeyboard;
        if (OwnerHitManager == null)
        {
            allowKeyboard = true;
        }
        else
        {
            allowKeyboard = OwnerHitManager.AcceptKeyboard;
            if (!allowKeyboard)
            {
                // If owner disallows keyboard but there is no gamepad available, enable keyboard fallback
                bool hasAssignedPad = (OwnerHitManager.AssignedGamepad != null) || (Gamepad.current != null);
                if (!hasAssignedPad)
                    allowKeyboard = true;
            }
        }

        var keyboard = allowKeyboard ? Keyboard.current : null;

        // Current song time
        float songTime = GameManager.SongTime;

        float timeUntilStart = StartTime - songTime;
        float timeUntilEnd = EndTime - songTime;

        float startY = HitLine.position.y + timeUntilStart * ScrollSpeed;
        float endY = HitLine.position.y + timeUntilEnd * ScrollSpeed;

        // Compute current held states (gamepad OR keyboard)
        bool curLeftTriggerHeld = (gamepad != null && OwnerHitManager.button1.IsPressed()) || (keyboard != null && keyboard.sKey.isPressed);
        bool curLeftShoulderHeld = (gamepad != null && OwnerHitManager.button2.IsPressed()) || (keyboard != null && keyboard.dKey.isPressed);
        bool curRightShoulderHeld = (gamepad != null && OwnerHitManager.button3.IsPressed()) || (keyboard != null && keyboard.commaKey.isPressed);
        bool curRightTriggerHeld = (gamepad != null && OwnerHitManager.button4.IsPressed()) || (keyboard != null && keyboard.periodKey.isPressed);

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

                // Ensure OwnerHitManager is set when the hold starts (helps consistent input logic)
                if (OwnerHitManager == null)
                {
                    var parentSpawner = GetComponentInParent<NoteSpawner>();
                    if (parentSpawner != null && parentSpawner.hitManager != null)
                        OwnerHitManager = parentSpawner.hitManager;
                    else if (OwnerHitManager == null)
                        OwnerHitManager = FindObjectOfType<HitManager>();

                    if (OwnerHitManager != null)
                    {
                        if (scoreManager == null) scoreManager = OwnerHitManager.scoreManager;
                        if (hitEffectManager == null) hitEffectManager = OwnerHitManager.hitEffectManager;
                      }
                }

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

            // If initial press was missed, keep the hold note in scene but allow release judgment
            // so the player can still attempt the release timing window.
            if (initialPressMissed && !releaseJudgmentGiven && songTime >= EndTime)
            {
                bool stillHoldingForRelease = IsPressedForLane(Lane);
                float releaseTime = stillHoldingForRelease ? EndTime : songTime;
                RegisterReleaseJudgment(releaseTime);
                releaseJudgmentGiven = true;
                hasEnded = true;
                // update prev states and exit (RegisterReleaseJudgment will destroy after a short delay)
                UpdatePrevHeldStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftShoulderHeld, curRightShoulderHeld, curStickLeftHeld, curStickRightHeld);
                return;
            }

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

        ShowJudgment("MISS", false, false); // No direction for misses

        if (scoreManager != null)
        {
            scoreManager.AddJudgment("MISS");
        }

        // Do not destroy the hold note on initial miss — dim it so the player
        // can still attempt to hit the release timing window.
        DimNoteVisuals(0.6f);
    }

    // Dim the visual components of this hold note (head/body/end) by a factor.
    private void DimNoteVisuals(float factor)
    {
        if (Head != null)
        {
            var sr = Head.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                sr.color = new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
            }
        }

        if (Body != null && bodyRenderer != null)
        {
            var c = bodyRenderer.color;
            bodyRenderer.color = new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
        }

        if (End != null)
        {
            var sr = End.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                sr.color = new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
            }
        }
    }

    private void EarlyReleaseMiss()
    {
        Debug.Log($"[HoldNote] EARLY RELEASE - MISS");

        ShowJudgment("MISS", false, false); // No direction for misses

        if (scoreManager != null)
        {
            scoreManager.AddJudgment("MISS");
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

    // Called by HitManager when it starts this hold
    public void StartHoldFromHitManager(float songTime)
    {
        if (!hasStartedHold)
        {
            hasStartedHold = true;
            transform.position = new Vector3(transform.position.x, HitLine.position.y, transform.position.z);

            if (!initialPressScored)
            {
                ScoreInitialPress(songTime);
                initialPressScored = true;
            }
        }
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

        ShowJudgment(result, isEarly, isLate);

        if (scoreManager != null)
        {
            scoreManager.AddJudgment(result);
        }


        Debug.Log($"[HoldNote] Initial Press: {result} (?={diff * 1000f:F1} ms)");
    }

    // In the RegisterReleaseJudgment method of HoldNote:
    private void RegisterReleaseJudgment(float currentTime)
    {
        if (releaseChecked) return;
        releaseChecked = true;

        // If we never had an initial state (neither scored nor explicitly missed),
        // don't process release — just destroy the hold.
        if (!initialPressScored && !initialPressMissed)
        {
            DestroyHold();
            return;
        }

        // If the initial press was missed and the player never started the hold
        // later (never actually held the note), the release must be a MISS.
        if (initialPressMissed && !hasStartedHold)
        {
            ShowJudgment("MISS", false, false);
            if (scoreManager != null)
                scoreManager.AddJudgment("MISS");
            Debug.Log("[HoldNote] Release Judgment: MISS (never held)");
            Invoke(nameof(DestroyHold), 0.05f);
            return;
        }

        // Otherwise measure the *offset* from the correct release time.
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

        ShowJudgment(result, isEarly, isLate);

        if (scoreManager != null)
        {
            scoreManager.AddJudgment(result);
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
        // Allow keyboard fallback for singleplayer or when OwnerHitManager permits it
        var keyboard = (OwnerHitManager != null ? OwnerHitManager.AcceptKeyboard : true) ? Keyboard.current : null;

        if (lane == 0)
            return (gamepad != null && OwnerHitManager.button1.IsPressed()) || (keyboard != null && keyboard.sKey.isPressed);
        if (lane == 1)
            return (gamepad != null && OwnerHitManager.button2.IsPressed()) || (keyboard != null && keyboard.dKey.isPressed);
        if (lane == 2)
            return (gamepad != null && OwnerHitManager.button3.IsPressed()) || (keyboard != null && keyboard.commaKey.isPressed);
        if (lane == 3)
            return (gamepad != null && OwnerHitManager.button4.IsPressed()) || (keyboard != null && keyboard.periodKey.isPressed);
        if (lane == 4)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f));
        if (lane == 5)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f));

        return false;
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

    // Show judgment on the correct player's display (owner HitManager), fallback to scene instance
    private void ShowJudgment(string label, bool isEarly = false, bool isLate = false)
    {
        JudgmentDisplay jd = null;
        if (OwnerHitManager != null)
            jd = OwnerHitManager.JudgmentDisplay;
        if (jd == null)
            jd = FindObjectOfType<JudgmentDisplay>(); // fallback

        if (jd != null)
            jd.Show(label, isEarly, isLate);
    }
}

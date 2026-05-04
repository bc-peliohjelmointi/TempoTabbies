using System.Collections;
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
    private _GameManager _gm;

    [Header("Hold Components")]
    public GameObject Head;
    private SpriteRenderer sr; // For taco rotation
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
        _gm = FindFirstObjectByType<_GameManager>();
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
            if (_gm.taco)
            {
                SpriteRenderer sr = Head.GetComponent<SpriteRenderer>();
                sr.sprite = Body.GetComponent<Note>().tacoSr.sprite;
            }
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
                OwnerHitManager = FindFirstObjectByType<HitManager>();
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
        if (_gm.taco)
        {
            if (Head != null)
            {
                Head.transform.localEulerAngles += new Vector3(0, 0, 5);
            }
        }

        if (!Music || !HitLine || hasEnded) return;

        // Use owner hit manager's assigned pad when available.
        // Only fall back to Gamepad.current in singleplayer to avoid one physical
        // controller driving both players in multiplayer.
        var assigned = OwnerHitManager?.AssignedGamepad;
        bool singleplayerAllowAnyGamepad = _GameManager.instance == null || !_GameManager.instance.multiplayer;
        gamepad = assigned ?? (singleplayerAllowAnyGamepad ? Gamepad.current : null);

        // Decide keyboard usage: rely on OwnerHitManager.AcceptKeyboard (or allow if no owner).
        bool allowKeyboard = OwnerHitManager == null ? true : OwnerHitManager.AcceptKeyboard;
        var keyboard = allowKeyboard ? Keyboard.current : null;

        // Current song time
        float songTime = GameManager.SongTime;

        float timeUntilStart = StartTime - songTime;
        float timeUntilEnd = EndTime - songTime;

        float startY = HitLine.position.y + timeUntilStart * ScrollSpeed;
        float endY = HitLine.position.y + timeUntilEnd * ScrollSpeed;
        // Compute current held states (gamepad OR keyboard)
        bool curLeftTriggerHeld = (gamepad != null && OwnerHitManager != null && OwnerHitManager.button1 != null && OwnerHitManager.button1.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key1.isPressed);
        bool curLeftShoulderHeld = (gamepad != null && OwnerHitManager != null && OwnerHitManager.button2 != null && OwnerHitManager.button2.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key2.isPressed);
        bool curRightShoulderHeld = (gamepad != null && OwnerHitManager != null && OwnerHitManager.button3 != null && OwnerHitManager.button3.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key3.isPressed);
        bool curRightTriggerHeld = (gamepad != null && OwnerHitManager != null && OwnerHitManager.button4 != null && OwnerHitManager.button4.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key4.isPressed);

        bool curStickLeftHeld = false;
        bool curStickRightHeld = false;
        if (gamepad != null)
        {
            curStickLeftHeld = gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f;
            curStickRightHeld = gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f;
        }
        // Keyboard swipe fallbacks
        if (keyboard != null)
        {
            curStickLeftHeld = curStickLeftHeld || (!OwnerHitManager.gamepadOn && OwnerHitManager.swipeLeft.isPressed);
            curStickRightHeld = curStickRightHeld || (!OwnerHitManager.gamepadOn && OwnerHitManager.swipeRight.isPressed);
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
                        OwnerHitManager = FindFirstObjectByType<HitManager>();

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
            StartCoroutine(Rumble());
        }

        // Do not destroy the hold note on initial miss — dim it so the player
        // can still attempt to hit the release timing window.
        DimNoteVisuals(0.6f);
    }
    public IEnumerator Rumble()
    {
        if (OwnerHitManager.motorOn || !OwnerHitManager._gm.missRumble)
        {
            yield break;
        }
        Debug.LogError("Rumble!");
        Gamepad assignedGamepad = OwnerHitManager?.AssignedGamepad;
        assignedGamepad?.SetMotorSpeeds(0.5f, 0.5f);
        yield return new WaitForSeconds(0.05f);
        assignedGamepad?.SetMotorSpeeds(0f, 0f);
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
            StartCoroutine(Rumble());
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
            StartCoroutine(Rumble());
            if (scoreManager != null)
                scoreManager.AddJudgment("MISS");
            Debug.Log("[HoldNote] Release Judgment: MISS (never held)");
            // Schedule root GameObject destruction safely (does not require this MonoBehaviour)
            Destroy(gameObject, 0.05f);
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

        // Schedule destruction of the root GameObject; this avoids invoking on a destroyed MonoBehaviour
        Destroy(gameObject, 0.05f);
    }
    private void DestroyHold()
    {
        if (Head) Destroy(Head);
        if (Body) Destroy(Body);
        if (End) Destroy(End);
        Destroy(gameObject);
    }

    // Called by HitManager when it wants the hold to process a release (e.g., player let go or hold ended).
    public void EndHoldFromHitManager(float songTime)
    {
        // Only process if we haven't already given a release judgment
        if (releaseChecked) return;

        // Use the same logic as the internal release handling
        RegisterReleaseJudgment(songTime);
    }

    private bool IsPressedForLane(int lane)
    {

        var keyboard = (OwnerHitManager != null ? OwnerHitManager.AcceptKeyboard : true) ? Keyboard.current : null;

        if (lane == 0)
            return (gamepad != null && OwnerHitManager != null && OwnerHitManager.button1 != null && OwnerHitManager.button1.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key1.isPressed);
        if (lane == 1)
            return (gamepad != null && OwnerHitManager != null && OwnerHitManager.button2 != null && OwnerHitManager.button2.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key2.isPressed);
        if (lane == 2)
            return (gamepad != null && OwnerHitManager != null && OwnerHitManager.button3 != null && OwnerHitManager.button3.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key3.isPressed);
        if (lane == 3)
            return (gamepad != null && OwnerHitManager != null && OwnerHitManager.button4 != null && OwnerHitManager.button4.IsPressed()) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key4.isPressed);
        if (lane == 4)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f)) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.swipeLeft.isPressed);
        if (lane == 5)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f)) || (!OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.swipeRight.isPressed);

        return false;
    }

    // Detect a press that occurred this frame for the given lane (supports keyboard and gamepad)
    private bool IsPressedThisFrameForLane(int lane, Keyboard keyboard,
        bool curLeftTriggerHeld, bool curRightTriggerHeld, bool curLeftShoulderHeld, bool curRightShoulderHeld,
        bool curStickLeftHeld, bool curStickRightHeld)
    {
        // Keyboard mapping: S -> left trigger (lane 0), D -> left bumper (lane 1), K -> right bumper (lane 2), L -> right trigger (lane 3)
        // Space -> left stick (lane 4), RightAlt -> right stick (lane 5)
        bool kbLeftTriggerPressed = !OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key1.wasPressedThisFrame;
        bool kbLeftBumperPressed = !OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key2.wasPressedThisFrame;
        bool kbRightBumperPressed = !OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key3.wasPressedThisFrame;
        bool kbRightTriggerPressed = !OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.key4.wasPressedThisFrame;
        bool kbStickLeftPressed = !OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.swipeLeft.wasPressedThisFrame;
        bool kbStickRightPressed = !OwnerHitManager.gamepadOn && keyboard != null && OwnerHitManager.swipeRight.wasPressedThisFrame;

        return lane switch
        {
            0 => (gamepad != null && curLeftTriggerHeld && !prevLeftTriggerHeld) || kbLeftTriggerPressed,
            1 => (gamepad != null && curLeftShoulderHeld && !prevLeftShoulderHeld) || kbLeftBumperPressed,
            2 => (gamepad != null && curRightShoulderHeld && !prevRightShoulderHeld) || kbRightBumperPressed,
            3 => (gamepad != null && curRightTriggerHeld && !prevRightTriggerHeld) || kbRightTriggerPressed,
            4 => (gamepad != null && curStickLeftHeld && !prevStickLeftHeld) || kbStickLeftPressed,
            5 => (gamepad != null && curStickRightHeld && !prevStickRightHeld) || kbStickRightPressed,
            _ => false,
        };
    }

    private void UpdatePrevHeldStates(bool curLeftTriggerHeld, bool curRightTriggerHeld, bool curLeftShoulderHeld, bool curRightShoulderHeld, bool curStickLeftHeld, bool curStickRightHeld)
    {
        // Track previous held states unconditionally so keyboard edges are detected
        // correctly even when no gamepad is present.
        prevLeftTriggerHeld = curLeftTriggerHeld;
        prevRightTriggerHeld = curRightTriggerHeld;
        prevLeftShoulderHeld = curLeftShoulderHeld;
        prevRightShoulderHeld = curRightShoulderHeld;
        prevStickLeftHeld = curStickLeftHeld;
        prevStickRightHeld = curStickRightHeld;
    }

    // Show judgment on the correct player's display (owner HitManager), fallback to scene instance
    private void ShowJudgment(string label, bool isEarly = false, bool isLate = false)
    {
        JudgmentDisplay jd = null;
        if (OwnerHitManager != null)
            jd = OwnerHitManager.JudgmentDisplay;
        if (jd == null)
            jd = FindFirstObjectByType<JudgmentDisplay>(); // fallback

        if (jd != null)
            jd.Show(label, isEarly, isLate);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HitManager : MonoBehaviour
{
    [Header("GameObjects assigned to buttons")]
    public GameObject leftTriggerObject;
    public GameObject rightTriggerObject;
    public GameObject leftBumperObject;
    public GameObject rightBumperObject;
    public GameObject leftStickObject;
    public GameObject rightStickObject;

    [Header("Hit Effects")]
    public SimpleHitSprite hitEffectManager;
    [Header("Score Management")]
    public ScoreManager scoreManager;

    [Header("Rhythm Game References")]
    public AudioSource Music;
    public NoteSpawner Spawner;
    public Transform HitLine;

    [Header("Judgment Display Reference")]
    public JudgmentDisplay JudgmentDisplay;  // <-- Single persistent sprite object

    [Header("Lane Mapping (controller ? chart lane)")]
    public int leftTriggerLane = 0;
    public int leftBumperLane = 1;
    public int rightBumperLane = 2;
    public int rightTriggerLane = 3;
    public int leftStickLane = 4;
    public int rightStickLane = 5;

    [Header("Input options")]
    [Tooltip("When true this HitManager will also accept keyboard fallback input (useful for player 1).")]
    public bool AcceptKeyboard = true;

    private bool stickLeftInitialPress = false;
    private bool stickRightInitialPress = false;

    private Gamepad gamepad;              // currently-used gamepad for this HitManager (kept for legacy use)
    private Gamepad assignedGamepad;      // explicitly assigned device from MultiHitManager
    private readonly Dictionary<int, HoldNote> activeHolds = new();

    // Previous-frame gamepad-held states (used to detect press-this-frame reliably)
    private bool prevGamepadLeftTriggerHeld = false;
    private bool prevGamepadRightTriggerHeld = false;
    private bool prevGamepadLeftShoulderHeld = false;
    private bool prevGamepadRightShoulderHeld = false;
    private bool prevGamepadStickLeftHeld = false;
    private bool prevGamepadStickRightHeld = false;

    void Update()
    {
        // Prefer an explicitly assigned gamepad (from MultiHitManager). If none assigned, fall back to Gamepad.current.
        gamepad = assignedGamepad ?? Gamepad.current;

        // Read keyboard only if configured to accept it. This prevents keyboard from controlling both players.
        var keyboard = AcceptKeyboard ? Keyboard.current : null;

        // Current held states (gamepad OR keyboard)
        bool curLeftTriggerHeld = (gamepad != null && gamepad.leftTrigger.isPressed) || (keyboard != null && keyboard.sKey.isPressed);
        bool curLeftBumperHeld = (gamepad != null && gamepad.leftShoulder.isPressed) || (keyboard != null && keyboard.dKey.isPressed);
        bool curRightBumperHeld = (gamepad != null && gamepad.rightShoulder.isPressed) || (keyboard != null && keyboard.commaKey.isPressed);
        bool curRightTriggerHeld = (gamepad != null && gamepad.rightTrigger.isPressed) || (keyboard != null && keyboard.periodKey.isPressed);

        bool curStickLeftHeld = false;
        bool curStickRightHeld = false;
        if (gamepad != null)
        {
            curStickLeftHeld = gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f;
            curStickRightHeld = gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f;
        }

        // 
        bool leftTriggerPressedThisFrame = (gamepad != null && gamepad.leftTrigger.isPressed && !prevGamepadLeftTriggerHeld)
                                           || (keyboard != null && keyboard.sKey.wasPressedThisFrame);

        bool leftBumperPressedThisFrame = (gamepad != null && gamepad.leftShoulder.isPressed && !prevGamepadLeftShoulderHeld)
                                          || (keyboard != null && keyboard.dKey.wasPressedThisFrame);

        bool rightBumperPressedThisFrame = (gamepad != null && gamepad.rightShoulder.isPressed && !prevGamepadRightShoulderHeld)
                                           || (keyboard != null && keyboard.commaKey.wasPressedThisFrame);

        bool rightTriggerPressedThisFrame = (gamepad != null && gamepad.rightTrigger.isPressed && !prevGamepadRightTriggerHeld)
                                            || (keyboard != null && keyboard.periodKey.wasPressedThisFrame);

        bool stickLeftPressed = (curStickLeftHeld && !prevGamepadStickLeftHeld);
        bool stickRightPressed = (curStickRightHeld && !prevGamepadStickRightHeld);


        // Input visuals (supports both gamepad and keyboard held states)
        if (leftTriggerObject != null)
            leftTriggerObject.SetActive(curLeftTriggerHeld);
        if (rightTriggerObject != null)
            rightTriggerObject.SetActive(curRightTriggerHeld);
        if (leftBumperObject != null)
            leftBumperObject.SetActive(curLeftBumperHeld);
        if (rightBumperObject != null)
            rightBumperObject.SetActive(curRightBumperHeld);

        if (leftStickObject != null)
            leftStickObject.SetActive(curStickLeftHeld);
        if (rightStickObject != null)
            rightStickObject.SetActive(curStickRightHeld);

        if (Music == null || Spawner == null)
        {
            // update previous-frame gamepad states before returning
            UpdatePrevGamepadStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftBumperHeld, curRightBumperHeld, curStickLeftHeld, curStickRightHeld);
            return;
        }

        float songTime = GameManager.SongTime;

        // Tap / hold start detection (only on press-this-frame events)
        if (leftTriggerPressedThisFrame) TryHit(leftTriggerLane, songTime);
        if (leftBumperPressedThisFrame) TryHit(leftBumperLane, songTime);
        if (rightBumperPressedThisFrame) TryHit(rightBumperLane, songTime);
        if (rightTriggerPressedThisFrame) TryHit(rightTriggerLane, songTime);
        if (stickLeftPressed) TryHit(leftStickLane, songTime);
        if (stickRightPressed) TryHit(rightStickLane, songTime);

        // Track stick initial press state for non-gamepad fallbacks
        stickLeftInitialPress = curStickLeftHeld;
        stickRightInitialPress = curStickRightHeld;

        // Update hold tracking (uses held states)
        UpdateHolds(songTime);

        // Detect misses
        CheckForMisses(songTime);

        // store previous gamepad-held states for next-frame edge detection
        UpdatePrevGamepadStates(curLeftTriggerHeld, curRightTriggerHeld, curLeftBumperHeld, curRightBumperHeld, curStickLeftHeld, curStickRightHeld);
    }

    private void UpdatePrevGamepadStates(bool leftTriggerHeld, bool rightTriggerHeld, bool leftBumperHeld, bool rightBumperHeld, bool stickLeftHeld, bool stickRightHeld)
    {
        // Only track previous states for an actual gamepad device (avoid treating keyboard-only as gamepad edge)
        bool haveGamepad = assignedGamepad != null || Gamepad.current != null;

        prevGamepadLeftTriggerHeld = haveGamepad && leftTriggerHeld;
        prevGamepadRightTriggerHeld = haveGamepad && rightTriggerHeld;
        prevGamepadLeftShoulderHeld = haveGamepad && leftBumperHeld;
        prevGamepadRightShoulderHeld = haveGamepad && rightBumperHeld;
        prevGamepadStickLeftHeld = haveGamepad && stickLeftHeld;
        prevGamepadStickRightHeld = haveGamepad && stickRightHeld;
    }

    private void TryHit(int lane, float currentTime)
    {
        Note closestNote = null;
        HoldNote holdHead = null;
        float smallestDiff = float.MaxValue;

        foreach (Transform child in Spawner.transform)
        {
            if (child == null) continue;

            Note note = child.GetComponent<Note>();
            if (note != null && !note.Hit && note.Lane == lane)
            {
                float diff = Mathf.Abs(note.TargetTime - currentTime);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    closestNote = note;
                }
            }

            HoldNote hold = child.GetComponent<HoldNote>();
            if (hold != null && hold.Lane == lane)
            {
                float diff = Mathf.Abs(hold.StartTime - currentTime);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    holdHead = hold;
                }
            }
        }

        if (closestNote != null)
            EvaluateHit(closestNote, currentTime);
        else if (holdHead != null)
            TryStartHold(holdHead, lane, currentTime);
    }

    // In the EvaluateHit method of HitManager:
    private void EvaluateHit(Note note, float currentTime)
    {
        float diff = currentTime - note.TargetTime;
        float absDiff = Mathf.Abs(diff);

        Debug.Log($"[Hit Timing] Note: {note.TargetTime}, Current: {currentTime}, Diff: {diff}");

        string label;

        if (absDiff <= TimingWindows.Marvelous) label = "MARVELOUS";
        else if (absDiff <= TimingWindows.Perfect) label = "PERFECT";
        else if (absDiff <= TimingWindows.Great) label = "GREAT";
        else if (absDiff <= TimingWindows.Good) label = "GOOD";
        else if (absDiff <= TimingWindows.Bad) label = "BAD";
        else return;

        note.Hit = true;
        Destroy(note.gameObject);

        // Pass timing direction information
        bool isEarly = diff < 0;
        bool isLate = diff > 0;
        ShowJudgment(label, isEarly, isLate);

        // ADD SCORE
        if (scoreManager != null)
        {
            scoreManager.AddJudgment(label);
        }

        if (hitEffectManager != null && label != "MISS")
        {
            hitEffectManager.PlayHitEffect(note.Lane, "DEFAULT");
        }

        Debug.Log($"[{label}] lane {note.Lane} ?t={diff * 1000f:F1}ms");
    }

    private void TryStartHold(HoldNote hold, int lane, float currentTime)
    {
        // Only allow starting a hold when the input was a press THIS FRAME
        float diff = Mathf.Abs(hold.StartTime - currentTime);
        if (diff <= TimingWindows.Great && !activeHolds.ContainsKey(lane))
        {
            activeHolds[lane] = hold;
            ShowJudgment("MARVELOUS");
            Debug.Log($"[HOLD START] lane {lane}");
        }
    }

    private void UpdateHolds(float songTime)
    {
        List<int> toRelease = new();

        foreach (var kv in activeHolds)
        {
            int lane = kv.Key;
            HoldNote hold = kv.Value;

            bool stillHeld = IsLanePressed(lane);
            if (!stillHeld || songTime > hold.EndTime)
                toRelease.Add(lane);
        }

        foreach (int lane in toRelease)
        {
            activeHolds.Remove(lane);
            Debug.Log($"[HOLD RELEASE] lane {lane}");
        }
    }

    private bool IsLanePressed(int lane)
    {
        var keyboard = AcceptKeyboard ? Keyboard.current : null;

        if (lane == leftTriggerLane)
            return (gamepad != null && gamepad.leftTrigger.isPressed) || (keyboard != null && keyboard.sKey.isPressed);
        if (lane == rightTriggerLane)
            return (gamepad != null && gamepad.rightTrigger.isPressed) || (keyboard != null && keyboard.periodKey.isPressed);
        if (lane == leftBumperLane)
            return (gamepad != null && gamepad.leftShoulder.isPressed) || (keyboard != null && keyboard.dKey.isPressed);
        if (lane == rightBumperLane)
            return (gamepad != null && gamepad.rightShoulder.isPressed) || (keyboard != null && keyboard.commaKey.isPressed);
        if (lane == leftStickLane)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f));
        if (lane == rightStickLane)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f));

        return false;
    }

    private void CheckForMisses(float currentTime)
    {
        foreach (Transform child in Spawner.transform)
        {
            Note note = child.GetComponent<Note>();
            if (note == null || note.Hit) continue;

            if (currentTime > note.TargetTime + TimingWindows.Bad)
            {
                note.Hit = true;
                ShowJudgment("MISS", false, false); // No direction for misses

                // ADD MISS TO SCORE
                if (scoreManager != null)
                {
                    scoreManager.AddJudgment("MISS");
                }

                Destroy(note.gameObject);
                Debug.Log($"[MISS] lane {note.Lane}");
            }
        }
    }

    public void InitializeChart(int totalNotes)
    {
        if (scoreManager != null)
        {
            scoreManager.InitializeScore(totalNotes);
        }
    }

    private void ShowJudgment(string label, bool isEarly = false, bool isLate = false)
    {
        if (JudgmentDisplay != null)
            JudgmentDisplay.Show(label, isEarly, isLate);
    }

    // Called by MultiHitManager to explicitly bind a gamepad to this HitManager.
    public void SetGamepad(Gamepad pad)
    {
        assignedGamepad = pad;
        // If an explicit pad was provided, set 'gamepad' now so visuals respond immediately
        if (assignedGamepad != null)
            gamepad = assignedGamepad;
    }
}

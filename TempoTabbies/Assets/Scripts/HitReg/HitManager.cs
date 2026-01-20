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

    private bool stickLeftInitialPress = false;
    private bool stickRightInitialPress = false;


    private Gamepad gamepad;
    private readonly Dictionary<int, HoldNote> activeHolds = new();

    void Update()
    {
        gamepad = Gamepad.current;
        var keyboard = Keyboard.current;

        // TEMPORARY KEYBOARD INPUT
        bool kbLeftTriggerPressed = keyboard != null && keyboard.sKey.wasPressedThisFrame;
        bool kbLeftTriggerHeld = keyboard != null && keyboard.sKey.isPressed;

        bool kbLeftBumperPressed = keyboard != null && keyboard.dKey.wasPressedThisFrame;
        bool kbLeftBumperHeld = keyboard != null && keyboard.dKey.isPressed;

        bool kbRightBumperPressed = keyboard != null && keyboard.commaKey.wasPressedThisFrame;
        bool kbRightBumperHeld = keyboard != null && keyboard.commaKey.isPressed;

        bool kbRightTriggerPressed = keyboard != null && keyboard.periodKey.wasPressedThisFrame;
        bool kbRightTriggerHeld = keyboard != null && keyboard.periodKey.isPressed;

        // --- Input visuals (supports both gamepad and keyboard) ---
        if (leftTriggerObject != null)
            leftTriggerObject.SetActive((gamepad != null && gamepad.leftTrigger.isPressed) || kbLeftTriggerHeld);
        if (rightTriggerObject != null)
            rightTriggerObject.SetActive((gamepad != null && gamepad.rightTrigger.isPressed) || kbRightTriggerHeld);
        if (leftBumperObject != null)
            leftBumperObject.SetActive((gamepad != null && gamepad.leftShoulder.isPressed) || kbLeftBumperHeld);
        if (rightBumperObject != null)
            rightBumperObject.SetActive((gamepad != null && gamepad.rightShoulder.isPressed) || kbRightBumperHeld);

        bool stickLeftHeld = false;
        bool stickRightHeld = false;

        if (gamepad != null)
        {
            stickLeftHeld = gamepad.leftStick.ReadValue().x < -0.5f ||
                            gamepad.rightStick.ReadValue().x < -0.5f;
            stickRightHeld = gamepad.leftStick.ReadValue().x > 0.5f ||
                             gamepad.rightStick.ReadValue().x > 0.5f;
        }

        bool stickLeftPressed = stickLeftHeld && !stickLeftInitialPress;
        bool stickRightPressed = stickRightHeld && !stickRightInitialPress;


        if (leftStickObject != null)
            leftStickObject.SetActive(stickLeftHeld);
        if (rightStickObject != null)
            rightStickObject.SetActive(stickRightHeld);

        if (Music == null || Spawner == null)
            return;

        float songTime = GameManager.SongTime;

        // Tap / hold start detection
        if ((gamepad != null && gamepad.leftTrigger.wasPressedThisFrame) || kbLeftTriggerPressed) TryHit(leftTriggerLane, songTime);
        if ((gamepad != null && gamepad.leftShoulder.wasPressedThisFrame) || kbLeftBumperPressed) TryHit(leftBumperLane, songTime);
        if ((gamepad != null && gamepad.rightShoulder.wasPressedThisFrame) || kbRightBumperPressed) TryHit(rightBumperLane, songTime);
        if ((gamepad != null && gamepad.rightTrigger.wasPressedThisFrame) || kbRightTriggerPressed) TryHit(rightTriggerLane, songTime);
        if (stickLeftPressed) TryHit(leftStickLane, songTime);
        if (stickRightPressed) TryHit(rightStickLane, songTime);

        stickLeftInitialPress = stickLeftHeld;
        stickRightInitialPress = stickRightHeld;

        // Update hold tracking
        UpdateHolds(songTime);

        // Detect misses
        CheckForMisses(songTime);
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
        var keyboard = Keyboard.current;

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

    public void SetGamepad(Gamepad pad)
    {
        gamepad = pad;
    }
}

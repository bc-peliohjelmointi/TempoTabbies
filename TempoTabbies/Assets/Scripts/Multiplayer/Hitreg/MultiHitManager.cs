using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MultiHitManager : MonoBehaviour
{
    [Header("Player 1 GameObjects assigned to buttons")]
    public GameObject leftTriggerObject_P1;
    public GameObject rightTriggerObject_P1;
    public GameObject leftBumperObject_P1;
    public GameObject rightBumperObject_P1;
    public GameObject leftStickObject_P1;
    public GameObject rightStickObject_P1;

    [Header("Player 2 GameObjects assigned to buttons")]
    public GameObject leftTriggerObject_P2;
    public GameObject rightTriggerObject_P2;
    public GameObject leftBumperObject_P2;
    public GameObject rightBumperObject_P2;
    public GameObject leftStickObject_P2;
    public GameObject rightStickObject_P2;

    [Header("Score Management")]
    public ScoreManager scoreManager_P1;
    public ScoreManager scoreManager_P2;

    [Header("Rhythm Game References")]
    public AudioSource Music;
    public MultiNoteSpawner Spawner;
    public Transform HitLine_P1;
    public Transform HitLine_P2;

    [Header("Judgment Display Reference")]
    public JudgmentDisplay JudgmentDisplay_P1;
    public JudgmentDisplay JudgmentDisplay_P2;

    [Header("Lane Mapping - Player 1")]
    public int leftTriggerLane_P1 = 0;
    public int leftBumperLane_P1 = 1;
    public int rightBumperLane_P1 = 2;
    public int rightTriggerLane_P1 = 3;
    public int leftStickLane_P1 = 4;
    public int rightStickLane_P1 = 5;

    [Header("Lane Mapping - Player 2")]
    public int leftTriggerLane_P2 = 0;
    public int leftBumperLane_P2 = 1;
    public int rightBumperLane_P2 = 2;
    public int rightTriggerLane_P2 = 3;
    public int leftStickLane_P2 = 4;
    public int rightStickLane_P2 = 5;

    private bool stickLeftInitialPress_P1 = false;
    private bool stickRightInitialPress_P1 = false;
    private bool stickLeftInitialPress_P2 = false;
    private bool stickRightInitialPress_P2 = false;

    private Gamepad gamepad_P1;
    private Gamepad gamepad_P2;
    private readonly Dictionary<int, HoldNote> activeHolds_P1 = new();
    private readonly Dictionary<int, HoldNote> activeHolds_P2 = new();

    void Update()
    {
        // Get both gamepads (you might want to use PlayerInput instead for proper device assignment)
        var gamepads = Gamepad.all;
        if (gamepads.Count >= 1) gamepad_P1 = gamepads[0];
        if (gamepads.Count >= 2) gamepad_P2 = gamepads[1];

        // Update input visuals for both players
        UpdateInputVisuals();

        if (Music == null || Spawner == null)
            return;

        float songTime = GameManager.SongTime;

        // Handle input for both players
        HandlePlayerInput(0, gamepad_P1, activeHolds_P1, songTime);
        HandlePlayerInput(1, gamepad_P2, activeHolds_P2, songTime);

        // Update holds for both players
        UpdateHolds(activeHolds_P1, songTime);
        UpdateHolds(activeHolds_P2, songTime);

        // Detect misses for both players
        CheckForMisses(songTime);
    }

    private void UpdateInputVisuals()
    {
        // Player 1 visuals
        if (leftTriggerObject_P1 != null && gamepad_P1 != null)
            leftTriggerObject_P1.SetActive(gamepad_P1.leftTrigger.isPressed);
        if (rightTriggerObject_P1 != null && gamepad_P1 != null)
            rightTriggerObject_P1.SetActive(gamepad_P1.rightTrigger.isPressed);
        if (leftBumperObject_P1 != null && gamepad_P1 != null)
            leftBumperObject_P1.SetActive(gamepad_P1.leftShoulder.isPressed);
        if (rightBumperObject_P1 != null && gamepad_P1 != null)
            rightBumperObject_P1.SetActive(gamepad_P1.rightShoulder.isPressed);

        // Player 2 visuals
        if (leftTriggerObject_P2 != null && gamepad_P2 != null)
            leftTriggerObject_P2.SetActive(gamepad_P2.leftTrigger.isPressed);
        if (rightTriggerObject_P2 != null && gamepad_P2 != null)
            rightTriggerObject_P2.SetActive(gamepad_P2.rightTrigger.isPressed);
        if (leftBumperObject_P2 != null && gamepad_P2 != null)
            leftBumperObject_P2.SetActive(gamepad_P2.leftShoulder.isPressed);
        if (rightBumperObject_P2 != null && gamepad_P2 != null)
            rightBumperObject_P2.SetActive(gamepad_P2.rightShoulder.isPressed);

        // Stick visuals for both players
        UpdateStickVisuals();
    }

    private void UpdateStickVisuals()
    {
        bool stickLeftHeld_P1 = false, stickRightHeld_P1 = false;
        bool stickLeftHeld_P2 = false, stickRightHeld_P2 = false;

        if (gamepad_P1 != null)
        {
            stickLeftHeld_P1 = gamepad_P1.leftStick.ReadValue().x < -0.5f || gamepad_P1.rightStick.ReadValue().x < -0.5f;
            stickRightHeld_P1 = gamepad_P1.leftStick.ReadValue().x > 0.5f || gamepad_P1.rightStick.ReadValue().x > 0.5f;
        }

        if (gamepad_P2 != null)
        {
            stickLeftHeld_P2 = gamepad_P2.leftStick.ReadValue().x < -0.5f || gamepad_P2.rightStick.ReadValue().x < -0.5f;
            stickRightHeld_P2 = gamepad_P2.leftStick.ReadValue().x > 0.5f || gamepad_P2.rightStick.ReadValue().x > 0.5f;
        }

        if (leftStickObject_P1 != null) leftStickObject_P1.SetActive(stickLeftHeld_P1);
        if (rightStickObject_P1 != null) rightStickObject_P1.SetActive(stickRightHeld_P1);
        if (leftStickObject_P2 != null) leftStickObject_P2.SetActive(stickLeftHeld_P2);
        if (rightStickObject_P2 != null) rightStickObject_P2.SetActive(stickRightHeld_P2);
    }

    private void HandlePlayerInput(int playerIndex, Gamepad gamepad, Dictionary<int, HoldNote> activeHolds, float songTime)
    {
        if (gamepad == null) return;

        bool stickLeftPressed = false, stickRightPressed = false;

        // Determine stick press states
        if (playerIndex == 0)
        {
            bool stickLeftHeld = gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f;
            bool stickRightHeld = gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f;
            stickLeftPressed = stickLeftHeld && !stickLeftInitialPress_P1;
            stickRightPressed = stickRightHeld && !stickRightInitialPress_P1;
            stickLeftInitialPress_P1 = stickLeftHeld;
            stickRightInitialPress_P1 = stickRightHeld;
        }
        else
        {
            bool stickLeftHeld = gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f;
            bool stickRightHeld = gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f;
            stickLeftPressed = stickLeftHeld && !stickLeftInitialPress_P2;
            stickRightPressed = stickRightHeld && !stickRightInitialPress_P2;
            stickLeftInitialPress_P2 = stickLeftHeld;
            stickRightInitialPress_P2 = stickRightHeld;
        }

        // Get lane mapping for this player
        int leftTriggerLane = playerIndex == 0 ? leftTriggerLane_P1 : leftTriggerLane_P2;
        int leftBumperLane = playerIndex == 0 ? leftBumperLane_P1 : leftBumperLane_P2;
        int rightBumperLane = playerIndex == 0 ? rightBumperLane_P1 : rightBumperLane_P2;
        int rightTriggerLane = playerIndex == 0 ? rightTriggerLane_P1 : rightTriggerLane_P2;
        int leftStickLane = playerIndex == 0 ? leftStickLane_P1 : leftStickLane_P2;
        int rightStickLane = playerIndex == 0 ? rightStickLane_P1 : rightStickLane_P2;

        // Tap / hold start detection
        if (gamepad.leftTrigger.wasPressedThisFrame) TryHit(playerIndex, leftTriggerLane, songTime);
        if (gamepad.leftShoulder.wasPressedThisFrame) TryHit(playerIndex, leftBumperLane, songTime);
        if (gamepad.rightShoulder.wasPressedThisFrame) TryHit(playerIndex, rightBumperLane, songTime);
        if (gamepad.rightTrigger.wasPressedThisFrame) TryHit(playerIndex, rightTriggerLane, songTime);
        if (stickLeftPressed) TryHit(playerIndex, leftStickLane, songTime);
        if (stickRightPressed) TryHit(playerIndex, rightStickLane, songTime);
    }

    private void TryHit(int playerIndex, int lane, float currentTime)
    {
        Note closestNote = null;
        HoldNote holdHead = null;
        float smallestDiff = float.MaxValue;

        foreach (Transform child in Spawner.transform)
        {
            if (child == null) continue;

            // Only check notes for this player
            if (child.name.Contains($"Player{playerIndex + 1}"))
            {
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
        }

        if (closestNote != null)
            EvaluateHit(playerIndex, closestNote, currentTime);
        else if (holdHead != null)
            TryStartHold(playerIndex, holdHead, lane, currentTime);
    }

    private void EvaluateHit(int playerIndex, Note note, float currentTime)
    {
        float diff = currentTime - note.TargetTime;
        float absDiff = Mathf.Abs(diff);

        Debug.Log($"[Hit Timing] Player {playerIndex + 1} - Note: {note.TargetTime}, Current: {currentTime}, Diff: {diff}");

        string label;

        if (absDiff <= TimingWindows.Marvelous) label = "MARVELOUS";
        else if (absDiff <= TimingWindows.Perfect) label = "PERFECT";
        else if (absDiff <= TimingWindows.Great) label = "GREAT";
        else if (absDiff <= TimingWindows.Good) label = "GOOD";
        else if (absDiff <= TimingWindows.Bad) label = "BAD";
        else return;

        note.Hit = true;
        Destroy(note.gameObject);
        ShowJudgment(playerIndex, label);

        // ADD SCORE to appropriate player
        ScoreManager scoreManager = playerIndex == 0 ? scoreManager_P1 : scoreManager_P2;
        if (scoreManager != null)
        {
            scoreManager.AddJudgment(label);
        }

        Debug.Log($"[Player {playerIndex + 1} {label}] lane {note.Lane} ?t={diff * 1000f:F1}ms");
    }

    private void TryStartHold(int playerIndex, HoldNote hold, int lane, float currentTime)
    {
        float diff = Mathf.Abs(hold.StartTime - currentTime);
        var activeHolds = playerIndex == 0 ? activeHolds_P1 : activeHolds_P2;

        if (diff <= TimingWindows.Great && !activeHolds.ContainsKey(lane))
        {
            activeHolds[lane] = hold;
            ShowJudgment(playerIndex, "MARVELOUS");
            Debug.Log($"[Player {playerIndex + 1} HOLD START] lane {lane}");
        }
    }

    private void UpdateHolds(Dictionary<int, HoldNote> activeHolds, float songTime)
    {
        List<int> toRelease = new();

        foreach (var kv in activeHolds)
        {
            int lane = kv.Key;
            HoldNote hold = kv.Value;

            bool stillHeld = IsLanePressed(lane, activeHolds == activeHolds_P1 ? 0 : 1);
            if (!stillHeld || songTime > hold.EndTime)
                toRelease.Add(lane);
        }

        foreach (int lane in toRelease)
        {
            activeHolds.Remove(lane);
            Debug.Log($"[HOLD RELEASE] lane {lane}");
        }
    }

    private bool IsLanePressed(int lane, int playerIndex)
    {
        Gamepad gamepad = playerIndex == 0 ? gamepad_P1 : gamepad_P2;
        if (gamepad == null) return false;

        int leftTriggerLane = playerIndex == 0 ? leftTriggerLane_P1 : leftTriggerLane_P2;
        int rightTriggerLane = playerIndex == 0 ? rightTriggerLane_P1 : rightTriggerLane_P2;
        int leftBumperLane = playerIndex == 0 ? leftBumperLane_P1 : leftBumperLane_P2;
        int rightBumperLane = playerIndex == 0 ? rightBumperLane_P1 : rightBumperLane_P2;
        int leftStickLane = playerIndex == 0 ? leftStickLane_P1 : leftStickLane_P2;
        int rightStickLane = playerIndex == 0 ? rightStickLane_P1 : rightStickLane_P2;

        if (lane == leftTriggerLane) return gamepad.leftTrigger.isPressed;
        if (lane == rightTriggerLane) return gamepad.rightTrigger.isPressed;
        if (lane == leftBumperLane) return gamepad.leftShoulder.isPressed;
        if (lane == rightBumperLane) return gamepad.rightShoulder.isPressed;
        if (lane == leftStickLane)
            return gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f;
        if (lane == rightStickLane)
            return gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f;

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

                // Determine which player this note belongs to and show appropriate judgment
                int playerIndex = child.name.Contains("Player2") ? 1 : 0;
                ShowJudgment(playerIndex, "MISS");

                // ADD MISS TO SCORE for appropriate player
                ScoreManager scoreManager = playerIndex == 0 ? scoreManager_P1 : scoreManager_P2;
                if (scoreManager != null)
                {
                    scoreManager.AddJudgment("MISS");
                }

                Destroy(note.gameObject);
                Debug.Log($"[Player {playerIndex + 1} MISS] lane {note.Lane}");
            }
        }
    }

    public void InitializeChart(int totalNotes)
    {
        if (scoreManager_P1 != null) scoreManager_P1.InitializeScore(totalNotes);
        if (scoreManager_P2 != null) scoreManager_P2.InitializeScore(totalNotes);
    }

    private void ShowJudgment(int playerIndex, string label)
    {
        JudgmentDisplay display = playerIndex == 0 ? JudgmentDisplay_P1 : JudgmentDisplay_P2;
        if (display != null)
            display.Show(label);
    }
}
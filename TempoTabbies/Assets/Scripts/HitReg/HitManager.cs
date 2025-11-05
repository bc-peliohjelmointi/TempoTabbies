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

    [Header("Rhythm Game References")]
    public AudioSource Music;
    public NoteSpawner Spawner;
    public Transform HitLine;

    [Header("Judgment Prefabs")]
    public GameObject MarvelousPrefab;
    public GameObject PerfectPrefab;
    public GameObject GreatPrefab;
    public GameObject GoodPrefab;
    public GameObject BadPrefab;
    public GameObject MissPrefab;

    [Header("Lane Mapping (controller ? chart lane)")]
    public int leftTriggerLane = 0;
    public int leftBumperLane = 1;
    public int rightBumperLane = 2;
    public int rightTriggerLane = 3;
    public int leftStickLane = 4;
    public int rightStickLane = 5;

    private Gamepad gamepad;
    private readonly Dictionary<int, HoldNote> activeHolds = new();

    void Update()
    {
        gamepad = Gamepad.current;
        if (gamepad == null) return;

        // --- Input visuals (unchanged) ---
        if (leftTriggerObject != null)
            leftTriggerObject.SetActive(gamepad.leftTrigger.isPressed);
        if (rightTriggerObject != null)
            rightTriggerObject.SetActive(gamepad.rightTrigger.isPressed);
        if (leftBumperObject != null)
            leftBumperObject.SetActive(gamepad.leftShoulder.isPressed);
        if (rightBumperObject != null)
            rightBumperObject.SetActive(gamepad.rightShoulder.isPressed);

        bool anyStickLeft = gamepad.leftStick.ReadValue().x < -0.5f ||
                            gamepad.rightStick.ReadValue().x < -0.5f;
        bool anyStickRight = gamepad.leftStick.ReadValue().x > 0.5f ||
                             gamepad.rightStick.ReadValue().x > 0.5f;

        if (leftStickObject != null)
            leftStickObject.SetActive(anyStickLeft);
        if (rightStickObject != null)
            rightStickObject.SetActive(anyStickRight);

        if (Music == null || Spawner == null)
            return;

        float songTime = Music.time;

        // Tap / hold start detection
        if (gamepad.leftTrigger.wasPressedThisFrame) TryHit(leftTriggerLane, songTime);
        if (gamepad.leftShoulder.wasPressedThisFrame) TryHit(leftBumperLane, songTime);
        if (gamepad.rightShoulder.wasPressedThisFrame) TryHit(rightBumperLane, songTime);
        if (gamepad.rightTrigger.wasPressedThisFrame) TryHit(rightTriggerLane, songTime);
        if (anyStickLeft) TryHit(leftStickLane, songTime);
        if (anyStickRight) TryHit(rightStickLane, songTime);

        // Update hold tracking
        UpdateHolds(songTime);

        // Detect misses
        CheckForMisses(songTime);
    }

    // -----------------------------------------------------

    private void TryHit(int lane, float currentTime)
    {
        Note closestNote = null;
        HoldNote holdHead = null;
        float smallestDiff = float.MaxValue;

        foreach (Transform child in Spawner.transform)
        {
            if (child == null) continue;

            // Tap note
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

            // Hold note start
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
        {
            EvaluateHit(closestNote, currentTime);
        }
        else if (holdHead != null)
        {
            TryStartHold(holdHead, lane, currentTime);
        }
    }

    private void EvaluateHit(Note note, float currentTime)
    {
        float diff = currentTime - note.TargetTime;
        float absDiff = Mathf.Abs(diff);
        GameObject prefab = null;
        string label;

        if (absDiff <= TimingWindows.Marvelous) { prefab = MarvelousPrefab; label = "MARVELOUS"; }
        else if (absDiff <= TimingWindows.Perfect) { prefab = PerfectPrefab; label = "PERFECT"; }
        else if (absDiff <= TimingWindows.Great) { prefab = GreatPrefab; label = "GREAT"; }
        else if (absDiff <= TimingWindows.Good) { prefab = GoodPrefab; label = "GOOD"; }
        else if (absDiff <= TimingWindows.Bad) { prefab = BadPrefab; label = "BAD"; }
        else return;

        note.Hit = true;
        Destroy(note.gameObject);
        SpawnJudgment(prefab);
        Debug.Log($"[{label}] lane {note.Lane} ?t={diff * 1000f:F1}ms");
    }

    private void TryStartHold(HoldNote hold, int lane, float currentTime)
    {
        float diff = Mathf.Abs(hold.StartTime - currentTime);
        if (diff <= TimingWindows.Great && !activeHolds.ContainsKey(lane))
        {
            activeHolds[lane] = hold;
            SpawnJudgment(MarvelousPrefab);
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
        if (gamepad == null) return false;

        if (lane == leftTriggerLane) return gamepad.leftTrigger.isPressed;
        if (lane == rightTriggerLane) return gamepad.rightTrigger.isPressed;
        if (lane == leftBumperLane) return gamepad.leftShoulder.isPressed;
        if (lane == rightBumperLane) return gamepad.rightShoulder.isPressed;
        if (lane == leftStickLane)
            return gamepad.leftStick.ReadValue().x < -0.5f ||
                   gamepad.rightStick.ReadValue().x < -0.5f;
        if (lane == rightStickLane)
            return gamepad.leftStick.ReadValue().x > 0.5f ||
                   gamepad.rightStick.ReadValue().x > 0.5f;

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
                SpawnJudgment(MissPrefab);
                Destroy(note.gameObject);
                Debug.Log($"[MISS] lane {note.Lane}");
            }
        }
    }

    private void SpawnJudgment(GameObject prefab)
    {
        if (prefab == null || HitLine == null) return;
        GameObject obj = Instantiate(prefab, HitLine.position, Quaternion.identity);
        Destroy(obj, 0.5f);
    }
}

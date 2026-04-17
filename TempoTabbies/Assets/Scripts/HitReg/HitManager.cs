using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;
using JetBrains.Annotations;

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

    // expose assigned gamepad to HoldNote
    public Gamepad AssignedGamepad => assignedGamepad;

    // Previous-frame gamepad-held states (used to detect press-this-frame reliably)
    private bool prevGamepadLeftTriggerHeld = false;
    private bool prevGamepadRightTriggerHeld = false;
    private bool prevGamepadLeftShoulderHeld = false;
    private bool prevGamepadRightShoulderHeld = false;
    private bool prevGamepadStickLeftHeld = false;
    private bool prevGamepadStickRightHeld = false;

    // Helper keyboard checks to accept multiple common keys as fallbacks
    private static bool AnyKeyPressed(Keyboard kb, params KeyControl[] keys)
    {
        if (kb == null) return false;
        foreach (var k in keys)
        {
            if (k != null && k.isPressed) return true;
        }
        return false;
    }

    private static bool AnyKeyPressedThisFrame(Keyboard kb, params KeyControl[] keys)
    {
        if (kb == null) return false;
        foreach (var k in keys)
        {
            if (k != null && k.wasPressedThisFrame) return true;
        }
        return false;
    }

    [Header("player button changes")]
    public int playerNumber;
    public ButtonControl button1;
    public ButtonControl button2;
    public ButtonControl button3;
    public ButtonControl button4;

    private _GameManager _gm;

    private void Awake()
    {
        _gm = FindFirstObjectByType<_GameManager>();
        if (playerNumber == 1)
        {
            assignedGamepad = ConvertToGamepad(_gm.p1.inputDevice);
            if (_gm.p1.button1 == null)
            {
                _gm.p1.SetDefaultButtons();
            }
            button1 = _gm.p1.button1;
            button2 = _gm.p1.button2;
            button3 = _gm.p1.button3;
            button4 = _gm.p1.button4;
        }
        else if (playerNumber == 2)
        {
            assignedGamepad = ConvertToGamepad(_gm.p2.inputDevice);
            if (_gm.p2.button1 == null)
            {
                _gm.p2.SetDefaultButtons();
            }
            button1 = _gm.p2.button1;
            button2 = _gm.p2.button2;
            button3 = _gm.p2.button3;
            button4 = _gm.p2.button4;
        }

        // Auto-assign a per-player SimpleHitSprite if none was set in the inspector.
        if (hitEffectManager == null)
        {
            // Use the claiming API so multiple HitManagers don't get the same instance.
            var claimed = SimpleHitSprite.FindAndClaim(playerNumber);
            hitEffectManager = claimed;

            if (hitEffectManager == null)
                Debug.LogWarning($"HitManager(player {playerNumber}): No unclaimed SimpleHitSprite found in scene to assign.");
            else
                Debug.Log($"HitManager(player {playerNumber}): Assigned SimpleHitSprite '{hitEffectManager.name}' (player {hitEffectManager.playerNumber}).");
        }

        // Diagnostic: log assigned spawner and hitEffect manager for debug tracing
        Debug.Log($"HitManager.Awake: '{name}' player={playerNumber} Spawner={(Spawner!=null?Spawner.name:"null")} SpawnerID={(Spawner!=null?Spawner.GetInstanceID():0)} HitEffectManager={(hitEffectManager!=null?hitEffectManager.name:"null")}");
        // If the project isn't using MultiHitManager, ensure only one HitManager
        // accepts keyboard in multiplayer.
        EnsureKeyboardOwnership(_gm);
    }
    private Gamepad ConvertToGamepad(InputDevice device)
    {
        if (device == null) return null;

        if (device is Gamepad gp) return gp;

        foreach (var g in Gamepad.all)
        {
            if (g.deviceId == device.deviceId)
                return g;
        }

        return null;
    }


    private static void EnsureKeyboardOwnership(_GameManager gm)
    {
        if (gm == null) return;
        if (!gm.multiplayer) return;
       

        var all = FindObjectsOfType<HitManager>();
        if (all == null || all.Length == 0) return;

        // Find hitmanagers for player 1 and 2 (if present)
        var hm1 = all.FirstOrDefault(h => h.playerNumber == 1);
        var hm2 = all.FirstOrDefault(h => h.playerNumber == 2);

        // Helper to detect whether a player (via GameManager) has a gamepad device
        bool PlayerHasPad(int playerNum)
        {
            var p = playerNum == 1 ? gm.p1 : gm.p2;
            if (p == null) return false;
            if (p.inputDevice is Gamepad) return true;
            // try matching by device id
            if (p.inputDevice != null)
            {
                foreach (var g in Gamepad.all)
                {
                    if (g.deviceId == p.inputDevice.deviceId) return true;
                }
            }
            return false;
        }

        bool p1HasPad = PlayerHasPad(1);
        bool p2HasPad = PlayerHasPad(2);

        // Default: disable keyboard on all HitManagers
        foreach (var h in all) h.AcceptKeyboard = false;

        if (hm1 != null && hm2 != null)
        {
            if (!p1HasPad && p2HasPad)
            {
                hm1.AcceptKeyboard = true;
            }
            else if (p1HasPad && !p2HasPad)
            {
                hm2.AcceptKeyboard = true;
            }
            else if (!p1HasPad && !p2HasPad)
            {
                // both lack gamepads -> prefer player 1
                hm1.AcceptKeyboard = true;
            }
            else
            {
                // both have pads -> nobody uses keyboard
            }
        }
        else if (hm1 != null)
        {
            if (!p1HasPad) hm1.AcceptKeyboard = true;
        }
        else if (hm2 != null)
        {
            if (!p2HasPad) hm2.AcceptKeyboard = true;
        }
    }

    void Update()
    {
        Debug.Log($"HitManager(player {playerNumber}) buttons {button1?.name} {button2?.name} {button3?.name} {button4?.name} assignedGamepad={(assignedGamepad!=null?assignedGamepad.displayName:"null")} Gamepad.current={(Gamepad.current!=null?Gamepad.current.displayName:"null")} Gamepad.count={Gamepad.all.Count} AcceptKeyboard={AcceptKeyboard}");

        // Try to (re)bind assigned gamepad / per-player button controls at runtime.
        
        if (_gm != null)
        {
            var ps = playerNumber == 1 ? _gm.p1 : _gm.p2;
            if (ps != null)
            {
                Debug.Log($"HitManager(player {playerNumber}) PlayerScript present: playerName={ps.name} inputDevice={(ps.inputDevice!=null?ps.inputDevice.displayName:"null")}");
                // Attempt to bind assignedGamepad from the PlayerScript device
                if (assignedGamepad == null && ps.inputDevice != null)
                {
                    var tryPad = ConvertToGamepad(ps.inputDevice);
                    if (tryPad != null)
                    {
                        assignedGamepad = tryPad;
                        Debug.Log($"HitManager(player {playerNumber}): bound assignedGamepad to {assignedGamepad.displayName}");
                    }
                }
                    // If still no assignedGamepad, try per-player index from Gamepad.all
                    if (assignedGamepad == null && Gamepad.all.Count >= playerNumber && playerNumber > 0)
                    {
                        assignedGamepad = Gamepad.all[playerNumber - 1];
                        Debug.Log($"HitManager(player {playerNumber}): fallback bound assignedGamepad to {assignedGamepad.displayName} by index");
                    }

                // Ensure per-player ButtonControl bindings exist; SetDefaultButtons is safe to call repeatedly.
                if (ps.button1 == null || ps.button2 == null || ps.button3 == null || ps.button4 == null)
                {
                    ps.SetDefaultButtons();
                    Debug.Log($"HitManager(player {playerNumber}): SetDefaultButtons called, buttons: {ps.button1?.name} {ps.button2?.name} {ps.button3?.name} {ps.button4?.name}");
                }

                // Update our cached ButtonControls from the player script
                button1 ??= ps.button1;
                button2 ??= ps.button2;
                button3 ??= ps.button3;
                button4 ??= ps.button4;
                Debug.Log($"HitManager(player {playerNumber}) cached buttons: {button1?.name} {button2?.name} {button3?.name} {button4?.name}");
            }
        }

        // Decide whether we allow falling back to any connected gamepad (singleplayer only)
        bool singleplayerAllowAnyGamepad = _gm == null || !_gm.multiplayer;

        
        gamepad = assignedGamepad ?? (singleplayerAllowAnyGamepad ? Gamepad.current : null);

        
        // already coordinates ownership in multiplayer, so a simple flag check is sufficient.
        var keyboard = AcceptKeyboard ? Keyboard.current : null;

        bool AnyGamepadHas(System.Func<Gamepad, bool> pred)
        {
            if (!singleplayerAllowAnyGamepad) return false;
            foreach (var gp in Gamepad.all)
            {
                if (gp != null && pred(gp)) return true;
            }
            return false;
        }

        
        // s -> lane 0, d -> lane 1, k -> lane 2, l -> lane 3
        // space -> left swipe (lane 4), rightAlt -> right swipe (lane 5)
        bool curLeftTriggerHeld = (button1 != null && button1.isPressed)
                                  || (keyboard != null && keyboard.sKey.isPressed);

        bool curLeftBumperHeld = (button2 != null && button2.isPressed)
                                 || (keyboard != null && keyboard.dKey.isPressed);

        bool curRightBumperHeld = (button3 != null && button3.isPressed)
                                  || (keyboard != null && keyboard.kKey.isPressed);

        bool curRightTriggerHeld = (button4 != null && button4.isPressed)
                                   || (keyboard != null && keyboard.lKey.isPressed);

        bool curStickLeftHeld = (gamepad != null && (gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f))
                                || AnyGamepadHas(gp => gp.leftStick.ReadValue().x < -0.5f || gp.rightStick.ReadValue().x < -0.5f)
                                || (keyboard != null && keyboard.spaceKey.isPressed);

        bool curStickRightHeld = (gamepad != null && (gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f))
                                 || AnyGamepadHas(gp => gp.leftStick.ReadValue().x > 0.5f || gp.rightStick.ReadValue().x > 0.5f)
                                 || (keyboard != null && keyboard.rightAltKey.isPressed);

        // Press-this-frame detection
        bool leftTriggerPressedThisFrame = (curLeftTriggerHeld && !prevGamepadLeftTriggerHeld) || (keyboard != null && keyboard.sKey.wasPressedThisFrame);
        bool leftBumperPressedThisFrame = (curLeftBumperHeld && !prevGamepadLeftShoulderHeld) || (keyboard != null && keyboard.dKey.wasPressedThisFrame);
        bool rightBumperPressedThisFrame = (curRightBumperHeld && !prevGamepadRightShoulderHeld) || (keyboard != null && keyboard.kKey.wasPressedThisFrame);
        bool rightTriggerPressedThisFrame = (curRightTriggerHeld && !prevGamepadRightTriggerHeld) || (keyboard != null && keyboard.lKey.wasPressedThisFrame);
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
        // Track previous held states for edge (press-this-frame) detection.
        prevGamepadLeftTriggerHeld = leftTriggerHeld;
        prevGamepadRightTriggerHeld = rightTriggerHeld;
        prevGamepadLeftShoulderHeld = leftBumperHeld;
        prevGamepadRightShoulderHeld = rightBumperHeld;
        prevGamepadStickLeftHeld = stickLeftHeld;
        prevGamepadStickRightHeld = stickRightHeld;
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
            // inform hold about start so it uses the correct HitManager input mapping
            hold.OwnerHitManager = this;
            hold.StartHoldFromHitManager(currentTime);

            // Play hit effect for hold start, but DO NOT show a hard-coded judgment here.
            if (hitEffectManager != null)
                hitEffectManager.PlayHitEffect(lane, "DEFAULT");

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
            // Do NOT call into the HoldNote here. HoldNote manages its own release
            // timing and scoring in its Update. Calling its release method from
            // HitManager caused double-handling and MissingReferenceExceptions.
            activeHolds.Remove(lane);
            Debug.Log($"[HOLD RELEASE] lane {lane}");
        }
    }

    private bool IsLanePressed(int lane)
    {

        var keyboard = AcceptKeyboard ? Keyboard.current : null;

        if (lane == leftTriggerLane)
            return (gamepad != null && button1 != null && button1.isPressed) || (keyboard != null && keyboard.sKey.isPressed);
        if (lane == rightTriggerLane)
            return (gamepad != null && button4 != null && button4.isPressed) || (keyboard != null && keyboard.lKey.isPressed);
        if (lane == leftBumperLane)
            return (gamepad != null && button2 != null && button2.isPressed) || (keyboard != null && keyboard.dKey.isPressed);
        if (lane == rightBumperLane)
            return (gamepad != null && button3 != null && button3.isPressed) || (keyboard != null && keyboard.kKey.isPressed);
        if (lane == leftStickLane)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x < -0.5f || gamepad.rightStick.ReadValue().x < -0.5f)) || (keyboard != null && keyboard.spaceKey.isPressed);
        if (lane == rightStickLane)
            return (gamepad != null && (gamepad.leftStick.ReadValue().x > 0.5f || gamepad.rightStick.ReadValue().x > 0.5f)) || (keyboard != null && keyboard.rightAltKey.isPressed);

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
                if (scoreManager.eightLives)
                {
                    ShowJudgment("MARVELOUS", false, false); // Show BAD instead of MISS when 8 Lives is active
                }
                else
                {
                    ShowJudgment("MISS", false, false); // No direction for misses
                }

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

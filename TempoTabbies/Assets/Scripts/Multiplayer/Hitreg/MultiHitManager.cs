using UnityEngine;
using UnityEngine.InputSystem;

public class MultiHitManager : MonoBehaviour
{
    [Header("Player 1 Components")]
    public HitManager hitManager_P1;

    [Header("Player 2 Components")]
    public HitManager hitManager_P2;

    private Gamepad gamepad_P1;
    private Gamepad gamepad_P2;

    void Update()
    {
        // Prefer PlayerScript devices from the central _GameManager instance
        var gm = _GameManager.instance;
        PlayerScript p1 = gm != null ? gm.p1 : null;
        PlayerScript p2 = gm != null ? gm.p2 : null;

        if (p1 != null)
            gamepad_P1 = ConvertToGamepad(p1.inputDevice);
        else
            gamepad_P1 = null;

        if (p2 != null)
            gamepad_P2 = ConvertToGamepad(p2.inputDevice);
        else
            gamepad_P2 = null;

        // Fallback to Gamepad.all if PlayerScript didn't provide a Gamepad yet
        var gamepads = Gamepad.all;
        if (gamepad_P1 == null && gamepads.Count >= 1) gamepad_P1 = gamepads[0];
        if (gamepad_P2 == null && gamepads.Count >= 2) gamepad_P2 = gamepads[1];

        // Pass the correct gamepad to each HitManager
        if (hitManager_P1 != null)
            hitManager_P1.SetGamepad(gamepad_P1);

        if (hitManager_P2 != null)
            hitManager_P2.SetGamepad(gamepad_P2);
    }

    // Safe converter: cast if already a Gamepad, otherwise match by deviceId
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
}
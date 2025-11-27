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
        // Get both gamepads
        var gamepads = Gamepad.all;
        if (gamepads.Count >= 1) gamepad_P1 = gamepads[0];
        if (gamepads.Count >= 2) gamepad_P2 = gamepads[1];

        // Pass the correct gamepad to each HitManager
        // Each HitManager will handle its own input visuals and hit detection
        if (hitManager_P1 != null)
        {
            hitManager_P1.SetGamepad(gamepad_P1);
        }

        if (hitManager_P2 != null)
        {
            hitManager_P2.SetGamepad(gamepad_P2);
        }
    }
}
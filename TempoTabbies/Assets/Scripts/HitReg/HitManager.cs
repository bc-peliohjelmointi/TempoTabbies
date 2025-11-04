using UnityEngine;
using UnityEngine.InputSystem;

public class HitManager : MonoBehaviour
{
    [Header("GameObjects assigned to buttons")]
    public GameObject leftTriggerObject;
    public GameObject rightTriggerObject;
    public GameObject leftBumperObject;
    public GameObject rightBumperObject;
    public GameObject leftStickObject;
    public GameObject rightStickObject;

    private Gamepad gamepad;

    void Update()
    {
        gamepad = Gamepad.current;
        if (gamepad == null)
            return;

        // LT
        if (leftTriggerObject != null)
            leftTriggerObject.SetActive(gamepad.leftTrigger.isPressed);

        // RT
        if (rightTriggerObject != null)
            rightTriggerObject.SetActive(gamepad.rightTrigger.isPressed);

        // LB
        if (leftBumperObject != null)
            leftBumperObject.SetActive(gamepad.leftShoulder.isPressed);

        // RB
        if (rightBumperObject != null)
            rightBumperObject.SetActive(gamepad.rightShoulder.isPressed);

        // Left Stick Click
        if (leftStickObject != null)
            leftStickObject.SetActive(gamepad.leftStickButton.isPressed);

        // Right Stick Click
        if (rightStickObject != null)
            rightStickObject.SetActive(gamepad.rightStickButton.isPressed);
    }
}

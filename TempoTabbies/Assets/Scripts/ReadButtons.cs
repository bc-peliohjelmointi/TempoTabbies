using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ReadButtons : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI textController;
    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        foreach (var key in keyboard.allKeys)
        {
            if (key == null) continue;

            if (key.wasPressedThisFrame)
            {
                text.text = key.displayName;
                Debug.Log(key);
            }
        }


        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        foreach (var control in gamepad.allControls)
        {
            if (control is ButtonControl button && button.wasPressedThisFrame)
            {
                Debug.Log(control);
                Debug.Log("display" + control.displayName);
                Debug.Log(".name" + control.name);
                textController.text = control.name;
            }
        }
    }
}

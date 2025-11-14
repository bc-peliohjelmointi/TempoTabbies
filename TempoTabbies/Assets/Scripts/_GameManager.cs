using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// General game manager
/// </summary>
public class _GameManager : MonoBehaviour
{
    public static _GameManager instance;

    // A spot to remember what stage is currently selected
    public int stageID;

    public int whoGetsToPlay; // When 0, only player 1 gets to do stuff in menus, when 1, only player 2 gets to do stuff in menus

    // Setting values to remember
    public float volume;
    public float scrollSpeed;
    public float stickSensitivity;
    public float audioOffset;
    public bool assistTick;
    public float assistTickVolume;
    public bool hitSound;
    public float hitSoundVolume;
    // public ??? noteColor;

    public enum GameState
    {
        MainMenu, // The main menu
        Options, // The options menu
        StageSelect, // The song select screen
        Game, // The songs being played
        CardSelection, // Selecting cards mid game
        Pause // Pausing mid game
    }
    public GameState state;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Turns on all the controllers (keyboards, gamepads etc.)
    public void EnableControllers()
    {
        // Every controller
        var allControllers = InputSystem.devices;
        // Checks each controller to turn them all on
        for (int i = 0; i < allControllers.Count; i++)
        {
            // if a controller is disabled, enable it
            if (!allControllers[i].enabled)
            {
                InputSystem.EnableDevice(allControllers[i]);
            }
        }
    }
}

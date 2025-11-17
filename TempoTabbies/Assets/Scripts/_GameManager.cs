using System.Collections.Generic;
using System.Linq;
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

    // The players, first a script to find them all, then the 2 players individually
    public List<PlayerScript> players;
    public PlayerScript p1;
    public PlayerScript p2;

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
        else if (instance != null && instance != this)
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

    // when players are loaded in, use this to add them to a list
    public void FindPlayers()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.InstanceID).ToList();
        if (players.Count > 0)
        {
            p1 = players[0];
            if (players.Count > 1)
            {
                p2 = players[1];
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// General game manager
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class _GameManager : MonoBehaviour
{
    public static _GameManager instance;

    public int stageID;

    public int whoGetsToPlay; // When 0, only player 1 gets to do stuff in menus, when 1, only player 2 gets to do stuff in menus

    public bool multiplayer;

    // Setting values to remember
    [Header("Settings values to save")]
    public float volume;
    public bool showButtons;
    public float scrollSpeed;
    public float audioOffset; // in ms
    public bool assistTick;
    public float assistTickVolume;
    public bool hitSound;
    public float hitSoundVolume;
    public bool epilepsy;
    public bool movingBG;
    public string noteOne;
    public string noteTwo;

    // The players, first a list  to find them all, then the 2 players individually
    [Header("The players")]
    public List<PlayerScript> players;
    public PlayerScript p1;
    public PlayerScript p2;

    public int p1Score;
    public int p2Score;
    public bool whoWon; // true = p1 wins, false = p2 wins 

    public bool party;
    public bool crazy;
    public bool taco;

    // Audio
    [Header("Audio file for background music")]
    public AudioSource source;

    private JSON_Stuff json;

    public enum GameState
    {
        Start, // Starting section where p1 is created
        MainMenu, // The main menu
        Options, // The options menu
        Profiles, // The menu you make personal settings in
        StageSelect, // The song select screen
        Player2Confirmation, // confirms player 2s existence
        PlayerSelect, // Selects player profiles
        CatSelect, // Selecting a cat
        Practise, // The singleplayer practice mode
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
        ScoreDatabase.Initialize();
        json = FindFirstObjectByType<JSON_Stuff>();
        source = GetComponent<AudioSource>();
        source.Play();
        source.loop = true;
    }

    private void Update()
    {
        // Checks if players are null when it can, to find players
        if (p1 == null || p2 == null)
        {
            FindPlayers();
        }

        if (state == GameState.Game || state == GameState.StageSelect)
        {
            source.Stop();
        }
        else
        {
            if (!source.isPlaying)
            {
                source.Play();
            }
        }
        if (multiplayer)
        {
            if (p1Score == 3)
            {
                p1Score = 1;
                p2Score = 0;
            }
            if (p2Score == 3)
            {
                p2Score = 1;
                p1Score = 0;
            }
        }
    }

    // When players are loaded in, use this to add them to a list that other scripts can see
    public void FindPlayers()
    {
        players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.InstanceID).ToList();
        if (players.Count > 0 && p1 == null)
        {
            p1 = players[0];
        }
        if (multiplayer == true && players.Count > 1 && p2 == null)
        {
            // This works, since when you add a 2nd player, it goes to slot 0, even though when player 1 is added, it also goes to slot 0
            p2 = players[0];
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

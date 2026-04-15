using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerScript : MonoBehaviour
{
    public PlayerInput playerInput;

    // The players player number, 0 = player 1, 1 = player 2
    [Header("Who the player is")]
    public int _playerIndex;
    public InputDevice inputDevice;
    public Gamepad gamepad;

    // Other scripts
    private _GameManager gameManager;
    public JSON_Stuff json;
    private MainMenuManager mainMenu;
    private OptionsManager optionsMenu;
    private CatSelectionManager catMenu;

    // The needed inputs
    [Header("The inputs we're using")]
    public InputAction submit;
    public InputAction navigate;
    public InputAction clickButton;

    // Cards and Score
    [Header("In game info")]
    public List<CardDataScript> AllCards = new List<CardDataScript>();

    public int Score;
    public int Combo;
    public int cat; // use this to say which cat has been chosen for the player, currently 1 = tabby, 2 = orange, 3 = nothing

    // Stored values that are player specific
    [Header("Stuff we save")]
    public float scrollSpeed;
    public bool assistTick;
    public bool showButtons;
    public ButtonControl button1;
    public ButtonControl button2;
    public ButtonControl button3;
    public ButtonControl button4;

    private void Awake()
    {
        // Makes players stay alive between scenes
        DontDestroyOnLoad(gameObject);
    }

    public float Submit()
    {
        return submit.ReadValue<float>();
    }

    private void OnEnable()
    {
        // Everything we need to find
        gameManager = FindFirstObjectByType<_GameManager>();
        playerInput = GetComponent<PlayerInput>();
        _playerIndex = playerInput.playerIndex;
        navigate = playerInput.actions.FindAction("Navigate");
        submit = playerInput.actions.FindAction("Submit");
        clickButton = playerInput.actions.FindAction("ClickButton");
    }

    private void Update()
    {
       /*if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Destroy(gameObject);
        }*/
        if (AllCards.Count > 0 && gameManager.state != _GameManager.GameState.Game)
        {
            Debug.Log(gameManager.state);
            foreach (CardDataScript card in AllCards)
            {
                card.activeP1 = false;
                card.activeP2 = false;
            }
        }

        // Checks what state the game is currently in
        switch (gameManager.state)
        {
            case _GameManager.GameState.Start:
                break;

            case _GameManager.GameState.MainMenu:
                if (gameManager.state == _GameManager.GameState.MainMenu)
                {
                    // Gets the main menu script
                    if (mainMenu == null)
                    {
                        mainMenu = FindFirstObjectByType<MainMenuManager>();
                    }

                    // Checks the movement that we need for menus
                    mainMenu.moveAmount = navigate.ReadValue<Vector2>();
                    mainMenu.clickValue = clickButton.ReadValue<float>();
                }
                break;

            case _GameManager.GameState.Options:
                // gets the options menu script
                if (optionsMenu == null)
                {
                    optionsMenu = FindFirstObjectByType<OptionsManager>();
                }

                // Checks the movement that we need for menus
                optionsMenu.moveAmount = navigate.ReadValue<Vector2>();
                optionsMenu.clickValue = clickButton.ReadValue<float>();
                optionsMenu.submitValue = submit.ReadValue<float>();
                break;

            case _GameManager.GameState.Profiles:
                break;

            case _GameManager.GameState.PlayerSelect:
                break;

            case _GameManager.GameState.StageSelect:
                if (!gameManager.party)
                {
                    AllCards.Clear();
                }
                break;

            case _GameManager.GameState.Game:
                foreach (CardDataScript card in AllCards)
                {
                    if (_playerIndex == 0 && gameManager.multiplayer)
                    {
                        card.activeP1 = true;
                    }
                    else if (_playerIndex == 1)
                    {
                        card.activeP2 = true;
                    }
                    else if (!gameManager.multiplayer)
                    {
                        card.activeP1 = true;
                    }
                }
                break;

            case _GameManager.GameState.CatSelect:
                if (catMenu == null)
                {
                    catMenu = FindFirstObjectByType<CatSelectionManager>();
                }
                if (name == "Player(Clone)")
                {
                    json = FindFirstObjectByType<JSON_Stuff>();
                    json.LoadPlayerToPlayer("Default", _playerIndex);
                }
                if (_playerIndex == gameManager.whoGetsToPlay)
                {
                    catMenu.moveAmount = navigate.ReadValue<Vector2>();
                    catMenu.clickValue = clickButton.ReadValue<float>();
                }
                break;

            case _GameManager.GameState.CardSelection:
                break;
        }
        if (button1 != null)
        {
            Debug.Log($"player {_playerIndex} buttons {button1?.name} {button2?.name} {button3?.name} {button4?.name}");
        }

        var allControllers = InputSystem.devices;
        // THIS controller
        inputDevice = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;
    }

    public void SetDefaultButtons()
    {
        if (inputDevice == null)
        {
            Debug.LogWarning($"Player {_playerIndex + 1} has no input device assigned.");
            return;
        }
        // Assuming a standard gamepad layout, you can map buttons like this:
        button1 = inputDevice.TryGetChildControl<ButtonControl>("leftTrigger");
        button2 = inputDevice.TryGetChildControl<ButtonControl>("leftShouler");
        button3 = inputDevice.TryGetChildControl<ButtonControl>("rightShoulder");
        button4 = inputDevice.TryGetChildControl<ButtonControl>("rightTrigger");
        Debug.Log($"Player {_playerIndex + 1} buttons set: Button1={button1?.name}, Button2={button2?.name}, Button3={button3?.name}, Button4={button4?.name}");
    }

    public void SetNewButtons(string newButton1, string newButton2, string newButton3, string newButton4)
    {
        if (inputDevice == null)
        {
            Debug.LogWarning($"Player {_playerIndex + 1} has no input device assigned.");
            return;
        }
        button1 = inputDevice.TryGetChildControl<ButtonControl>(newButton1);
        button2 = inputDevice.TryGetChildControl<ButtonControl>(newButton2);
        button3 = inputDevice.TryGetChildControl<ButtonControl>(newButton3);
        button4 = inputDevice.TryGetChildControl<ButtonControl>(newButton4);
        Debug.Log($"Player {_playerIndex + 1} buttons updated: Button1={button1?.name}, Button2={button2?.name}, Button3={button3?.name}, Button4={button4?.name}");
    }
}
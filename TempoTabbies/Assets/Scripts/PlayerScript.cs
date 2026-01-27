using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public PlayerInput playerInput;

    // The players player number, 0 = player 1, 1 = player 2
    [Header("Who the player is")]
    public int _playerIndex;

    // Other scripts
    private _GameManager gameManager;
    private ChartSelectManager chartManager;
    private PauseMenuManager pauseMenu;
    private MainMenuManager mainMenu;
    private OptionsManager optionsMenu;
    private Create_LoadPlayer profilesMenu;
    private CatSelectionManager catMenu;
    private Player2Confirmation p2Confirm;

    // The needed inputs
    [Header("The inputs we're using")]
    public InputAction submit;
    public InputAction navigate;
    public InputAction clickButton;

    // Cards and Score
    [Header("In game info")]
    public List<CardDataScript.CardData> AllCards;
    public int Score;
    public int Combo;
    public int cat; // use this to say which cat has been chosen for the player, currently 1 = tabby, 2 = orange, 3 = nothing

    // Stored values that are player specific
    [Header("Stuff we save")]
    public float scrollSpeed;
    public bool assistTick;
    public bool showButtons;

    private void Awake()
    {
        // Makes players stay alive between scenes
        DontDestroyOnLoad(gameObject);
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
       /* if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Destroy(gameObject);
        }*/
        // Checks what state the game is currently in
        switch (gameManager.state)
        {
            case _GameManager.GameState.Start:
                break;
            case _GameManager.GameState.Player2Confirmation:
                if (p2Confirm == null)
                {
                    p2Confirm = FindFirstObjectByType<Player2Confirmation>();
                }
                float submitValue = submit.ReadValue<float>();
                if (submitValue > 0)
                {
                    p2Confirm.submit = submitValue;
                }
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
                float click = clickButton.ReadValue<float>();
                if (click > 0)
                {
                    optionsMenu.currentPlayer = this;
                }
                break;

            case _GameManager.GameState.StageSelect:
                if (chartManager == null)
                {
                    chartManager = FindFirstObjectByType<ChartSelectManager>();
                }
                chartManager.submitValue = submit.ReadValue<float>();
                break;

            case _GameManager.GameState.Game:
                // gets the pause menu script
                if (pauseMenu == null)
                {
                    pauseMenu = FindFirstObjectByType<PauseMenuManager>();
                }
                // Checks if the pauseMenu is inactive
                if (!pauseMenu.isPauseMenuActive)
                {
                    float submitValue2 = submit.ReadValue<float>();
                    if (submitValue2 > 0)
                    {
                        // Makes this player the active player
                        gameManager.whoGetsToPlay = _playerIndex;
                        // Opens the pause menu
                        pauseMenu.OpenPauseMenu();
                        // Disables the other controllers
                        DisableOthers();
                    }
                }
                // Checks if this player should be moving in the menus
                if (_playerIndex == gameManager.whoGetsToPlay && pauseMenu.isPauseMenuActive)
                {
                    // Checks the movement that we need for menus
                    pauseMenu.moveAmount = navigate.ReadValue<Vector2>();
                    pauseMenu.clickValue = clickButton.ReadValue<float>();
                }
                break;

            case _GameManager.GameState.CatSelect:
                if (catMenu == null)
                {
                    catMenu = FindFirstObjectByType<CatSelectionManager>();
                }
                if (_playerIndex == gameManager.whoGetsToPlay)
                {
                    catMenu.moveAmount = navigate.ReadValue<Vector2>();
                    catMenu.clickValue = clickButton.ReadValue<float>();
                }
                catMenu.submitValue = submit.ReadValue<float>();
                break;

            case _GameManager.GameState.CardSelection:
                // liikkuminenen            navigate.ReadValue<Vector2>();
                // napin A painaminen       clickButton.ReadValue<float>();
                break;
        }
    }

    // Turns off other players controls
    public void DisableOthers()
    {
        // All the controllers (keyboards, gamepads etc.)
        var allControllers = InputSystem.devices;
        // THIS controller
        var myDevice = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;

        // Checks each controller to turn them all off
        for (int i = 0; i < allControllers.Count; i++)
        {
            if (allControllers[i] != myDevice)
            {
                InputSystem.DisableDevice(allControllers[i]);
            }
        }
    }
    public void ApplyProfile(PlayerProfileData data)
    {
        scrollSpeed = data.scrollSpeed;
        assistTick = data.assistTick;
        showButtons = data.showButtons;

        Debug.Log($"Player {_playerIndex + 1} profiili ladattu");
    }
}

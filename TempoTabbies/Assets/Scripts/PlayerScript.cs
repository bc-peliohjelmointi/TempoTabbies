using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
    private PlayerInput playerInput;
    // The players player number, 0 = player 1, 1 = player 2
    public int _playerIndex;

    // Other scripts
    private _GameManager gameManager;
    private UIPlayerBehaviour pauseMenu;
    private MainMenuManager mainMenu;
    private OptionsManager optionsMenu;
    private StageSelectManager stageSelect;

    // The needed inputs
    public InputAction submit;
    public InputAction navigate;
    public InputAction clickButton;

    // Cards and Score
    public List<CardDataScript.CardData> AllCards;
    public int Score;

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
        // Checks what state the game is currently in
        if (gameManager.state == _GameManager.GameState.MainMenu)
        {
            // Gets the main menu script
            if (mainMenu == null)
            {
                mainMenu = FindFirstObjectByType<MainMenuManager>();
            }
            if (_playerIndex == gameManager.whoGetsToPlay)
            {
                // Checks the movement that we need for menus
                mainMenu.moveAmount = navigate.ReadValue<Vector2>();
                mainMenu.clickValue = clickButton.ReadValue<float>();
            }
        }
        else if (gameManager.state == _GameManager.GameState.Options)
        {
            // gets the options menu script
            if (optionsMenu == null)
            {
                optionsMenu = FindFirstObjectByType<OptionsManager>();
            }
            if (_playerIndex == gameManager.whoGetsToPlay)
            {
                // Checks the movement that we need for menus
                optionsMenu.moveAmount = navigate.ReadValue<Vector2>();
                optionsMenu.clickValue = clickButton.ReadValue<float>();
            }
        }
        else if (gameManager.state == _GameManager.GameState.StageSelect)
        {
            /// Not touching this yet, as I am pretty sure its getting changed a lot
            // gets the stage select script
            if (stageSelect == null)
            {
                stageSelect = FindFirstObjectByType<StageSelectManager>();
            }
        }
        else if (gameManager.state == _GameManager.GameState.Game)
        {
            // gets the pause menu script
            if (pauseMenu == null)
            {
                pauseMenu = FindFirstObjectByType<UIPlayerBehaviour>();
            }
            // Checks if the pauseMenu is inactive
            if (!pauseMenu.isPauseMenuActive)
            {
                float submitValue = submit.ReadValue<float>();
                if (submitValue > 0)
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
        }

        if (gameManager.state == _GameManager.GameState.CardSelection)
        {
            // liikkuminenen            navigate.ReadValue<Vector2>();
            // napin A painaminen       clickButton.ReadValue<float>();
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
                Debug.Log(i);
                InputSystem.DisableDevice(allControllers[i]);
            }
        }
    }

    // Turns on other players controls
    public void EnableOthers()
    {
        var allControllers = InputSystem.devices;
        var myDevice = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;
        for (int i = 0; i < allControllers.Count; i++)
        {
            if (allControllers[i] != myDevice)
            {
                InputSystem.EnableDevice(allControllers[i]);
            }
        }
    }
}

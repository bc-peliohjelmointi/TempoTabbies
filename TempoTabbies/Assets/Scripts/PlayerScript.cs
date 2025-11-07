using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    private PlayerInput playerInput;
    // Which player is active // 0 means player 1, 1 means player 2
    public int _playerIndex;

    private UIPlayerBehaviour pauseMenu;
    private _GameManager gameManager;

    // The needed inputs
    public InputAction submit;
    public InputAction navigate;
    public InputAction clickButton;

    public bool isInGame = true;


    private void Start()
    {
        pauseMenu = FindFirstObjectByType<UIPlayerBehaviour>();
        gameManager = FindFirstObjectByType<_GameManager>();
        playerInput = GetComponent<PlayerInput>();
        _playerIndex = playerInput.playerIndex;
        navigate = playerInput.actions.FindAction("Navigate");
        submit = playerInput.actions.FindAction("Submit");
        clickButton = playerInput.actions.FindAction("ClickButton");
    }

    private void Update()
    {
        // Checks if the player is in  a song
        if (isInGame)
        {
            // Checks if this player should be allowed to move in menus
            if (!pauseMenu.isPauseMenuActive)
            {
                float submitValue = submit.ReadValue<float>();
                pauseMenu.submitValue = submitValue;
                if (submitValue > 0)
                {
                    gameManager.whoGetsToPlay = _playerIndex;
                    DisableOthers();
                }
            }
            if (_playerIndex == gameManager.whoGetsToPlay && pauseMenu.isPauseMenuActive)
            {
                pauseMenu.moveAmount = navigate.ReadValue<Vector2>();
                pauseMenu.clickValue = clickButton.ReadValue<float>(); 
            }
        }
    }

    public void DisableOthers()
    {
        var allControllers = InputSystem.devices;
        var myDevice = playerInput.devices[_playerIndex];
        for (int i = 0; allControllers.Count > i; i++)
        {
            if (allControllers[i] != myDevice)
            {
                InputSystem.DisableDevice(allControllers[i]);
            }
        }
    }

    public void EnableOthers()
    {
        var allControllers = InputSystem.devices;
        for (int i = 0; allControllers.Count > i; i++)
        {
            if (allControllers[i] != InputSystem.devices[_playerIndex])
            {
                InputSystem.EnableDevice(allControllers[i]);
            }
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// The in game pause menu script
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    // Player movement, which is sent by the PlayerScript.cs Class
    [Header("Player input values")]
    public Vector2 moveAmount;
    public float submitValue;
    public float clickValue;

    private _GameManager gameManager;

    // Wether the menu is active or not
    [Header("Are we paused")]
    [SerializeField] public bool isPauseMenuActive = false;

    // Menu buttons
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] UnityEngine.UI.Button resume;
    [SerializeField] UnityEngine.UI.Button menu;

    // Short timer for when the menu goes away
    [SerializeField] private TextMeshProUGUI timerText;
    float timer;

    // The state for which button is currently selected
    public enum ButtonSelect
    {
        resume,
        menu
    }
    public ButtonSelect buttonSelect;
    // Movement timer to make movement in the menu better
    bool canMove;
    float moveTimer;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<_GameManager>();
    }

    private void Update()
    {
        // Check if the pause menu is active
        if (isPauseMenuActive)
        {
            // Check which button is meant to be selected
            switch (buttonSelect)
            {
                case ButtonSelect.resume: // Continue
                    EventSystem.current.SetSelectedGameObject(resume.gameObject);
                    if (clickValue > 0)
                    {
                        OnContinueClick();
                    }
                    // Moves to the desired button
                    if (moveAmount.y < -0.1f && canMove)
                    {
                        buttonSelect = ButtonSelect.menu;
                        canMove = false;
                    }
                    break;

                case ButtonSelect.menu: // Menu
                    EventSystem.current.SetSelectedGameObject(menu.gameObject);
                    if (clickValue > 0)
                    {
                        OnMenuClick();
                    }
                    // Moves to the desired button
                    else if (moveAmount.y > 0.1 && canMove)
                    {
                        buttonSelect = ButtonSelect.resume;
                        canMove = false;
                    }
                    break;
            }

            // Timer for movement, so the player doesn't just go to the top and bottom
            if (!canMove)
            {
                if (moveTimer < 0.2f)
                {
                    moveTimer += Time.deltaTime;
                }
                else
                {
                    canMove = true;
                    moveTimer = 0;
                }
            }
        }
        else if (!isPauseMenuActive)
        {
            // Timer for when the menu turns off
            // Set the timer to 4 when you turn the menu off
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                timerText.text = ((int)timer).ToString();
                if (timer <= 0)
                {
                    timerText.gameObject.SetActive(false);
                    // Make the game continue
                }
            }
        }
    }

    // What happens when you click the continue button
    public void OnContinueClick()
    {
        // Turns on the controllers that are disabled
        gameManager.EnableControllers();
        gameManager.state = _GameManager.GameState.Game;
        isPauseMenuActive = false;
        pauseMenu.SetActive(false);
        // Activates the timer
        timerText.gameObject.SetActive(true);
        timer = 4;
    }

    // What happens when you click the Menu button
    public void OnMenuClick()
    {
        gameManager.state = _GameManager.GameState.MainMenu;
        SceneManager.LoadScene("Options");
    }

    // Opens the pause menu mid game
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        gameManager.state = _GameManager.GameState.Pause;
        isPauseMenuActive = true;
        timerText.gameObject.SetActive(false);
        buttonSelect = ButtonSelect.resume;
    }
}

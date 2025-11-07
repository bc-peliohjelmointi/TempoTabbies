using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// The in game pause menu script
/// </summary>
public class UIPlayerBehaviour : MonoBehaviour
{
    public Vector2 moveAmount;
    public float submitValue;
    public float clickValue;

    private _GameManager gameManager;

    // Wether the menu is active or not
    [SerializeField] public bool isPauseMenuActive = false;

    // Menu buttons
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] UnityEngine.UI.Button button1;
    [SerializeField] UnityEngine.UI.Button button2;
    [SerializeField] UnityEngine.UI.Button button3;

    // Short timer for when the menu goes away
    [SerializeField] private TextMeshProUGUI timerText;
    float timer;

    // The state for which button is currently selected
    public enum ButtonSelect
    {
        button1,
        button2,
        button3
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
            // Check the stick movement
            //Vector2 moveAmount = navigate.ReadValue<Vector2>();

            // Check which button is meant to be selected
            switch (buttonSelect)
            {
                case ButtonSelect.button1:
                    EventSystem.current.SetSelectedGameObject(button1.gameObject);
                    if (clickValue > 0)
                    {
                        OnContinueClick();
                    }
                    if (moveAmount.y < -0.1f && canMove)
                    {
                        buttonSelect = ButtonSelect.button2;
                        canMove = false;
                    }
                    break;

                case ButtonSelect.button2:
                    EventSystem.current.SetSelectedGameObject(button2.gameObject);
                    if (clickValue > 0)
                    {
                        OnOptionsClick();
                    }
                    if (moveAmount.y < -0.1f && canMove)
                    {
                        buttonSelect = ButtonSelect.button3;
                        canMove = false;
                    }
                    else if (moveAmount.y > 0.1 && canMove)
                    {
                        buttonSelect = ButtonSelect.button1;
                        canMove = false;
                    }
                    break;

                case ButtonSelect.button3:
                    EventSystem.current.SetSelectedGameObject(button3.gameObject);
                    if (clickValue > 0)
                    {
                        OnQuitClick();
                    }
                    if (moveAmount.y > 0.1 && canMove)
                    {
                        buttonSelect = ButtonSelect.button2;
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

    public void OnContinueClick()
    {
        gameManager.EnableControllers();
        isPauseMenuActive = false;
        pauseMenu.SetActive(false);
        timerText.gameObject.SetActive(true);
        timer = 4;
    }

    public void OnOptionsClick()
    {
        SceneManager.LoadScene("Options");
    }

    public void OnQuitClick()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenPauseMenu(PlayerScript player)
    {
        pauseMenu.SetActive(true);
        isPauseMenuActive = true;
        timerText.gameObject.SetActive(false);
        buttonSelect = ButtonSelect.button1;
    }
}

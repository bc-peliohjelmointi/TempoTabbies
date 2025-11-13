using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu script
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MainMenuManager : MonoBehaviour
{
    // Menu objects
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] UnityEngine.UI.Button button1;
    [SerializeField] UnityEngine.UI.Button button2;
    [SerializeField] UnityEngine.UI.Button button3;

    private _GameManager gameManager;

    // Player movement input
    public Vector2 moveAmount;
    public float clickValue;

    // Audio
    AudioSource source;

    // State to know which button is being selected
    public enum ButtonSelect
    {
        button1,
        button2,
        button3
    }
    public ButtonSelect buttonSelect;
    // Timer to make movement between buttons better
    bool canMove;
    float moveTimer;

    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(button1.gameObject);
        gameManager = FindAnyObjectByType<_GameManager>();
        source = GetComponent<AudioSource>();
        source.Play();
        source.loop = true;
    }

    private void Update()
    {
        // Check which button is currently selected
        switch (buttonSelect)
        {
            case ButtonSelect.button1: // Stage select
                // Selects the correct button
                EventSystem.current.SetSelectedGameObject(button1.gameObject);
                // Checks if the button is clicked
                if (clickValue > 0)
                {
                    OnStageSelectClick();
                }
                // Moves to the desired button
                if (moveAmount.y < -0.1f && canMove)
                {
                    buttonSelect = ButtonSelect.button2;
                    canMove = false;
                }
                break;

            case ButtonSelect.button2: // Options
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

            case ButtonSelect.button3: // Quit
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

    public void OnStageSelectClick()
    {
        gameManager.state = _GameManager.GameState.StageSelect;
        SceneManager.LoadScene("MainGame");
    }

    public void OnOptionsClick()
    {
        gameManager.state = _GameManager.GameState.Options;
        SceneManager.LoadScene("Options");
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}

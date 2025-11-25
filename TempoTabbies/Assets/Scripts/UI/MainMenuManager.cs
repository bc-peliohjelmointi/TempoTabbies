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
    [Header("The UI elements")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] UnityEngine.UI.Button stageSelect;
    [SerializeField] UnityEngine.UI.Button options;
    [SerializeField] UnityEngine.UI.Button quit;

    private JSON_Stuff json;
    private _GameManager gameManager;

    // Player movement input
    [Header("Player input values")]
    public Vector2 moveAmount;
    public float clickValue;

    // Audio
    [Header("Audio file for background music")]
    AudioSource source;

    // State to know which button is being selected
    public enum ButtonSelect
    {
        stageSelect,
        options,
        quit
    }
    public ButtonSelect buttonSelect;
    // Timer to make movement between buttons better
    bool canMove;
    float moveTimer;

    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(stageSelect.gameObject);
        json = FindAnyObjectByType<JSON_Stuff>();
        gameManager = FindAnyObjectByType<_GameManager>();
        source = GetComponent<AudioSource>();
        source.Play();
        source.loop = true;

        json.LoadGameManager();
        json.LoadPlayer1();
        json.LoadPlayer2();
    }

    private void Update()
    {
        // Check which button is currently selected
        switch (buttonSelect)
        {
            case ButtonSelect.stageSelect: // Stage select
                // Selects the correct button
                EventSystem.current.SetSelectedGameObject(stageSelect.gameObject);
                // Checks if the button is clicked
                if (clickValue > 0)
                {
                    OnStageSelectClick();
                }
                // Moves to the desired button
                if (moveAmount.y < -0.1f && canMove)
                {
                    buttonSelect = ButtonSelect.options;
                    canMove = false;
                }
                break;

            case ButtonSelect.options: // Options
                EventSystem.current.SetSelectedGameObject(options.gameObject);
                if (clickValue > 0)
                {
                    OnOptionsClick();
                }
                if (moveAmount.y < -0.1f && canMove)
                {
                    buttonSelect = ButtonSelect.quit;
                    canMove = false;
                }
                else if (moveAmount.y > 0.1 && canMove)
                {
                    buttonSelect = ButtonSelect.stageSelect;
                    canMove = false;
                }
                break;

            case ButtonSelect.quit: // Quit
                EventSystem.current.SetSelectedGameObject(quit.gameObject);
                if (clickValue > 0)
                {
                    OnQuitClick();
                }
                if (moveAmount.y > 0.1 && canMove)
                {
                    buttonSelect = ButtonSelect.options;
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

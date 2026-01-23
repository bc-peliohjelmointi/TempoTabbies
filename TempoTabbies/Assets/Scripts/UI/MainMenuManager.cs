using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu script
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // Menu objects
    [Header("The UI elements")]
    [SerializeField] Button catSelect;
    [SerializeField] Button practise;
    [SerializeField] Button options;
    [SerializeField] Button quit;
    [SerializeField] Button multiplayer;

    private JSON_Stuff json;
    private _GameManager gameManager;

    // Player movement, which is sent by the PlayerScript.cs Class
    [Header("Player input values")]
    public Vector2 moveAmount;
    public float clickValue;

    

    // State to know which button is being selected
    public enum ButtonSelect
    {
        multiplayer,
        catSelect,
        practice,
        options,
        quit
    }
    public ButtonSelect buttonSelect;
    // Timer to make movement between buttons better
    bool canMove;
    float moveTimer;

    private void Awake()
    {
        StartCoroutine(WaitAFrame());
        json = FindAnyObjectByType<JSON_Stuff>();
        gameManager = FindAnyObjectByType<_GameManager>();

        json.LoadGameManager();
    }

    IEnumerator WaitAFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(quit.gameObject);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(practise.gameObject);
        buttonSelect = ButtonSelect.practice;
    }

    private void Update()
    {
        // Check which button is currently selected
        switch (buttonSelect)
        {
            case ButtonSelect.catSelect: // cat select
                // Selects the correct button
                EventSystem.current.SetSelectedGameObject(catSelect.gameObject);
                // Checks if the button is clicked
                /*if (clickValue > 0)
                {
                    OnCatSelectClick();
                }*/
                // Moves to the desired button
                if (moveAmount.y < -0.1f && canMove)
                {
                    buttonSelect = ButtonSelect.practice;
                    canMove = false;
                }
                break;

            case ButtonSelect.practice:
                // Selects the correct button
                EventSystem.current.SetSelectedGameObject(practise.gameObject);
                // Moves to the desired button
                if (moveAmount.y < -0.1f && canMove)
                {
                    buttonSelect = ButtonSelect.options;
                    canMove = false;
                }
                /*else if (moveAmount.y > 0.1 && canMove)
                {
                    buttonSelect = ButtonSelect.catSelect;
                    canMove = false;
                }*/
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
                    buttonSelect = ButtonSelect.practice;
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

    public void OnCatSelectClick()
    {
        gameManager.state = _GameManager.GameState.Player2Confirmation;
        gameManager.multiplayer = true;
        SceneManager.LoadScene("Player2Confirmation");
    }

    public void OnPractiseClick()
    {
        gameManager.multiplayer = false;
        gameManager.state = _GameManager.GameState.CatSelect;
        SceneManager.LoadScene("CatSelect");
    }

    public void OnOptionsClick()
    {
        gameManager.state = _GameManager.GameState.Options;
        SceneManager.LoadScene("Options");
    }

    public void OnMultiplayerClick()
    {
        gameManager.multiplayer = true;
        SceneManager.LoadScene("MultiplayerStageSelect");
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}

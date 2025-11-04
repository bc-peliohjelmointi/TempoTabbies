using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIPlayerBehaviour : MonoBehaviour
{
    // Wether the manu is active or not
    [SerializeField] public bool isPauseMenuActive = false;
    [SerializeField] public bool isStageSelectActive = true;

    // Pause menu
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject pauseButton;

    // Short timer for when the menu goes away
    [SerializeField] private TextMeshProUGUI timerText;
    float timer;

    // Stage select menu
    [SerializeField] private GameObject stageSelect;
    [SerializeField] private GameObject stageButton;

    private void Update()
    {
        if (!isPauseMenuActive && !isStageSelectActive)
        {
            if (Input.GetButtonDown("Submit"))
            {
                OpenPauseMenu();
            }
            // Timer for when the menu turns off
            // Set the timer to 3 when you turn the menu off
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                timerText.text = ((int)timer).ToString();
                if (timer >= 0)
                {
                    // Make the game start
                }
            }
        }
    }

    public void OnContinueClick()
    {
        isPauseMenuActive = false;
        pauseMenu.SetActive(false);
        timerText.gameObject.SetActive(true);
        timer = 3;
    }

    public void OnOptionsClick()
    {
        SceneManager.LoadScene("Options"); 
    }

    public void OnQuitClick()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        isPauseMenuActive = true;
        timerText.gameObject.SetActive(false);
        // Changes the button to be the currently selected object for the controller
        EventSystem.current.SetSelectedGameObject(pauseButton);
    }

    public void OpenStageSelect()
    {
        isStageSelectActive = true;
        stageSelect.SetActive(true);
        EventSystem.current.SetSelectedGameObject(stageButton);
    }

    public void OnStage1Click()
    {
        isStageSelectActive = false;
        OnContinueClick();
        // Begin the game with stage 1 as the song
    }

    public void OnStage2Click()
    {
        isStageSelectActive = false;
        OnContinueClick();
        // Begin the game with stage 2 as the song
    }
}

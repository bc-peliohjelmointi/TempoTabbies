using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPlayerBehaviour : MonoBehaviour
{
    // Wether the manu is actie or not
    [SerializeField] public bool isMenuActive = true;

    // All the buttons
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject button;

    // Short timer for when the menu goes away
    float timer;

    private void Start()
    {
    }

    private void Update()
    {
        if (!isMenuActive)
        {
            if (Input.GetButtonDown("Submit"))
            {
                OpenMenu();
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

    // Actually changes the state of the state, to make all buttons real

    public void OnContinueClick()
    {
        isMenuActive = false;
        menu.SetActive(false);
        timerText.gameObject.SetActive(true);
        timer = 3;
    }

    public void OnOptionsClick()
    {
        // Open the options screen 
    }

    public void OnQuitClick()
    {
        // Return the player to the starting menu
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        isMenuActive = true;
        timerText.gameObject.SetActive(false);
        // Changes the button to be the currently selected object for the controller
        EventSystem.current.SetSelectedGameObject(button);
    }
}

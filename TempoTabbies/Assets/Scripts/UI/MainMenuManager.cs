using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Menu
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] UnityEngine.UI.Button button1;
    [SerializeField] UnityEngine.UI.Button button2;
    [SerializeField] UnityEngine.UI.Button button3;

    public InputAction navigate;

    public enum ButtonSelect
    {
        button1,
        button2,
        button3
    }
    public ButtonSelect buttonSelect;
    bool canMove;
    float moveTimer;

    private void Awake()
    {
        navigate = InputSystem.actions.FindAction("Navigate");
        EventSystem.current.SetSelectedGameObject(button1.gameObject);
    }

    private void Update()
    {
        Vector2 moveAmount = navigate.ReadValue<Vector2>();
        switch (buttonSelect)
        {
            case ButtonSelect.button1:
                EventSystem.current.SetSelectedGameObject(button1.gameObject);
                if (moveAmount.y < -0.1f && canMove)
                {
                    buttonSelect = ButtonSelect.button2;
                    canMove = false;
                }
                break;

            case ButtonSelect.button2:
                EventSystem.current.SetSelectedGameObject(button2.gameObject);
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
        SceneManager.LoadScene("MainGame");
    }

    public void OnOptionsClick()
    {
        SceneManager.LoadScene("Options");
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage select menu script
/// </summary>
public class StageSelectManager : MonoBehaviour
{
    private _GameManager gameManager;

    // The player actions
    public InputAction navigate;

    // The menu objects
    [SerializeField] private GameObject stageSelect;
    [SerializeField] private UnityEngine.UI.Button stage1;
    [SerializeField] private UnityEngine.UI.Button stage2;

    // The state to know which button is currently selected
    public enum Stage
    {
        stage1,
        stage2
    }
    public Stage stage;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<_GameManager>();
        EventSystem.current.SetSelectedGameObject(stage1.gameObject);
        navigate = InputSystem.actions.FindAction("Navigate");
    }

    private void Update()
    {
        Vector2 moveAmount = navigate.ReadValue<Vector2>();

        // Checks which stage button is currently selected
        switch (stage)
        {
            case Stage.stage1:
                EventSystem.current.SetSelectedGameObject(stage1.gameObject);
                if (moveAmount.y < -0.1f)
                {
                    stage = Stage.stage2;
                }
                break;
            case Stage.stage2:
                EventSystem.current.SetSelectedGameObject(stage2.gameObject);
                if (moveAmount.y > 0.1f)
                {
                    stage = Stage.stage1;
                }
                break;
        }
    }

    public void OnStage1Click()
    {
        gameManager.stageID = 1;
        SceneManager.LoadScene("Game");
    }

    public void OnStage2Click()
    {
        gameManager.stageID = 2;
        SceneManager.LoadScene("Game");
    }
}
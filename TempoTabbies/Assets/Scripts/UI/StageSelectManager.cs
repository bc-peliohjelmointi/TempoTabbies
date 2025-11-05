using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StageSelectManager : MonoBehaviour
{
    private _GameManager gameManager;

    public InputAction navigate;

    [SerializeField] public bool isStageSelectActive = true;

    [SerializeField] private GameObject stageSelect;

    [SerializeField] private UnityEngine.UI.Button stage1;
    [SerializeField] private UnityEngine.UI.Button stage2;

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

        if (isStageSelectActive)
        {
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
    }

    public void OpenStageSelect()
    {
        isStageSelectActive = true;
        stageSelect.SetActive(true);
        EventSystem.current.SetSelectedGameObject(stage1.gameObject);
    }

    public void OnStage1Click()
    {
        gameManager.stageID = 1;
        isStageSelectActive = false;
        // Begin the game with stage 1 as the song
    }

    public void OnStage2Click()
    {
        gameManager.stageID = 2;
        isStageSelectActive = false;
        // Begin the game with stage 2 as the song
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    public MenuAnimations paw;

    // Player movement, which is sent by the PlayerScript.cs Class
    [Header("Player input values")]
    public Vector2 moveAmount;
    public float clickValue;

    private GameObject lastSelected;


    private void Awake()
    {
        gameManager = FindAnyObjectByType<_GameManager>();
        gameManager.state = _GameManager.GameState.MainMenu;
        StartCoroutine(WaitAFrame());
        json = FindAnyObjectByType<JSON_Stuff>();

        json.LoadGameManager();
    }

    IEnumerator WaitAFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(quit.gameObject);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(catSelect.gameObject);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (lastSelected != EventSystem.current.currentSelectedGameObject)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
        }
        if (lastSelected != EventSystem.current.currentSelectedGameObject)
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }

    public void OnCatSelectClick()
    {
        gameManager.multiplayer = true;
        paw.scene = "PlayerSelect";
    }

    public void OnPractiseClick()
    {
        gameManager.multiplayer = false;
        paw.scene = "PlayerSelect";
    }

    public void OnOptionsClick()
    {
        paw.scene = "Options";
    }

    public void OnMultiplayerClick()
    {
        gameManager.multiplayer = true;
        paw.scene = "MultiplayerStageSelect";
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}

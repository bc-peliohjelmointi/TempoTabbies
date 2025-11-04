using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject button;
    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(button);
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

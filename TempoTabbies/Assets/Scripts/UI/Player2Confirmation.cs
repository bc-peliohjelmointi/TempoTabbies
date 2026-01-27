using UnityEngine;
using UnityEngine.SceneManagement;

public class Player2Confirmation : MonoBehaviour
{
    private _GameManager gm;
    public float submit;

    private void Awake()
    {
        gm = FindFirstObjectByType<_GameManager>();
        gm.state = _GameManager.GameState.Player2Confirmation;
    }

    // Update is called once per frame
    void Update()
    {
        if (submit > 0)
        {
            gm.state = _GameManager.GameState.MainMenu;
            SceneManager.LoadScene("MainMenu");
        }
        if (gm.p2 != null)
        {
            gm.state = _GameManager.GameState.CatSelect;
            SceneManager.LoadScene("CatSelect");
        }
    }
}

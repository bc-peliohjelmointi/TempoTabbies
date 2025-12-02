using UnityEngine;
using UnityEngine.SceneManagement;

public class Player2Confirmation : MonoBehaviour
{
    _GameManager gm;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = FindFirstObjectByType<_GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.p2 != null)
        {
            gm.state = _GameManager.GameState.CatSelect;
            SceneManager.LoadScene("CatSelect");
        }
    }
}

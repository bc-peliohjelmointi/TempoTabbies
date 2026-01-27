using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    private _GameManager gm;
    public MenuAnimations anims;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = FindFirstObjectByType<_GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.p1 != null)
        {
            anims.scene = "MainMenu";
            anims.PawStB();
        }
    }
}

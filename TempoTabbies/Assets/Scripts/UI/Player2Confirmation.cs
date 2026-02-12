using UnityEngine;
using UnityEngine.SceneManagement;

public class Player2Confirmation : MonoBehaviour
{
    private _GameManager gm;
    private JSON_Stuff json;
    public MenuAnimations anims;
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
            anims.scene = "MainMenu";
            anims.PawStB();
        }
        if (gm.p2 != null)
        {
            anims.scene = "CatSelect";
            if (anims.animator.GetCurrentAnimatorStateInfo(0).length < anims.animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
            {
                anims.PawStB();
            }
        }
    }
}

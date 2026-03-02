using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TonextSceneDuoScript : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI p1;
    public TextMeshProUGUI p2;

    public JSON_Stuff json;
    public MenuAnimations anims;
    public _GameManager gm;

    private void Start()
    {
        json = FindFirstObjectByType<JSON_Stuff>();
        gm = FindFirstObjectByType<_GameManager>();
    }

    public void onClickEnter()
    {
        if (gm.p2 != null)
        {
            json.LoadPlayerToPlayer(p1.text, 0);
            json.LoadPlayerToPlayer(p2.text, 1);
            anims.scene = "CatSelect";
            if (anims.animator.GetCurrentAnimatorStateInfo(0).length < anims.animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
            {
                anims.PawStB();
            }
        }
    }
}

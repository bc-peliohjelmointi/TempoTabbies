using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TonextSceneDuoScript : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI p1;
    public TextMeshProUGUI p2;
    public TextMeshProUGUI gameModeName;
    public TextMeshProUGUI gameMode;

    public JSON_Stuff json;
    public MenuAnimations anims;
    public _GameManager gm;

    private void Start()
    {
        json = FindFirstObjectByType<JSON_Stuff>();
        gm = FindFirstObjectByType<_GameManager>();
        gameMode.text = "In normal mode, the game plays regularly, with no effects that change gameplay";
    }

    public void onClickEnter()
    {
        if (gameModeName.text == "Normal Mode")
        {
            gm.party = false;
        }
        else if (gameModeName.text == "Party Mode")
        {
            gm.party = true;
        }
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
    public void GameModes(TextMeshProUGUI mode)
    {
        if (mode.text == "Normal Mode")
        {
            gameMode.text = "In normal mode, the game plays regularly, with no effects that change gameplay";
        }
        else if (mode.text == "Party Mode")
        {
            gameMode.text = "In party mode, players choose cards after a song to impair the other player or help themselves";
        }
    }
}

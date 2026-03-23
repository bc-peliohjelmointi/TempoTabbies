using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TonextSceneDuoScript : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI p1;
    public TextMeshProUGUI p2;
    public TextMeshProUGUI gameModeName;
    public TextMeshProUGUI gameMode;

    public GameObject p2Object;

    public JSON_Stuff json;
    public MenuAnimations anims;
    public _GameManager gm;

    private void Start()
    {
        json = FindFirstObjectByType<JSON_Stuff>();
        gm = FindFirstObjectByType<_GameManager>();
        gameMode.text = "In normal mode, the game plays regularly, with no effects that change gameplay";
        if (!gm.multiplayer)
        {
            p2Object.SetActive(false);
        }
    }

    public void onClickEnter()
    {
        if (gameModeName.text == "Normal Mode")
        {
            gm.party = false;
            gm.crazy = false;
        }
        else if (gameModeName.text == "Party Mode")
        {
            gm.party = true;
            gm.crazy = false;
        }
        else if (gameModeName.text == "Crazy Mode")
        {
            gm.party = true;
            gm.crazy = true;
        }

        Debug.Log(p1.text);
        if (!File.Exists($"JSON/{p1.text}.json"))
        {
            json.LoadPlayerToPlayer($"DefaultProfiles/{p1.text}", 0);
        }
        else
        {
            json.LoadPlayerToPlayer(p1.text, 0);
        }

        if (gm.p2 != null && gm.multiplayer)
        {
            if (!File.Exists($"JSON/{p2.text}.json"))
            {
                json.LoadPlayerToPlayer($"DefaultProfiles/{p2.text}", 1);
            }
            else
            {
                json.LoadPlayerToPlayer(p2.text, 1);
            }

            anims.scene = "CatSelect";
            if (anims.animator.GetCurrentAnimatorStateInfo(0).length < anims.animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
            {
                anims.PawStB();
            }
        }
        else if (!gm.multiplayer)
        {
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
        else if (mode.text == "Crazy Mode")
        {
            gameMode.text = "In crazy mode, players can have multiple effects active at once \n Warning: This can result in very crazy games";
        }
    }
}

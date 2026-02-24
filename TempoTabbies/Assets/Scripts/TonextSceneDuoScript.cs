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

    private void Start()
    {
        json = FindFirstObjectByType<JSON_Stuff>();
    }

    public void onClickEnter()
    {
        json.LoadPlayerToPlayer(p1.text, 0);
        json.LoadPlayerToPlayer(p2.text, 1);
        SceneManager.LoadScene("CatSelect");
    }
}

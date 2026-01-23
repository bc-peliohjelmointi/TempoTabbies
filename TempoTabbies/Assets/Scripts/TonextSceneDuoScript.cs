using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TonextSceneDuoScript : MonoBehaviour
{
    public Button button;
    public void onClickEnter()
    {
        SceneManager.LoadScene("CatSelect");
    }
}

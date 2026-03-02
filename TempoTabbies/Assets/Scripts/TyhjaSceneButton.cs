using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TyhjaSceneButton : MonoBehaviour
{
    public Button button;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void onClickEnter()
    {
        SceneManager.LoadScene("KorttiValintaScene");
    }
}

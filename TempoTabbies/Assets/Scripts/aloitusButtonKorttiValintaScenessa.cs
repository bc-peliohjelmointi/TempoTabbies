using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class aloitusButtonKorttiValintaScenessa : MonoBehaviour
{
    public Button button;
    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Image paw;
    public bool changeSize;
    public bool starting = false;
    public bool verticalLock;

    private void Awake()
    {
        paw.gameObject.SetActive(starting);
    }

    private void Update()
    {
        if (verticalLock)
        {
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.Vertical;
        }
    }

    private void OnDisable()
    {
        paw.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (changeSize) { gameObject.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f); }
        paw.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (changeSize) { gameObject.transform.localScale = Vector3.one; }
        paw.gameObject.SetActive(false);
    }
}

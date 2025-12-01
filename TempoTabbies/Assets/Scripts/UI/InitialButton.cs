using UnityEngine;
using UnityEngine.EventSystems;

public class InitialButton : MonoBehaviour, IDeselectHandler
{
    public void OnDeselect(BaseEventData eventData)
    {
        gameObject.SetActive(false);
    }
}

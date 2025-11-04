using UnityEngine;
using UnityEngine.EventSystems;

public class StageUISelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // Input what happes when you select the button
    public void OnSelect(BaseEventData eventData)
    {
        gameObject.transform.localScale = new Vector3(6, 6, 6);
        // Prolly play the first song and show a big image of the songs
    }

    // Input what happens when you deselect the button
    public void OnDeselect(BaseEventData eventData)
    {
        gameObject.transform.localScale = new Vector3(3, 3, 3);
        // Undo what OnSelect() does
    }
}

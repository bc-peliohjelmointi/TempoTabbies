using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class is for buttons and changes what they do when selected
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ButtonSelected : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // The song attached to the button
    AudioSource source;
    [SerializeField] bool makeBigger = false;

    // Input what happes when you select the button
    public void OnSelect(BaseEventData eventData)
    {
        // we find these here, because the menu scripts select buttons, which can happen before the buttons awake
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }

        source.PlayOneShot(source.clip);

        if (makeBigger)
        {
            gameObject.transform.localScale = new Vector3(4f, 4f, 4f);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (makeBigger)
        {
            gameObject.transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
        }
    }
}

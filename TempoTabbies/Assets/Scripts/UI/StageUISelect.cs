using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class is for buttons and changes what they do when selected or deselected
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class StageUISelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // The song attached to the button
    AudioSource source;

    private _GameManager gameManager;

    // Input what happes when you select the button
    public void OnSelect(BaseEventData eventData)
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<_GameManager>();
        }
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }
        // Buttons all play a sound when selected, 
        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    // Input what happens when you deselect the button
    public void OnDeselect(BaseEventData eventData)
    {
        // Undo what OnSelect() does
    }
}

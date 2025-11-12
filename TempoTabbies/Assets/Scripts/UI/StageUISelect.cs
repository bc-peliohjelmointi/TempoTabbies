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
        // we find these here, because the menu scripts select buttons, which can happen before the buttons awake
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<_GameManager>();
        }
        if (source == null)
        {
            source = GetComponent<AudioSource>();
        }

        // Play the audio, if it is not already playing, this stops the button select tweaking out and selecting itself twice in a millisecond
        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    // Input what happens when you deselect the button
    public void OnDeselect(BaseEventData eventData)
    {
        
    }
}

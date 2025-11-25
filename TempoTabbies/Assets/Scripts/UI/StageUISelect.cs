using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class is for buttons and changes what they do when selected
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class StageUISelect : MonoBehaviour, ISelectHandler
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

        source.PlayOneShot(source.clip);
    }
}

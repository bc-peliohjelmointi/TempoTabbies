using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class is for buttons and changes what they do when selected or deselected
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class StageUISelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    AudioSource source;
    [SerializeField] GameObject songsImage;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        Debug.Log(source);
    }

    // Input what happes when you select the button
    public void OnSelect(BaseEventData eventData)
    {
        // Prolly play the first song and show a big image of the songs
        gameObject.transform.localScale = new Vector3(6, 6, 6);
        songsImage.SetActive(true);
        source.Play();
    }

    // Input what happens when you deselect the button
    public void OnDeselect(BaseEventData eventData)
    {
        // Undo what OnSelect() does
        gameObject.transform.localScale = new Vector3(3, 3, 3);
        songsImage.SetActive(false);
        source.Stop();
    }
}

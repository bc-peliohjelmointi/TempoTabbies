using UnityEngine;
using UnityEngine.EventSystems;

public class ChartButtonActualButton : MonoBehaviour, ISelectHandler
{
    public SongButtonsActualButton songButton;

    public void OnSelect(BaseEventData baseEvent)
    {
        if (songButton == null) return;
        if (!songButton.Music.isPlaying)
        {
            StartCoroutine(songButton.ChangeSongs());
        }
    }
}

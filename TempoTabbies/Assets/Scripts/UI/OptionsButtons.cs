using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsButtons : MonoBehaviour, ISelectHandler
{
    public OptionsManager manager;
    public int menuNumber;
    public void OnSelect(BaseEventData eventData)
    {
        if (menuNumber == 0)
        {
            manager.OnAudioMenuClick(false);
        }
        else if (menuNumber == 1)
        {
            manager.OnGameplayMenuClick(false);
        }
        else if (menuNumber == 2)
        {
            manager.OnProfileMenuClick(false);
        }
        else if(menuNumber == 3)
        {
            manager.OnAccessabilityClick(false);
        }
    }
}

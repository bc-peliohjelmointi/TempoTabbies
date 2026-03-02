using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileButtonObject : MonoBehaviour, ISelectHandler
{
    public Scrollbar scrollbar;
    public TMP_Dropdown dropdown;

    public void OnSelect(BaseEventData eventData)
    {
        scrollbar = dropdown.GetComponentInChildren<Scrollbar>();
        scrollbar.value += 1/dropdown.options.Count;
    }
}

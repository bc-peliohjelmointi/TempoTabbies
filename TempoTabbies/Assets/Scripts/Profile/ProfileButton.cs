using TMPro;
using UnityEngine;

public class ProfileButton : MonoBehaviour
{
    /// <summary>
    /// Saving the player does NOT work
    /// </summary>
    public JSON_Stuff json;
    public Create_LoadPlayer maker;
    public TextMeshProUGUI text;

    private void Awake()
    {
        name = name.Replace("(Clone)", "");
        text.text = name;
    }

    public void LoadJSON()
    {
        json.LoadPlayer(name, maker.playerNumber);
        maker.scrollSlider.value = maker.scrollSpeed;
        maker.AssistTick(); maker.AssistTick();
    }
}

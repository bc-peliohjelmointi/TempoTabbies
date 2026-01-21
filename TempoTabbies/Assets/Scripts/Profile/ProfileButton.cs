using System.IO;
using TMPro;
using UnityEngine;

public class ProfileButton : MonoBehaviour
{
    public JSON_Stuff json;
    public Create_LoadPlayer maker;
    public TextMeshProUGUI text;

    private void Awake()
    {
        name = name.Replace("(Clone)", "");
        text.text = name;
        if (json == null)
        {
            json = FindFirstObjectByType<JSON_Stuff>();
        }
    }

    public void NullName()
    {
        maker.chosenName.text = "Name of profile";
    }

    public void LoadJSON()
    {
        json.LoadPlayer(name);
        maker.chosenName.text = name;
        maker.scrollSlider.value = maker.scrollSpeed;
        maker.AssistTick(); maker.AssistTick();
    }
}

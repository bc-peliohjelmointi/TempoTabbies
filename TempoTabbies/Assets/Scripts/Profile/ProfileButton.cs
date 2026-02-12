using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProfileButton : MonoBehaviour
{
    public JSON_Stuff json;
    public Create_LoadPlayer maker;

    public TextMeshProUGUI text; // This buttons text

    private void Awake()
    {
        text.text = name;
        if (json == null)
        {
            json = FindFirstObjectByType<JSON_Stuff>();
        }
        if (maker == null)
        {
            maker = FindFirstObjectByType<Create_LoadPlayer>();
        }
    }

    // Changes the text in the input field to the starting text
    public void NullName()
    {
        maker.chosenName.text = "Name of profile";
        maker.chosenNameBackup = "Name of profile";
    }

    // Loads the JSON file attached to this object
    public void LoadJSON()
    {
        json.LoadPlayer(name);
        maker.chosenName.text = name;
        maker.scrollSlider.value = maker.scrollSpeed;
        maker.ShowButtons(); maker.ShowButtons();
        maker.AssistTick(); maker.AssistTick();
    }
}

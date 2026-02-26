using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileButton : MonoBehaviour, ISelectHandler
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

    public void OnSelect(BaseEventData eventData)
    {
        if (maker.lastSelectedGameObject != null && maker.lastSelectedGameObject.name != "Save")
        {
            if (maker.lastSelectedGameObject.transform.position.y > gameObject.transform.position.y)
            {
                maker.startButton.gameObject.transform.position += new Vector3(0, 30, 0);
                maker.newPlayerBtn.gameObject.transform.position += new Vector3(0, 30, 0);
                foreach (GameObject go in maker.playerList)
                {
                    go.transform.position += new Vector3(0, 30, 0);
                }
            }
            else if (maker.lastSelectedGameObject.transform.position.y < gameObject.transform.position.y)
            {
                maker.startButton.gameObject.transform.position -= new Vector3(0, 30, 0);
                maker.newPlayerBtn.gameObject.transform.position -= new Vector3(0, 30, 0);
                foreach (GameObject go in maker.playerList)
                {
                    go.transform.position -= new Vector3(0, 30, 0);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Create_LoadPlayer : MonoBehaviour
{
    public JSON_Stuff json; // JSON script
    public string player; // string that we put in JSON to store names

    public float scrollSpeed; // the number we change
    public Slider scrollSlider; // the object itself
    public TextMeshProUGUI scrollValue; // the number next to the slider

    public bool assistTick; // the bool we change for assist tick
    public Image assistImage; // the image to show if assist tick is off or on

    public bool showButtons; // the bool we change for wether button inputs are shown on screen
    public Image buttonImage; // the image to show if showing button inputs is off or on;

    public TMP_InputField chosenName; // the input field we get players to input file names

    [Header("GameObjects to keep track of the JSON file buttons")]
    public GameObject playerPrefab; // the object we copy for players
    public GameObject playerParent; // the placement of the copies original spot
    public List<GameObject> playerList; // list of created plyer buttons


    [Header("Where buttons go when changin sides")]
    public GameObject startButton; // the button that we start on
    public GameObject startOfEdit; // the button the editing starts on

    [field:HideInInspector]
    public Button newPlayer;

    private void Awake()
    {
        GoToStartButton();
        MakeButtons();
        AssistTick(); AssistTick();
        if (json == null)
        {
            json = FindFirstObjectByType<JSON_Stuff>();
        }
    }

    private void Update()
    {
        scrollValue.text = scrollSlider.value.ToString();
    }

    /// <summary>
    /// Deletes the current player list
    /// Finds the JSON files in the "JSON" folder
    /// Creates a player button for each of them
    /// </summary>
    public void MakeButtons()
    {
        foreach (GameObject player in playerList)
        {
            Destroy(player);
        }
        playerList.Clear();
        int placement = -80;
        var last = Directory.GetFiles("JSON").LastOrDefault();
        foreach (string file in Directory.GetFiles("JSON"))
        {
            GameObject button = Instantiate(playerPrefab, playerParent.transform);
            button.name = file.Replace("JSON\\", "");
            button.name.Replace("(Clone)", "");
            button.transform.position += new Vector3(0, placement, 0);
            button.SetActive(true);
            placement -= 40;
            if (playerList.Count == 0)
            {
                Navigation nav = new Navigation();
                nav = newPlayer.navigation;
                nav.selectOnDown = button.GetComponent<Button>();
                newPlayer.navigation = nav;
            }
            Button btn = button.GetComponent<Button>();
            Navigation nav2 = new Navigation();
            if (!file.Equals(last))
            {
                nav2.mode = Navigation.Mode.Vertical;
                btn.navigation = nav2;
            }
            else
            {
                nav2.mode = Navigation.Mode.Explicit;
                nav2.selectOnDown = null;
                nav2.selectOnUp = playerList[playerList.Count - 1].GetComponent<Button>();
                btn.navigation = nav2;
            }
            playerList.Add(button);
        }
    }

    // Changes string player into the given text objects text
    public void ChangeName(TextMeshProUGUI text)
    {
        player = text.text;
    }

    // Saves the given details into a JSON file, excluding some names
    public void SaveName()
    {
        if (chosenName.text.ToLower() != "name of profile" && chosenName.text.ToLower() != "beginner" && chosenName.text.ToLower() != "seasoned" && chosenName.text.ToLower() != "expert")
        {
            json.SavePlayer(player, scrollSpeed, assistTick, showButtons);
        }
    }

    // Deletes whatever JSON file shares a name with what is in the input field
    public void DeleteName()
    {
        File.Delete($"JSON/{chosenName.text}");
    }

    // Makes sure the scroll speed doesn't have 300 decimals
    public void ScrollSpeed()
    {
        if ((int)scrollSlider.value != scrollSlider.value)
        {
            scrollSlider.value = (int)scrollSlider.value;
        }
        scrollSpeed = scrollSlider.value;
    }

    // Swaps assist tick between true/false
    public void AssistTick()
    {
        if (assistTick == false)
        {
            assistTick = true;
            assistImage.color = Color.green;
        }
        else
        {
            assistTick = false;
            assistImage.color = Color.red;
        }
    }

    // Swaps show buttons between true/false
    public void ShowButtons()
    {
        if (showButtons == false)
        {
            showButtons = true;
            buttonImage.color = Color.green;
        }
        else
        {
            showButtons = false;
            buttonImage.color = Color.red;
        }
    }

    // Makes the selected object the starting button
    public void GoToStartButton()
    {
        EventSystem.current.SetSelectedGameObject(startButton);
    }

    // Makes the selected object the editing starting button
    public void GoToStartOfEdit()
    {
        EventSystem.current.SetSelectedGameObject(startOfEdit);
    }

    // Changes scenes back to options
    public void BackToOptions()
    {
        SceneManager.LoadScene("Options");
    }
}

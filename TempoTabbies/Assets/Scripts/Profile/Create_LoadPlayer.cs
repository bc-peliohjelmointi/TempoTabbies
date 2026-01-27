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
    [field: HideInInspector]
    public _GameManager _gm;

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

    [Header("Where buttons go when changing sides")]
    public GameObject startButton; // the button that we start on
    public GameObject startOfEdit; // the button the editing starts on

    [field: HideInInspector]
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
        if (_gm == null)
        {
            _gm = FindFirstObjectByType<_GameManager>();
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
        // Removs pre existing buttons
        foreach (GameObject player in playerList)
        {
            Destroy(player);
        }
        playerList.Clear();

        int placement = -80; // The Y axis of the current button being made
        var last = Directory.GetFiles("JSON").LastOrDefault(); // Checks wether this is the last loop of foreach
        foreach (string file in Directory.GetFiles("JSON"))
        {
            Navigation nav = new Navigation(); // A placeholder navigation for buttons
            GameObject button = Instantiate(playerPrefab, playerParent.transform); // Makes the button
            Button btn = button.GetComponent<Button>(); // Gets the new buttons Button component
            button.name = file.Replace("JSON\\", ""); // Change the buttons name
            button.name.Replace("(Clone)", "");
            // if the name is one of the base classes, don't maake the button
            if (button.name.ToLower() == "beginner" || button.name.ToLower() == "seasoned" || button.name.ToLower() == "expert")
            { 
                Destroy(button);
            }
            else
            {
                button.transform.position += new Vector3(0, placement, 0); // Put the button in the right place
                button.SetActive(true);
                placement -= 40; // Changes placement for the next button

                // Changes the starting buttons navigation to go to the first created button
                if (playerList.Count == 0)
                {
                    nav = newPlayer.navigation;
                    nav.selectOnDown = btn;
                    newPlayer.navigation = nav;
                }

                // Changes every button except the last to have vertical navigation
                if (!file.Equals(last))
                {
                    nav.mode = Navigation.Mode.Vertical;
                    btn.navigation = nav;
                }
                // The last buttons navigation goes vertical but can't go down
                else
                {
                    nav.mode = Navigation.Mode.Explicit;
                    nav.selectOnDown = null;
                    nav.selectOnUp = playerList[playerList.Count - 1].GetComponent<Button>();
                    btn.navigation = nav;
                }

                playerList.Add(button);
            }
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
        if (File.Exists(chosenName.text))
        {
            File.Delete($"JSON/{chosenName.text}");
        }
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
        _gm.state = _GameManager.GameState.Options;
    }
    public void ApplyProfileToPlayer(PlayerScript player)
    {
        if (player == null) return;

        player.scrollSpeed = scrollSpeed;
        player.assistTick = assistTick;
        player.showButtons = showButtons;

        Debug.Log($"Profile annettu Player {player._playerIndex + 1}");
    }
    public void ApplyProfileToActivePlayer()
    {
        PlayerScript target = _gm.players
            .FirstOrDefault(p => p._playerIndex == _gm.whoGetsToPlay);

        ApplyProfileToPlayer(target);
    }

}

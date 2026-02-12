using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class Create_LoadPlayer : MonoBehaviour
{
    [field: HideInInspector]
    public _GameManager _gm;

    [field: HideInInspector]
    public float submit;
    private float timer;
    private float timerMax = 0.2f;

    public MenuAnimations anims;

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
    public string chosenNameBackup; // backup of the input field name in case of deletion

    public Button newPlayerBtn;

    [Header("GameObjects to keep track of the JSON file buttons")]
    public GameObject playerPrefab; // the object we copy for players
    public GameObject playerParent; // the placement of the copies original spot
    public List<GameObject> playerList; // list of created plyer buttons

    [Header("Where buttons go when changing sides")]
    public GameObject startButton; // the button that we start on
    public GameObject startOfEdit; // the button the editing starts on

    [Header("Sections of buttons")]
    public GameObject startingSection;
    public GameObject buttonEditSection;

    [Header("Buttons")]
    public ButtonControl button1;
    public ButtonControl button2;
    public ButtonControl button3;
    public ButtonControl button4;
    private string button1name;
    private string button2name;
    private string button3name;
    private string button4name;

    [Header("Images")]
    public GameObject button1Image;
    public GameObject button2Image;
    public GameObject button3Image;
    public GameObject button4Image;

    public enum State
    {
        start,
        edit,
        buttons
    }
    public State state;

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
        _gm.state = _GameManager.GameState.Profiles;
    }

    private void Update()
    {
        scrollValue.text = scrollSlider.value.ToString();
        switch (state)
        {
            case State.start:
                if (submit > 0 && timerMax <= timer)
                {
                    BackToOptions();
                    timer = 0;
                }
                else if (timerMax > timer)
                {
                    timer += Time.deltaTime;
                }
                break;

            case State.edit:
                if (submit > 0 && timerMax <= timer)
                {
                    SaveName();
                    GoToStartButton();
                    timer = 0;
                }
                else if (timerMax > timer)
                {
                    timer += Time.deltaTime;
                }
                break;

            case State.buttons:
                if (submit > 0 && timerMax <= timer)
                {
                    BackToEditing();
                    GoToStartOfEdit();
                    timer = 0;
                    button1Image.SetActive(false);
                    button2Image.SetActive(false);
                    button3Image.SetActive(false);
                    button4Image.SetActive(false);
                }
                else if (timerMax > timer)
                {
                    timer += Time.deltaTime;
                }
                if (button1 == null && timerMax <= timer)
                {
                    button1Image.SetActive(true);
                    var gamepad = Gamepad.current;
                    if (gamepad == null) return;

                    foreach (var control in gamepad.allControls)
                    {
                        if (control is ButtonControl button && button.wasPressedThisFrame)
                        {
                            button1 = button;
                            timer = 0;
                            button1Image.SetActive(false);
                            button2Image.SetActive(true);
                        }
                    }
                }
                else if (button2 == null && timerMax <= timer)
                {
                    button2Image.SetActive(true);
                    var gamepad = Gamepad.current;
                    if (gamepad == null) return;

                    foreach (var control in gamepad.allControls)
                    {
                        if (control is ButtonControl button && button.wasPressedThisFrame)
                        {
                            button2 = button;
                            timer = 0;
                            button2Image.SetActive(false);
                            button3Image.SetActive(true);
                        }
                    }
                }
                else if (button3 == null && timerMax <= timer)
                {
                    button3Image.SetActive(true);
                    var gamepad = Gamepad.current;
                    if (gamepad == null) return;

                    foreach (var control in gamepad.allControls)
                    {
                        if (control is ButtonControl button && button.wasPressedThisFrame)
                        {
                            button3 = button;
                            timer = 0;
                            button3Image.SetActive(false);
                            button4Image.SetActive(true);
                        }
                    }
                }
                else if (button4 == null && timerMax <= timer)
                {
                    button4Image.SetActive(true);
                    var gamepad = Gamepad.current;
                    if (gamepad == null) return;

                    foreach (var control in gamepad.allControls)
                    {
                        if (control is ButtonControl button && button.wasPressedThisFrame)
                        {
                            button4 = button;
                            timer = 0;
                            button4Image.SetActive(false);
                        }
                    }
                }
                else if (timerMax <= timer)
                {
                    BackToEditing();
                    GoToStartOfEdit();
                    timer = 0;
                }
                break;
        }
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
            button.name = button.name.Replace(".json", "");

            // if the name is one of the base classes, don't maake the button
            if (button.name.ToLower() == "beginner" || button.name.ToLower() == "seasoned" || button.name.ToLower() == "expert")
            {
                Destroy(button);
            }
            else
            {
                RectTransform rt = button.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, placement);
                placement -= 40; // Changes placement for the next button
                button.SetActive(true);
                Debug.Log(placement);

                // Changes the starting buttons navigation to go to the first created button
                if (playerList.Count == 0)
                {
                    nav = newPlayer.navigation;
                    nav.selectOnDown = btn;
                    newPlayer.navigation = nav;
                }
                if (Directory.GetFiles("JSON").Count() == 1)
                {
                    nav.selectOnDown = null;
                    nav.selectOnUp = newPlayerBtn;
                    btn.navigation = nav;
                }
                else if (!file.Equals(last)) // Changes every button except the last to have vertical navigation
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
        Debug.Log(button4);
        if (button4 == null)
        {
            button1name = "leftTrigger";
            button2name = "leftShoulder";
            button3name = "rightShoulder";
            button4name = "rightTrigger";
        }
        else
        {
            button1name = button1.name;
            button2name = button2.name;
            button3name = button3.name;
            button4name = button4.name;
        }
        if (chosenName.text.ToLower() != "name of profile" && chosenName.text.ToLower() != "beginner" && chosenName.text.ToLower() != "seasoned" && chosenName.text.ToLower() != "expert")
        {
            json.SavePlayer(player, scrollSpeed, assistTick, showButtons, button1name, button2name, button3name, button4name);
        }
    }

    // Deletes whatever JSON file shares a name with what is in the input field
    public void DeleteName()
    {
        if (File.Exists(chosenName.text))
        {
            File.Delete($"JSON/{chosenName.text}.json");
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
        state = State.start;
    }

    // Makes the selected object the editing starting button
    public void GoToStartOfEdit()
    {
        EventSystem.current.SetSelectedGameObject(startOfEdit);
        state = State.edit;
    }

    // Changes scenes back to options
    public void BackToOptions()
    {
        anims.scene = "Options";
        anims.PawStB();
    }

    public void NullButtons()
    {
        button1 = null;
        button2 = null;
        button3 = null;
        button4 = null;
    }

    public void StartButtonChange()
    {
        timer = 0;
        button1Image.SetActive(true);
        state = State.buttons;
        startingSection.SetActive(false);
        buttonEditSection.SetActive(true);
    }

    public void BackToEditing()
    {
        state = State.edit;
        startingSection.SetActive(true);
        buttonEditSection.SetActive(false);
    }

    public void LoadPlayerToPlayer(int number)
    {
        json.LoadPlayerToPlayer(player, number);
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public Button saveButton;

    [Header("Sections of buttons")]
    public GameObject startingSection;
    public GameObject buttonEditSection;

    [Header("Buttons")]
    public ButtonControl button1;
    public ButtonControl button2;
    public ButtonControl button3;
    public ButtonControl button4;
    public string button1name;
    public string button2name;
    public string button3name;
    public string button4name;

    [Header("Keys")]
    public KeyControl key1;
    public KeyControl key2;
    public KeyControl key3;
    public KeyControl key4;
    public KeyControl swipeLeft;
    public KeyControl swipeRight;
    public string key1name;
    public string key2name;
    public string key3name;
    public string key4name;
    public string swipeLeftname;
    public string swipeRightname;

    [Header("Images")]
    public GameObject button1Image;
    public GameObject button2Image;
    public GameObject button3Image;
    public GameObject button4Image;
    public GameObject swipeLeftImage;
    public GameObject swipeRightImage;

    EventSystem system;
    public GameObject lastSelectedGameObject;
    public GameObject currentSelectedGameObject_Recent;
    private GameObject lastSelected;

    public enum State
    {
        start,
        edit,
        buttons,
        keys
    }
    public State state;

    [field: HideInInspector]
    public Button newPlayer;

    private void Awake()
    {
        MakeButtons();
        AssistTick(); AssistTick();
        system = EventSystem.current;
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

    private void Start()
    {
        GoToStartButton();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (lastSelected != EventSystem.current.currentSelectedGameObject)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
        }
        if (lastSelected != EventSystem.current.currentSelectedGameObject)
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
        scrollValue.text = scrollSlider.value.ToString();
        GetLastGameObjectSelected();
        switch (state)
        {
            case State.start:
                foreach (PlayerScript player in _gm.players)
                {
                    submit = player.Submit();
                    if (submit > 0 && timerMax <= timer)
                    {
                        BackToOptions();
                        timer = 0;
                    }
                    else if (timerMax > timer)
                    {
                        timer += Time.deltaTime;
                    }
                }
                break;

            case State.edit:
                foreach (PlayerScript player in _gm.players)
                {
                    submit = player.Submit();
                    if (submit > 0 && timerMax <= timer)
                    {
                        saveButton.onClick.Invoke();
                        timer = 0;
                    }
                    else if (timerMax > timer)
                    {
                        timer += Time.deltaTime;
                    }
                }
                break;

            case State.buttons:
                foreach (PlayerScript player in _gm.players)
                {
                    submit = player.Submit();
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
                            if (button1.name == "up" || button1.name == "down" || button1.name == "left" || button1.name == "right")
                            {
                                button1 = null;
                                break;
                            }
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
                            if (button2.name == "up" || button2.name == "down" || button2.name == "left" || button2.name == "right" || button1 == button2)
                            {
                                button2 = null;
                                break;
                            }
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
                            if (button3.name == "up" || button3.name == "down" || button3.name == "left" || button3.name == "right" || button1 == button3 || button2 == button3)
                            {
                                button3 = null;
                                break;
                            }
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
                            if (button4.name == "up" || button4.name == "down" || button4.name == "left" || button4.name == "right" || button1 == button4 || button2 == button4 || button3 == button4)
                            {
                                button4 = null;
                                break;
                            }
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
            case State.keys:
                foreach (PlayerScript player in _gm.players)
                {
                    submit = player.Submit();
                    if (submit > 0 && timerMax <= timer)
                    {
                        BackToEditing();
                        GoToStartOfEdit();
                        timer = 0;
                        button1Image.SetActive(false);
                        button2Image.SetActive(false);
                        button3Image.SetActive(false);
                        button4Image.SetActive(false);
                        swipeLeftImage.SetActive(false);
                        swipeRightImage.SetActive(false);
                    }
                    else if (timerMax > timer)
                    {
                        timer += Time.deltaTime;
                    }
                }
                if (key1 == null && timerMax <= timer)
                {
                    button1Image.SetActive(true);

                    foreach (KeyControl key in Keyboard.current.allKeys)
                    {
                        if (key.wasPressedThisFrame)
                        {
                            key1 = key;
                            timer = 0;
                            if (key.keyCode == Key.Escape || key.keyCode == Key.Enter)
                            {
                                key1 = null;
                                break;
                            }
                            button1Image.SetActive(false);
                            button2Image.SetActive(true);
                        }
                    }

                }
                else if (key2 == null && timerMax <= timer)
                {
                    button2Image.SetActive(true);
                    foreach (KeyControl key in Keyboard.current.allKeys)
                    {
                        if (key.wasPressedThisFrame)
                        {
                            key2 = key;
                            timer = 0;
                            if (key.keyCode == Key.Escape || key.keyCode == Key.Enter || key.keyCode == key1.keyCode)
                            {
                                key2 = null;
                                break;
                            }
                            button2Image.SetActive(false);
                            button3Image.SetActive(true);
                        }
                    }
                }
                else if (key3 == null && timerMax <= timer)
                {
                    button3Image.SetActive(true);
                    foreach (KeyControl key in Keyboard.current.allKeys)
                    {
                        if (key.wasPressedThisFrame)
                        {
                            key3 = key;
                            timer = 0;
                            if (key.keyCode == Key.Escape || key.keyCode == Key.Enter || key.keyCode == key1.keyCode || key.keyCode == key2.keyCode)
                            {
                                key3 = null;
                                break;
                            }
                            button3Image.SetActive(false);
                            button4Image.SetActive(true);
                        }
                    }
                }
                else if (key4 == null && timerMax <= timer)
                {
                    button4Image.SetActive(true);
                    foreach (KeyControl key in Keyboard.current.allKeys)
                    {
                        if (key.wasPressedThisFrame)
                        {
                            key4 = key;
                            timer = 0;
                            if (key.keyCode == Key.Escape || key.keyCode == Key.Enter || key.keyCode == key1.keyCode || key.keyCode == key2.keyCode || key.keyCode == key3.keyCode)
                            {
                                key4 = null;
                                break;
                            }
                            button4Image.SetActive(false);
                            swipeLeftImage.SetActive(true);
                        }
                    }
                }
                else if (swipeLeft == null && timerMax <= timer)
                {
                    swipeLeftImage.SetActive(true);
                    foreach (KeyControl key in Keyboard.current.allKeys)
                    {
                        if (key.wasPressedThisFrame)
                        {
                            swipeLeft = key;
                            timer = 0;
                            if (key.keyCode == Key.Escape || key.keyCode == Key.Enter || key.keyCode == key1.keyCode || key.keyCode == key2.keyCode || key.keyCode == key3.keyCode || key.keyCode == key4.keyCode)
                            {
                                swipeLeft = null;
                                break;
                            }
                            swipeLeftImage.SetActive(false);
                            swipeRightImage.SetActive(true);
                        }
                    }
                }
                else if (swipeRight == null && timerMax <= timer)
                {
                    swipeRightImage.SetActive(true);
                    foreach (KeyControl key in Keyboard.current.allKeys)
                    {
                        if (key.wasPressedThisFrame)
                        {
                            swipeRight = key;
                            timer = 0;
                            if (key.keyCode == Key.Escape || key.keyCode == Key.Enter || key.keyCode == key1.keyCode || key.keyCode == key2.keyCode || key.keyCode == key3.keyCode || key.keyCode == key4.keyCode || key.keyCode == swipeLeft.keyCode)
                            {
                                swipeRight = null;
                                break;
                            }
                            swipeRightImage.SetActive(false);
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

    private void GetLastGameObjectSelected()
    {
        if (system.currentSelectedGameObject != currentSelectedGameObject_Recent)
        {
            lastSelectedGameObject = currentSelectedGameObject_Recent;
            currentSelectedGameObject_Recent = system.currentSelectedGameObject;
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
        Debug.Log(button4name);
        if (button4name == null)
        {
            button1name = "leftTrigger";
            button2name = "leftShoulder";
            button3name = "rightShoulder";
            button4name = "rightTrigger";
        }
        else
        {
            if (button4 != null)
            {
                button1name = button1.name;
                button2name = button2.name;
                button3name = button3.name;
                button4name = button4.name;
            }
        }
        if (key4name == null)
        {
            key1name = "S";
            key2name = "D";
            key3name = "K";
            key4name = "L";
            swipeLeftname = "Space";
            swipeRightname = "RightAlt";
        }
        else
        {
            if (key4 != null)
            {
                key1name = key1.keyCode.ToString().Replace("Digit", "");
                key2name = key2.keyCode.ToString().Replace("Digit", "");
                key3name = key3.keyCode.ToString().Replace("Digit", "");
                key4name = key4.keyCode.ToString().Replace("Digit", "");
                swipeLeftname = swipeLeft.keyCode.ToString().Replace("Digit", "");
                swipeRightname = swipeRight.keyCode.ToString().Replace("Digit", "");
            }
        }

        if (chosenName.text.ToLower() != "name of profile" && chosenName.text.ToLower() != "beginner" && chosenName.text.ToLower() != "seasoned" && chosenName.text.ToLower() != "expert" && chosenName.text.ToLower() != "" && !chosenName.text.ToLower().Contains(".json"))
        {
            json.SavePlayer(player, scrollSpeed, assistTick, showButtons, button1name, button2name, button3name, button4name, key1name, key2name, key3name, key4name, swipeLeftname, swipeRightname);
        }
        else
        {
            chosenName.text = "INVALID NAME";
        }
    }

    // Deletes whatever JSON file shares a name with what is in the input field
    public void DeleteName()
    {
        File.Delete($"JSON/{chosenName.text}.json".Replace("\u200B", ""));
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
        lastSelectedGameObject = null;
        EventSystem.current.SetSelectedGameObject(startButton);
        startButton.transform.localPosition = new Vector3(startButton.transform.localPosition.x, 0, startButton.transform.localPosition.z);
        newPlayerBtn.transform.localPosition = new Vector3(newPlayerBtn.transform.localPosition.x, -40, newPlayerBtn.transform.position.z);
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

    public void NullKeys()
    {
        key1 = null;
        key2 = null;
        key3 = null;
        key4 = null;
        swipeLeft = null;
        swipeRight = null;
    }

    public void StartButtonChange()
    {
        NullButtons();
        timer = 0;
        button1Image.SetActive(true);
        state = State.buttons;
        startingSection.SetActive(false);
        buttonEditSection.SetActive(true);
    }

    public void StartKeyChange()
    {
        NullKeys();
        timer = 0;
        button1Image.SetActive(true);
        state = State.keys;
        startingSection.SetActive(false);
        buttonEditSection.SetActive(true);
    }

    public void BackToEditing()
    {
        state = State.edit;
        startingSection.SetActive(true);
        buttonEditSection.SetActive(false);
    }

    public void NewProfileButton()
    {
        scrollSlider.value = 5;
        ScrollSpeed();
        assistTick = false;
        AssistTick();
        showButtons = false;
        ShowButtons();
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

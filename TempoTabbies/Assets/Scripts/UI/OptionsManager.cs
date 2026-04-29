using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    // Player movement, which is sent by the PlayerScript.cs Class
    [field: HideInInspector]
    public Vector2 moveAmount;
    [field: HideInInspector]
    public float clickValue;
    [field: HideInInspector]
    public float submitValue;

    [field: HideInInspector]
    public float audioOffsetFloat;

    // Other scripts
    private _GameManager gameManager;
    private JSON_Stuff json;
    public MenuAnimations anims;

    // The UI elements
    [Header("Every UI element in the shared options")]
    public Button backButton;
    public Button profiles;
    public Slider volumeSlider;
    public Button showButton;
    public Button audioOffset1;
    public Button audioOffset2;
    public Button assistTick;
    public Slider assistTickVolume;
    public Slider lineOpacity;
    public Button missRumble;
    public Button hitSound;
    public Slider hitSoundVolume;
    public Button audioTop;
    public Button gameplay;
    public Button profilesOption;
    public Button epilepsy;
    public GameObject audioMenu;
    public GameObject gameplayMenu;
    public GameObject profileMenu;
    public GameObject accessability;

    // gameObjects to show button are on or off
    [Header("The images for buttons, to see if they are false or true")]
    public Image assistTickConfirmation;
    public Image hitSoundConfirmation;
    public Image showButtonConfirmation;
    public Image missRumbleConfirmation;
    public Image epilepsyConfirmation;
    public Image movingBGImage;

    // Slider value text
    [Header("Number text in the settings")]
    public TextMeshProUGUI volumeValue;
    public TextMeshProUGUI audioOffsetValue;
    public TextMeshProUGUI assistTickVolumeValue;
    public TextMeshProUGUI lineOpacityValue;
    public TextMeshProUGUI hitSoundVolumeValue;

    private GameObject lastSelected;

    public enum SelectedMenu
    {
        Audio,
        Gameplay,
        Profiles
    }

    [Header("The current option state")]
    public SelectedMenu selectedMenu;

    void Awake()
    {
        gameManager = FindAnyObjectByType<_GameManager>();
        gameManager.state = _GameManager.GameState.Options;
        json = FindAnyObjectByType<JSON_Stuff>();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        // sets the sliders and buttons to the current values
        volumeSlider.value = gameManager.volume;
        audioOffsetFloat = gameManager.audioOffset;
        assistTickVolume.value = gameManager.assistTickVolume;
        hitSoundVolume.value = gameManager.hitSoundVolume;
        // just clicks buttons twice, so if true in gameManager, it goes false then back to true we do this so it can check what the button bools are in the gameManager
        AssistTick(); AssistTick(); HitSound(); HitSound(); ShowButtons(); ShowButtons(); OnEpilepsyClick(); OnEpilepsyClick(); OnMovingBGClick(); OnMovingBGClick(); OnMissRumbleClick(); OnMissRumbleClick();
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

        foreach (PlayerScript player in gameManager.players)
        {
            submitValue = player.Submit();
            if (submitValue > 0.1f)
            {
                json.SaveGameManager();

                backButton.onClick.Invoke();
            }
        }
        if ((int)volumeSlider.value != volumeSlider.value) { volumeSlider.value = (int)volumeSlider.value; }

        volumeValue.text = (volumeSlider.value).ToString();
        audioOffsetValue.text = ((int)audioOffsetFloat).ToString() + "ms";
        assistTickVolumeValue.text = ((int)(assistTickVolume.value * 10)).ToString();
        lineOpacityValue.text = ((int)(lineOpacity.value * 10)).ToString() + "%";
        hitSoundVolumeValue.text = ((int)(hitSoundVolume.value * 10)).ToString();

        AudioListener.volume = volumeSlider.value / 10;
        gameManager.volume = volumeSlider.value;
        gameManager.assistTickVolume = assistTickVolume.value;
        gameManager.lineOpacity = (int)lineOpacity.value;
        gameManager.hitSoundVolume = hitSoundVolume.value;
    }

    public void OnAudioOffset1Click()
    {
        audioOffsetFloat += 2;
        gameManager.audioOffset = audioOffsetFloat;
    }

    public void OnAudioOffset2Click()
    {
        audioOffsetFloat -= 2;
        gameManager.audioOffset = audioOffsetFloat;
    }

    public void OnReturnClick()
    {
        json.SaveGameManager();

        anims.scene = "MainMenu";
    }

    public void OnProfileClick()
    {
        anims.scene = "Profiles";
    }

    public void OnEpilepsyClick()
    {
        if (gameManager.epilepsy == true)
        {
            gameManager.epilepsy = false;
            epilepsyConfirmation.color = Color.softRed;
        }
        else
        {
            gameManager.epilepsy = true;
            epilepsyConfirmation.color = Color.limeGreen;
        }
    }

    public void OnMovingBGClick()
    {
        gameManager.movingBG = !gameManager.movingBG;
        if (gameManager.movingBG)
        {
            movingBGImage.color = Color.limeGreen;
        }
        else
        {
            movingBGImage.color = Color.softRed;
        }
    }

    public void OnMissRumbleClick()
    {
        gameManager.missRumble = !gameManager.missRumble;
        if (gameManager.missRumble)
        {
            missRumbleConfirmation.color = Color.limeGreen;
        }
        else
        {
            missRumbleConfirmation.color = Color.softRed;
        }
    }

    public void NoteOne(TextMeshProUGUI text)
    {
        if (text.text == "blue")
        {
            gameManager.noteOne = "blue";
        }
        else if (text.text == "red")
        {
            gameManager.noteOne = "red";
        }
        else if (text.text == "yellow")
        {
            gameManager.noteOne = "yellow";
        }
        else if (text.text == "green")
        {
            gameManager.noteOne = "green";
        }
        else if (text.text == "purple")
        {
            gameManager.noteOne = "purple";
        }
    }

    public void NoteTwo(TextMeshProUGUI text)
    {
        if (text.text == "blue")
        {
            gameManager.noteTwo = "blue";
        }
        else if (text.text == "red")
        {
            gameManager.noteTwo = "red";
        }
        else if (text.text == "yellow")
        {
            gameManager.noteTwo = "yellow";
        }
        else if (text.text == "green")
        {
            gameManager.noteTwo = "green";
        }
        else if (text.text == "purple")
        {
            gameManager.noteTwo = "purple";
        }
    }

    public void AssistTick()
    {
        if (gameManager.assistTick == true)
        {
            gameManager.assistTick = false;
            assistTickConfirmation.color = Color.softRed;
        }
        else
        {
            gameManager.assistTick = true;
            assistTickConfirmation.color = Color.limeGreen;
        }
    }

    public void HitSound()
    {
        if (gameManager.hitSound == true)
        {
            gameManager.hitSound = false;
            hitSoundConfirmation.color = Color.softRed;
        }
        else
        {
            gameManager.hitSound = true;
            hitSoundConfirmation.color = Color.limeGreen;
        }
    }

    public void OnAudioMenuClick(bool reselect)
    {
        if (reselect)
        {
            EventSystem.current.SetSelectedGameObject(volumeSlider.gameObject);
        }
        audioMenu.SetActive(true);
        gameplayMenu.SetActive(false);
        profileMenu.SetActive(false);
        accessability.SetActive(false);
        lastSelected = audioMenu;
    }
    public void OnGameplayMenuClick(bool reselect)
    {
        if (reselect)
        {
            EventSystem.current.SetSelectedGameObject(assistTick.gameObject);
        }
        audioMenu.SetActive(false);
        gameplayMenu.SetActive(true);
        profileMenu.SetActive(false);
        accessability.SetActive(false);
        lastSelected = gameplayMenu;
    }
    public void OnProfileMenuClick(bool reselect)
    {
        if (reselect)
        {
            EventSystem.current.SetSelectedGameObject(profiles.gameObject);
        }
        audioMenu.SetActive(false);
        gameplayMenu.SetActive(false);
        profileMenu.SetActive(true);
        accessability.SetActive(false);
        lastSelected = profileMenu;
    }

    public void OnAccessabilityClick(bool reselect)
    {
        if (reselect)
        {
            EventSystem.current.SetSelectedGameObject(epilepsy.gameObject);
        }
        audioMenu.SetActive(false);
        gameplayMenu.SetActive(false);
        profileMenu.SetActive(false);
        accessability.SetActive(true);
        lastSelected = accessability;
    }

    public void ShowButtons()
    {
        if (gameManager.showButtons == true)
        {
            gameManager.showButtons = false;
            showButtonConfirmation.color = Color.softRed;
        }
        else
        {
            gameManager.showButtons = true;
            showButtonConfirmation.color = Color.limeGreen;
        }
    }
}
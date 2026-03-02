using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public Slider scrollSpeed;
    public Button audioOffset1;
    public Button audioOffset2;
    public Button assistTick;
    public Slider assistTickVolume;
    public Button hitSound;
    public Slider hitSoundVolume;
    public Button audioTop;
    public Button gameplay;
    public Button profilesOption;
    public GameObject audioMenu;
    public GameObject gameplayMenu;
    public GameObject profileMenu;

    // gameObjects to show button are on or off
    [Header("The images for buttons, to see if they are false or true")]
    public Image assistTickConfirmation;
    public Image hitSoundConfirmation;
    public Image showButtonConfirmation;

    // Slider value text
    [Header("Number text in the settings")]
    public TextMeshProUGUI volumeValue;
    public TextMeshProUGUI scrollSpeedValue;
    public TextMeshProUGUI audioOffsetValue;
    public TextMeshProUGUI assistTickVolumeValue;
    public TextMeshProUGUI hitSoundVolumeValue;

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
        scrollSpeed.value = gameManager.scrollSpeed;
        audioOffsetFloat = gameManager.audioOffset;
        assistTickVolume.value = gameManager.assistTickVolume;
        hitSoundVolume.value = gameManager.hitSoundVolume;
        AssistTick(); AssistTick(); // just clicks them twice, so if true in gameManager, it goes false then back to true ->
        HitSound(); HitSound();     // we do this so it can check what the button bools are in the gameManager
        ShowButtons(); ShowButtons();
    }


    private void Update()
    {
        if (submitValue > 0.1f)
        {
            json.SaveGameManager();

            backButton.onClick.Invoke();
        }
        if ((int)volumeSlider.value != volumeSlider.value) { volumeSlider.value = (int)volumeSlider.value; }
        if ((int)scrollSpeed.value != scrollSpeed.value) { scrollSpeed.value = (int)scrollSpeed.value; }

        volumeValue.text = (volumeSlider.value).ToString();
        scrollSpeedValue.text = scrollSpeed.value.ToString();
        audioOffsetValue.text = ((int)audioOffsetFloat).ToString() + "ms";
        assistTickVolumeValue.text = ((int)(assistTickVolume.value * 10)).ToString();
        hitSoundVolumeValue.text = ((int)(hitSoundVolume.value * 10)).ToString();

        AudioListener.volume = volumeSlider.value / 10;
        gameManager.volume = volumeSlider.value;
        gameManager.scrollSpeed = scrollSpeed.value;
        gameManager.assistTickVolume = assistTickVolume.value;
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

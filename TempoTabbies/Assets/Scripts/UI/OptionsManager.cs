using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class OptionsManager : MonoBehaviour
{
    // Player movement, which is sent by the PlayerScript.cs Class
    [field: HideInInspector]
    public Vector2 moveAmount;
    [field: HideInInspector]
    public float clickValue;
    [field: HideInInspector]
    public float submitValue;
    public PlayerScript currentPlayer;

    [field: HideInInspector]
    public float audioOffsetFloat;

    // Other scripts
    private _GameManager gameManager;
    private JSON_Stuff json;

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

    // Audio
    AudioSource source;

    // Enum to check what is selected
    public enum Selected
    {
        backButton,
        profiles,
        volumeSlider,
        showButtons,
        scrollSpeed,
        audioOffset1,
        audioOffset2,
        assistTick,
        assistTickVolume,
        hitSound,
        hitSoundVolume
    }

    [Header("The current option state")]
    public Selected selected;

    // player movement timer
    bool canMove;
    float moveTimer;

    void Awake()
    {
        WaitAFrame();
        gameManager = FindAnyObjectByType<_GameManager>();
        json = FindAnyObjectByType<JSON_Stuff>();

        selected = Selected.backButton;

        // sets the sliders and buttons to the current values
        volumeSlider.value = gameManager.volume;
        scrollSpeed.value = gameManager.scrollSpeed;
        audioOffsetFloat = gameManager.audioOffset;
        assistTickVolume.value = gameManager.assistTickVolume;
        hitSoundVolume.value = gameManager.hitSoundVolume;
        AssistTick(); AssistTick(); // just clicks them twice, so if true in gameManager, it goes false then back to true ->
        HitSound(); HitSound();     // we do this so it can check what the button bools are in the gameManager
        ShowButtons(); ShowButtons();

        // Plays audio
        source = GetComponent<AudioSource>();
        source.Play();
        source.loop = true;
    }

    IEnumerator WaitAFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
    }


    private void Update()
    {
        if (submitValue > 0.1f)
        {
            json.SaveGameManager();
            if (gameManager.p1 != null)
            {
                json.SavePlayer1();
            }
            if (gameManager.p2 != null)
            {
                json.SavePlayer2();
            }

            gameManager.state = _GameManager.GameState.MainMenu;
            SceneManager.LoadScene("MainMenu");
        }
        if ((int)volumeSlider.value != volumeSlider.value) { volumeSlider.value = (int)volumeSlider.value; }
        if ((int)scrollSpeed.value != scrollSpeed.value) { scrollSpeed.value = (int)scrollSpeed.value; }

        volumeValue.text = ((int)(volumeSlider.value * 10)).ToString();
        scrollSpeedValue.text = scrollSpeed.value.ToString();
        audioOffsetValue.text = ((int)audioOffsetFloat).ToString() + "ms";
        assistTickVolumeValue.text = ((int)(assistTickVolume.value * 10)).ToString();
        hitSoundVolumeValue.text = ((int)(hitSoundVolume.value * 10)).ToString();
        switch (selected)
        {
            case Selected.backButton: // Back to menu
                                      // Selects the correct button
                EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.profiles;
                    canMove = false;
                }
                break;

            case Selected.profiles:
                EventSystem.current.SetSelectedGameObject(profiles.gameObject);
                if (clickValue > 0.1)
                {
                    OnReturnClick();
                }
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.backButton;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.volumeSlider;
                    canMove = false;
                }
                break;

            case Selected.volumeSlider: // The volume slider
                                        // Selects the slider
                EventSystem.current.SetSelectedGameObject(volumeSlider.gameObject);
                AudioListener.volume = volumeSlider.value;
                gameManager.volume = volumeSlider.value;
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.profiles;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.showButtons;
                    canMove = false;
                }
                break;

            case Selected.showButtons:
                EventSystem.current.SetSelectedGameObject(showButton.gameObject);
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.volumeSlider;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.scrollSpeed;
                    canMove = false;
                }
                break;
            case Selected.scrollSpeed: // The scroll speed slider
                EventSystem.current.SetSelectedGameObject(scrollSpeed.gameObject);
                gameManager.scrollSpeed = scrollSpeed.value;
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.showButtons;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.audioOffset1;
                    canMove = false;
                }
                break;

            case Selected.audioOffset1: // The audio offset slider
                EventSystem.current.SetSelectedGameObject(audioOffset1.gameObject);
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.scrollSpeed;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.audioOffset2;
                    canMove = false;
                }
                break;
            case Selected.audioOffset2: // The audio offset slider
                EventSystem.current.SetSelectedGameObject(audioOffset2.gameObject);
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.audioOffset1;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.assistTick;
                    canMove = false;
                }
                break;
            case Selected.assistTick:
                EventSystem.current.SetSelectedGameObject(assistTick.gameObject);
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.audioOffset2;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.assistTickVolume;
                    canMove = false;
                }
                break;

            case Selected.assistTickVolume:
                EventSystem.current.SetSelectedGameObject(assistTickVolume.gameObject);
                gameManager.assistTickVolume = assistTickVolume.value;
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.assistTick;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.hitSound;
                    canMove = false;
                }
                break;

            case Selected.hitSound:
                EventSystem.current.SetSelectedGameObject(hitSound.gameObject);
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.assistTickVolume;
                    canMove = false;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.hitSoundVolume;
                    canMove = false;
                }
                break;

            case Selected.hitSoundVolume:
                EventSystem.current.SetSelectedGameObject(hitSoundVolume.gameObject);
                gameManager.hitSoundVolume = hitSoundVolume.value;
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.hitSound;
                    canMove = false;
                }
                break;

        }

        if (!canMove)
        {
            if (moveTimer < 0.2f)
            {
                moveTimer += Time.deltaTime;
            }
            else
            {
                canMove = true;
                moveTimer = 0;
            }
        }
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

        gameManager.state = _GameManager.GameState.MainMenu;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnProfileClick()
    {
        gameManager.state = _GameManager.GameState.Profiles;
        SceneManager.LoadScene("Profiles");
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

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
    public PlayerScript currentPlayer;

    [field: HideInInspector]
    public float audioOffsetFloat;

    // Other scripts
    private _GameManager gameManager;
    private JSON_Stuff json;

    // The UI elements
    [Header("Every UI element in the shared options")]
    public Button backButton;
    public Button playerSpecific;
    public Slider volumeSlider;
    public Slider scrollSpeed;
    public Button audioOffset1;
    public Button audioOffset2;
    public Button assistTick;
    public Slider assistTickVolume;
    public Button hitSound;
    public Slider hitSoundVolume;

    // P1 specific
    [Header("Every UI element for player 1 options")]
    public Button buttonP1;
    public Slider scrollSpeedP1;
    public TextMeshProUGUI scrollSpeedValueP1;
    // P2 specific
    [Header("Every UI element for player 2 options")]
    public Button buttonP2;
    public Slider scrollSpeedP2;
    public TextMeshProUGUI scrollSpeedValueP2;

    // gameObjects to show button are on or off
    [Header("The images for buttons, to see if they are false or true")]
    public Image assistTickConfirmation;
    public Image hitSoundConfirmation;

    // Slider value text
    [Header("Number text in the settings")]
    public TextMeshProUGUI volumeValue;
    public TextMeshProUGUI scrollSpeedValue;
    public TextMeshProUGUI audioOffsetValue;
    public TextMeshProUGUI assistTickVolumeValue;
    public TextMeshProUGUI hitSoundVolumeValue;

    // The parent object of every UI item
    [Header("All the objects that contain everything")]
    public GameObject allOfIt;
    public GameObject allOfP1;
    public GameObject allOfP2;

    // Audio
    AudioSource source;

    // Enum to check what is selected
    public enum Selected
    {
        backButton,
        playerSpecific,
        volumeSlider,
        scrollSpeed,
        audioOffset1,
        audioOffset2,
        assistTick,
        assistTickVolume,
        hitSound,
        hitSoundVolume
    }

    public enum Player
    {
        none,
        p1,
        p2
    }

    [Header("The current option state")]
    public Selected selected;
    public Player player;

    // player movement timer
    bool canMove;
    float moveTimer;

    void Awake()
    {
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
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

        // Plays audio
        source = GetComponent<AudioSource>();
        source.Play();
        source.loop = true;
    }

    private void Update()
    {
        switch (player)
        {
            case Player.none:
                allOfIt.SetActive(true);
                allOfP1.SetActive(false);
                allOfP2.SetActive(false);
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
                        if (clickValue > 0.1)
                        {
                            OnReturnClick();
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.playerSpecific;
                            canMove = false;
                        }
                        break;

                    case Selected.playerSpecific:
                        EventSystem.current.SetSelectedGameObject(playerSpecific.gameObject);
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
                            selected = Selected.playerSpecific;
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
                            selected = Selected.volumeSlider;
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
                break;

            case Player.p1:
                allOfIt.SetActive(false);
                allOfP1.SetActive(true);
                allOfP2.SetActive(false);
                scrollSpeedValueP1.text = scrollSpeedP1.value.ToString();
                gameManager.whoGetsToPlay = 0;
                if (gameManager.p1.scrollSpeed != 0 && scrollSpeedP1.value == 0)
                {
                    scrollSpeedP1.value = gameManager.p1.scrollSpeed;
                }
                switch (selected)
                {
                    case Selected.backButton: // Back to menu
                                              // Selects the correct button
                        EventSystem.current.SetSelectedGameObject(buttonP1.gameObject);
                        if (clickValue > 0)
                        {
                            OnPlayerReturnClick();
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            canMove = false;
                        }
                        break;

                    case Selected.scrollSpeed: // The scroll speed slider
                        EventSystem.current.SetSelectedGameObject(scrollSpeedP1.gameObject);
                        gameManager.p1.scrollSpeed = scrollSpeedP1.value;
                        scrollSpeedValueP1.text = scrollSpeedP1.value.ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.backButton;
                            canMove = false;
                        }
                        break;
                }
                break;

            case Player.p2:
                allOfIt.SetActive(false);
                allOfP1.SetActive(false);
                allOfP2.SetActive(true);
                scrollSpeedValueP2.text = scrollSpeedP2.value.ToString();
                gameManager.whoGetsToPlay = 1;
                if (gameManager.p2.scrollSpeed != 0 && scrollSpeedP2.value == 0)
                {
                    scrollSpeedP2.value = gameManager.p2.scrollSpeed;
                }
                switch (selected)
                {
                    case Selected.backButton: // Back to menu
                                              // Selects the correct button
                        EventSystem.current.SetSelectedGameObject(buttonP2.gameObject);
                        if (clickValue > 0)
                        {
                            OnPlayerReturnClick();
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            canMove = false;
                        }
                        break;

                    case Selected.scrollSpeed: // The scroll speed slider
                        EventSystem.current.SetSelectedGameObject(scrollSpeedP2.gameObject);
                        gameManager.p2.scrollSpeed = scrollSpeedP2.value;
                        scrollSpeedValueP2.text = scrollSpeedP2.value.ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.backButton;
                            canMove = false;
                        }
                        break;
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

    public void OnPlayerSpecificClick()
    {
        if (currentPlayer.gameObject == gameManager.p1.gameObject)
        {
            player = Player.p1;
        }
        if (gameManager.p2 != null && currentPlayer.gameObject == gameManager.p2.gameObject)
        {
            player = Player.p2;
        }
    }

    public void OnPlayerReturnClick()
    {
        player = Player.none;
    }

    public void AssistTick()
    {
        if (gameManager.assistTick == false)
        {
            gameManager.assistTick = true;
            assistTickConfirmation.color = Color.softRed;
        }
        else
        {
            gameManager.assistTick = false;
            assistTickConfirmation.color = Color.limeGreen;
        }
    }

    public void HitSound()
    {
        if (gameManager.hitSound == false)
        {
            gameManager.hitSound = true;
            hitSoundConfirmation.color = Color.softRed;
        }
        else
        {
            gameManager.hitSound = false;
            hitSoundConfirmation.color = Color.limeGreen;
        }
    }
}

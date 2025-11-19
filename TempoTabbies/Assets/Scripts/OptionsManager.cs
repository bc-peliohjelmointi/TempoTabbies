using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class OptionsManager : MonoBehaviour
{
    // Player input values
    [field: HideInInspector]
    public Vector2 moveAmount;
    [field: HideInInspector]
    public float clickValue;

    // Other scripts
    private _GameManager gameManager;
    private JSON_Stuff json;

    // The UI elements
    [Header("Every UI element in the shared options")]
    public Button button1;
    public Slider volumeSlider;
    public Slider scrollSpeed;
    public Slider stickSensitivity;
    public Slider audioOffset;
    public Button assistTick;
    public Slider assistTickVolume;
    public Button hitSound;
    public Slider hitSoundVolume;
    public TMP_Dropdown noteColor;
    public Scrollbar scrollbar;

    // P! specific
    [Header("Every UI element for player 1 options")]
    public Button buttonP1;
    public Slider scrollSpeedP1;
    public Slider stickSensitivityP1;
    public TextMeshProUGUI scrollSpeedValueP1;
    public TextMeshProUGUI stickSensitivityValueP1;
    // P2 specific
    [Header("Every UI element for player 2 options")]
    public Button buttonP2;
    public Slider scrollSpeedP2;
    public Slider stickSensitivityP2;
    public TextMeshProUGUI scrollSpeedValueP2;
    public TextMeshProUGUI stickSensitivityValueP2;

    // gameObjects to show button are on or off
    [Header("The images for buttons, to see if they are false or true")]
    public Image assistTickConfirmation;
    public Image hitSoundConfirmation;
    // Slider value text
    [Header("Every piece of text in the settings")]
    public TextMeshProUGUI volumeValue;
    public TextMeshProUGUI scrollSpeedValue;
    public TextMeshProUGUI stickSensitivityValue;
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
        mouse,
        button1,
        volumeSlider,
        scrollSpeed,
        stickSensitivity,
        audioOffset,
        assistTick,
        assistTickVolume,
        hitSound,
        hitSoundVolume,
        noteColor
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
        EventSystem.current.SetSelectedGameObject(button1.gameObject);
        gameManager = FindAnyObjectByType<_GameManager>();
        json = FindAnyObjectByType<JSON_Stuff>();

        selected = Selected.button1;

        // sets the sliders and buttons to the current values
        volumeSlider.value = gameManager.volume;
        scrollSpeed.value = gameManager.scrollSpeed;
        stickSensitivity.value = gameManager.stickSensitivity;
        audioOffset.value = gameManager.audioOffset;
        assistTickVolume.value = gameManager.assistTickVolume;
        hitSoundVolume.value = gameManager.hitSoundVolume;
        AssistTick(); AssistTick(); // just clicks them twice, so if true in gameManager, it goes false then back to true ->
        HitSound(); HitSound();     // we do this so it can check what the button bools are in the game manager

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
                switch (selected)
                {
                    case Selected.mouse:
                        volumeValue.text = ((int)(volumeSlider.value * 10)).ToString();
                        scrollSpeedValue.text = ((int)(scrollSpeed.value * 10)).ToString();
                        stickSensitivityValue.text = ((int)(stickSensitivity.value * 10)).ToString();
                        audioOffsetValue.text = ((int)(audioOffset.value * 100)).ToString() + "ms";
                        assistTickVolumeValue.text = ((int)(assistTickVolume.value * 10)).ToString();
                        hitSoundVolumeValue.text = ((int)(hitSoundVolume.value * 10)).ToString();

                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.button1;
                            scrollbar.value = 0;
                            ScrollBar(0);
                            canMove = false;
                        }
                        break;

                    case Selected.button1: // Back to menu
                        // Selects the correct button
                        EventSystem.current.SetSelectedGameObject(button1.gameObject);
                        scrollbar.value = 0;
                        if (clickValue > 0)
                        {
                            // Add a button event here
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.volumeSlider;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.volumeSlider: // The volume slider
                        // Selects the slider
                        EventSystem.current.SetSelectedGameObject(volumeSlider.gameObject);
                        AudioListener.volume = volumeSlider.value;
                        gameManager.volume = volumeSlider.value;
                        volumeValue.text = ((int)(volumeSlider.value * 10)).ToString();
                        Debug.Log(moveAmount);
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.button1;
                            ScrollBar(-0.2f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.scrollSpeed: // The scroll speed slider
                        EventSystem.current.SetSelectedGameObject(scrollSpeed.gameObject);
                        gameManager.scrollSpeed = scrollSpeed.value;
                        scrollSpeedValue.text = ((int)(scrollSpeed.value * 10)).ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.volumeSlider;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.stickSensitivity;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.stickSensitivity: // The stick sensitivity slider
                        EventSystem.current.SetSelectedGameObject(stickSensitivity.gameObject);
                        gameManager.stickSensitivity = stickSensitivity.value;
                        stickSensitivityValue.text = ((int)(stickSensitivity.value * 10)).ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.audioOffset;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.audioOffset: // The audio offset slider
                        EventSystem.current.SetSelectedGameObject(audioOffset.gameObject);
                        gameManager.audioOffset = audioOffset.value;
                        audioOffsetValue.text = ((int)(audioOffset.value * 100)).ToString() + "ms";
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.stickSensitivity;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.assistTick;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.assistTick:
                        EventSystem.current.SetSelectedGameObject(assistTick.gameObject);
                        if (canMove && clickValue > 0)
                        {
                            AssistTick();
                            canMove = false;
                        }
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.audioOffset;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.assistTickVolume;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.assistTickVolume:
                        EventSystem.current.SetSelectedGameObject(assistTickVolume.gameObject);
                        gameManager.assistTickVolume = assistTickVolume.value;
                        assistTickVolumeValue.text = ((int)(assistTickVolume.value * 10)).ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.assistTick;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.hitSound;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.hitSound:
                        EventSystem.current.SetSelectedGameObject(hitSound.gameObject);
                        if (canMove && clickValue > 0)
                        {
                            HitSound();
                            canMove = false;
                        }
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.assistTickVolume;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.hitSoundVolume;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.hitSoundVolume:
                        EventSystem.current.SetSelectedGameObject(hitSoundVolume.gameObject);
                        gameManager.hitSoundVolume = hitSoundVolume.value;
                        hitSoundVolumeValue.text = ((int)(hitSoundVolume.value * 20)).ToString(); ;
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.hitSound;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.noteColor;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.noteColor: // The note color dropdown, possibly not being made
                        EventSystem.current.SetSelectedGameObject(noteColor.gameObject);
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.hitSoundVolume;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        break;
                }
                break;

            case Player.p1: // Remember to set scrolls peed as the selected object
                allOfIt.SetActive(false);
                allOfP1.SetActive(true);
                allOfP2.SetActive(false);
                if (gameManager.p1 == null)
                {
                    gameManager.p1 = gameManager.players[0];
                }
                if (gameManager.p1 != null)
                {
                    scrollSpeedP1.value = gameManager.p1.scrollSpeed;
                    scrollSpeedValueP1.text = (scrollSpeedP1.value * 10).ToString();

                    stickSensitivityP1.value = gameManager.p1.stickSensitivity;
                    stickSensitivityValueP1.text = (stickSensitivityP1.value * 10).ToString();
                }
                switch (selected)
                {
                    case Selected.mouse:
                        scrollSpeedValueP1.text = ((int)(scrollSpeedP1.value * 10)).ToString();
                        stickSensitivityValueP1.text = ((int)(stickSensitivityP1.value * 10)).ToString();
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.button1;
                            scrollbar.value = 0;
                            ScrollBar(0);
                            canMove = false;
                        }
                        break;
                    case Selected.button1: // Back to menu
                                           // Selects the correct button
                        EventSystem.current.SetSelectedGameObject(buttonP1.gameObject);
                        if (clickValue > 0)
                        {
                            // Add a button event here
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.scrollSpeed: // The scroll speed slider
                        EventSystem.current.SetSelectedGameObject(scrollSpeedP1.gameObject);
                        gameManager.p1.scrollSpeed = scrollSpeedP1.value;
                        scrollSpeedValueP1.text = (scrollSpeedP1.value * 10).ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.button1;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.stickSensitivity;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.stickSensitivity: // The stick sensitivity slider
                        EventSystem.current.SetSelectedGameObject(stickSensitivityP1.gameObject);
                        gameManager.p1.stickSensitivity = stickSensitivityP1.value;
                        stickSensitivityValueP1.text = (stickSensitivityP1.value * 10).ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        break;
                }
                break;

            case Player.p2:
                allOfIt.SetActive(false);
                allOfP1.SetActive(false);
                allOfP2.SetActive(true);
                if (gameManager.p2 == null)
                {
                    gameManager.p2 = gameManager.players[1];
                }
                if (gameManager.p2 != null)
                {
                    scrollSpeedP2.value = gameManager.p2.scrollSpeed;
                    scrollSpeedValueP2.text = (scrollSpeedP2.value * 10).ToString();

                    stickSensitivityP2.value = gameManager.p2.stickSensitivity;
                    stickSensitivityValueP2.text = (stickSensitivityP2.value * 10).ToString();
                }
                switch (selected)
                {
                    case Selected.mouse:
                        scrollSpeedValueP2.text = ((int)(scrollSpeedP2.value * 10)).ToString();
                        stickSensitivityValueP2.text = ((int)(stickSensitivityP2.value * 10)).ToString();
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.button1;
                            scrollbar.value = 0;
                            ScrollBar(0);
                            canMove = false;
                        }
                        break;
                    case Selected.button1: // Back to menu
                                           // Selects the correct button
                        EventSystem.current.SetSelectedGameObject(buttonP2.gameObject);
                        if (clickValue > 0)
                        {
                            // Add a button event here
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.scrollSpeed: // The scroll speed slider
                        EventSystem.current.SetSelectedGameObject(scrollSpeedP2.gameObject);
                        gameManager.p2.scrollSpeed = scrollSpeedP2.value;
                        scrollSpeedValueP2.text = (scrollSpeedP2.value * 10).ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.button1;
                            ScrollBar(-0.1f);
                            canMove = false;
                        }
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.stickSensitivity;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;

                    case Selected.stickSensitivity: // The stick sensitivity slider
                        EventSystem.current.SetSelectedGameObject(stickSensitivityP2.gameObject);
                        gameManager.p2.stickSensitivity = stickSensitivityP2.value;
                        stickSensitivityValueP2.text = (stickSensitivityP2.value * 10).ToString();
                        if (canMove && moveAmount.y > 0.1f)
                        {
                            selected = Selected.scrollSpeed;
                            ScrollBar(-0.1f);
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

    public void DropdownValueChanged(Color color)
    {
        // Figure this out later
    }

    public void OnReturnClick()
    {
        json.SaveGameManager();
        json.SavePlayer1();
        json.SavePlayer2();

        gameManager.state = _GameManager.GameState.MainMenu;
    }

    public void OnSelect(GameObject GO)
    {
        selected = Selected.mouse;
        EventSystem.current.SetSelectedGameObject(GO);
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

    public void ScrollBar(float value)
    {
        scrollbar.value += value;
        allOfIt.transform.localPosition = new Vector3(0, scrollbar.value * 100, 0);
    }
}

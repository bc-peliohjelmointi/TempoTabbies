using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Test the scrollbar with a controller
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class OptionsManager : MonoBehaviour
{
    // Player input values
    public Vector2 moveAmount;
    public float clickValue;

    // Other scripts
    private _GameManager _gameManager;

    // The UI elements
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
    public Slider scrollSpeedP1;
    public Slider stickSensitivityP1;
    public Button assistTickP1;
    public TextMeshProUGUI scrollSpeedValueP1;
    public TextMeshProUGUI stickSensitivityValueP1;
    // P2 specific
    public Slider scrollSpeedP2;
    public Slider stickSensitivityP2;
    public Button assistTickP2;
    public TextMeshProUGUI scrollSpeedValueP2;
    public TextMeshProUGUI stickSensitivityValueP2;

    // gameObjects to show button are on or off
    public Image assistTickConfirmation;
    public Image hitSoundConfirmation;
    // Slider value text
    public TextMeshProUGUI volumeValue;
    public TextMeshProUGUI scrollSpeedValue;
    public TextMeshProUGUI stickSensitivityValue;
    public TextMeshProUGUI audioOffsetValue;
    public TextMeshProUGUI assistTickVolumeValue;
    public TextMeshProUGUI hitSoundVolumeValue;

    // The parent object of every UI item
    public GameObject allOfIt;
    public GameObject allOfP1;
    public GameObject allOfP2;

    // Audio
    AudioSource source;


    List<PlayerScript> players;

    // Enum to check what is selected
    public enum Selected
    {
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
    public Selected selected;

    public enum Player
    {
        none,
        p1,
        p2
    }
    public Player player;

    // player movement timer
    bool canMove;
    float moveTimer;

    void Awake()
    {
        EventSystem.current.SetSelectedGameObject(button1.gameObject);
        _gameManager = FindAnyObjectByType<_GameManager>();

        // sets the sliders and buttons to the current values
        volumeSlider.value = _gameManager.volume;
        scrollSpeed.value = _gameManager.scrollSpeed;
        stickSensitivity.value = _gameManager.stickSensitivity;
        audioOffset.value = _gameManager.audioOffset;
        assistTickVolume.value = _gameManager.assistTickVolume;
        hitSoundVolume.value = _gameManager.hitSoundVolume;
        AssistTick(); AssistTick(); // just clicks them twice, so if true, it goes false then back to true, we do this so it can check what the button bools are in the game manager
        HitSound(); HitSound();

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

                switch (selected)
                {
                    case Selected.button1: // Back to menu
                                           // Selects the correct button
                        EventSystem.current.SetSelectedGameObject(button1.gameObject);
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
                        _gameManager.volume = volumeSlider.value;
                        volumeValue.text = (volumeSlider.value * 10).ToString();
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
                        _gameManager.scrollSpeed = scrollSpeed.value;
                        scrollSpeedValue.text = (scrollSpeed.value * 10).ToString();
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
                        _gameManager.stickSensitivity = stickSensitivity.value;
                        stickSensitivityValue.text = (stickSensitivity.value * 10).ToString();
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
                        _gameManager.audioOffset = audioOffset.value;
                        audioOffsetValue.text = (audioOffset.value * 10).ToString();
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
                        _gameManager.assistTickVolume = assistTickVolume.value;
                        assistTickVolumeValue.text = (assistTickVolume.value * 10).ToString();
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
                        _gameManager.hitSoundVolume = hitSoundVolume.value;
                        hitSoundVolumeValue.text = (hitSoundVolume.value * 10).ToString();
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

            case Player.p1:

                switch (selected)
                {
                    case Selected.scrollSpeed: // The scroll speed slider
                        EventSystem.current.SetSelectedGameObject(scrollSpeed.gameObject);
                        _gameManager.scrollSpeed = scrollSpeed.value;
                        scrollSpeedValue.text = (scrollSpeed.value * 10).ToString();
                        if (canMove && moveAmount.y < -0.1f)
                        {
                            selected = Selected.stickSensitivity;
                            ScrollBar(0.1f);
                            canMove = false;
                        }
                        break;
                }
                break;

            case Player.p2:
                switch (selected)
                {

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

    public void AssistTick()
    {
        if (_gameManager.assistTick == false)
        {
            _gameManager.assistTick = true;
            assistTickConfirmation.color = Color.black;
        }
        else
        {
            _gameManager.assistTick = false;
            assistTickConfirmation.color = Color.white;
        }
    }

    public void HitSound()
    {
        if (_gameManager.hitSound == false)
        {
            _gameManager.hitSound = true;
            hitSoundConfirmation.color = Color.black;
        }
        else
        {
            _gameManager.hitSound = false;
            hitSoundConfirmation.color = Color.white;
        }
    }

    public void ScrollBar(float value)
    {
        scrollbar.value += value;
        allOfIt.transform.localPosition = new Vector3(0, scrollbar.value * 100, 0);
    }
}

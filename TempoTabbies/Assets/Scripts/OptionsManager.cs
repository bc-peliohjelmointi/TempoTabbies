using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static _GameManager;

/// <summary>
/// Test the scrollbar with a controller
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class OptionsManager : MonoBehaviour
{
    // Player input values
    public Vector2 moveAmount;
    public float clickValue;
    public Vector2 scrollVallue;

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

    // Audio
    AudioSource source;

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
    public Stack<Selected> selectedStack = new Stack<Selected>();

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
        AssistTick(); AssistTick(); // just clicks them twice, so if true, it goes false then back to true
        HitSound(); HitSound();

        // Plays audio
        source = GetComponent<AudioSource>();
        source.Play();
        source.loop = true;
    }

    private void Update()
    {
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
                }
                break;

            case Selected.volumeSlider: // The volume slider
                // Selects the slider
                EventSystem.current.SetSelectedGameObject(volumeSlider.gameObject);
                AudioListener.volume = volumeSlider.value;
                _gameManager.volume = volumeSlider.value; 
                volumeValue.text = (volumeSlider.value * 10).ToString();
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.button1;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.scrollSpeed;
                }
                break;

            case Selected.scrollSpeed: // The scroll speed slider
                EventSystem.current.SetSelectedGameObject(scrollSpeed.gameObject);
                _gameManager.scrollSpeed = scrollSpeed.value;
                scrollSpeedValue.text = (scrollbar.value * 10).ToString(); 
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.volumeSlider;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.stickSensitivity;
                }
                break;

            case Selected.stickSensitivity: // The stick sensitivity slider
                EventSystem.current.SetSelectedGameObject(stickSensitivity.gameObject);
                _gameManager.stickSensitivity = stickSensitivity.value;
                stickSensitivityValue.text = (stickSensitivity.value * 10).ToString();
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.scrollSpeed;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.audioOffset;
                }
                break;

            case Selected.audioOffset: // The audio offset slider
                EventSystem.current.SetSelectedGameObject(audioOffset.gameObject);
                _gameManager.audioOffset = audioOffset.value;
                audioOffsetValue.text = (audioOffset.value * 10).ToString();
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.stickSensitivity;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.assistTick;
                }
                break;

            case Selected.assistTick:
                EventSystem.current.SetSelectedGameObject(assistTick.gameObject);
                if (clickValue > 0)
                {
                    AssistTick();
                }
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.audioOffset;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.hitSound;
                }
                break;

            case Selected.assistTickVolume:
                EventSystem.current.SetSelectedGameObject(assistTickVolume.gameObject);
                _gameManager.assistTickVolume = assistTickVolume.value;
                assistTickVolumeValue.text = (assistTickVolume.value * 10).ToString();
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.assistTick;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.hitSound;
                }
                break;

            case Selected.hitSound:
                EventSystem.current.SetSelectedGameObject(hitSound.gameObject);
                if (clickValue > 0)
                {
                    HitSound();
                }
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.assistTickVolume;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.noteColor;
                }
                break;

            case Selected.hitSoundVolume:
                EventSystem.current.SetSelectedGameObject(hitSoundVolume.gameObject);
                _gameManager.hitSoundVolume = hitSoundVolume.value;
                hitSoundVolumeValue.text = (hitSoundVolume.value * 10).ToString();
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.hitSound;
                }
                if (canMove && moveAmount.y < -0.1f)
                {
                    selected = Selected.noteColor;
                }
                break;

            case Selected.noteColor: // The note color dropdown
                EventSystem.current.SetSelectedGameObject(noteColor.gameObject);
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.audioOffset;
                }
                break;
        }

        if (scrollVallue.y > 0.1f)
        {
            scrollbar.value += 1;
            ScrollBar();
        }
        else if (scrollVallue.y < -0.1f)
        {
            scrollbar.value -= 1;
            ScrollBar();
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

    public void ScrollBar()
    {
        allOfIt.transform.localPosition = new Vector3(0, scrollbar.value*100, 0);
    }
}

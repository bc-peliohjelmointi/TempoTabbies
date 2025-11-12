using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class OptionsManager : MonoBehaviour
{
    // Player input values
    public Vector2 moveAmount;
    public float clickValue;

    // The button and slider
    public Button button1;
    public Slider volumeSlider;

    // Audio
    AudioSource source;

    // Enum to check what is selected
    public enum Selected
    {
        button1,
        volumeSlider
    }
    public Selected selected;

    // player movement timer
    bool canMove;
    float moveTimer;

    void Awake()
    {
        EventSystem.current.SetSelectedGameObject(button1.gameObject);
        volumeSlider.value = 1;
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
                if (canMove && moveAmount.y > 0.1f)
                {
                    selected = Selected.button1;
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
}

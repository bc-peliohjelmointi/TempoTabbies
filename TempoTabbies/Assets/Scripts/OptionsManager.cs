using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    // Player input values
    public Vector2 moveAmount;
    public float clickValue;

    // The button and slider
    public Button button1;
    public Slider volumeSlider;

    // Enum to check what is selected
    public enum Selected
    {
        button1,
        volumeSlider
    }
    public Selected selected;
    bool canMove;
    float moveTimer;

    void Awake()
    {
        EventSystem.current.SetSelectedGameObject(button1.gameObject);
    }

    private void Update()
    {
        switch (selected)
        {
            case Selected.button1:
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

            case Selected.volumeSlider:
                // Selects the slider
                EventSystem.current.SetSelectedGameObject(volumeSlider.gameObject);
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

    // Changes the global volume to be equal to the sliders value
    public void VolumeChange()
    {
        AudioListener.volume = volumeSlider.value;
    }
}

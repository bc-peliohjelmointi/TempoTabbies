using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SongButton : MonoBehaviour
{
    public TMP_Text TitleText;
    public TMP_Text ArtistText;
    public Transform ChartListParent;
    public GameObject ChartButtonPrefab;

    private SMFile currentSong;
    private ChartSelectManager manager;
    private bool chartsVisible = false;
    private GameObject previouslySelectedObject;

    void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ToggleCharts);
        }
    }

    void Update()
    {
        // Handle B button press to collapse when charts are expanded
        if (chartsVisible && Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            CollapseCharts();
        }
    }

    public void Setup(SMFile sm, ChartSelectManager mgr)
    {
        currentSong = sm;
        manager = mgr;

        TitleText.text = sm.Title;
        ArtistText.text = sm.Artist;

        // Create chart buttons but hide them initially
        foreach (var chart in sm.Charts)
        {
            GameObject btnObj = Instantiate(ChartButtonPrefab, ChartListParent);
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TMP_Text>();
            txt.text = $"{chart.Difficulty} ({chart.Meter})";

            SMChart currentChart = chart;
            btn.onClick.AddListener(() => OnChartSelected(currentChart));
        }

        // Hide charts initially
        if (ChartListParent != null)
        {
            ChartListParent.gameObject.SetActive(false);
        }
    }

    public void ToggleCharts()
    {
        if (!chartsVisible)
        {
            ExpandCharts();
        }
        else
        {
            CollapseCharts();
        }
    }

    private void ExpandCharts()
    {
        if (ChartListParent == null) return;

        // Store what was previously selected
        if (EventSystem.current != null)
        {
            previouslySelectedObject = EventSystem.current.currentSelectedGameObject;
        }

        chartsVisible = true;
        ChartListParent.gameObject.SetActive(true);

        // Select first chart button (or the only one for single charts)
        if (ChartListParent.childCount > 0)
        {
            Button firstChartButton = ChartListParent.GetChild(0).GetComponent<Button>();
            if (firstChartButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(firstChartButton.gameObject);
            }
        }

        // Disable other song buttons to prevent selection
        manager.SetOtherSongButtonsInteractable(false, this);
    }

    private void CollapseCharts()
    {
        chartsVisible = false;
        if (ChartListParent != null)
        {
            ChartListParent.gameObject.SetActive(false);
        }

        // Re-enable other song buttons
        manager.SetOtherSongButtonsInteractable(true, this);

        // Reselect this song button or the previously selected object
        if (EventSystem.current != null)
        {
            if (previouslySelectedObject != null && previouslySelectedObject != this.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(previouslySelectedObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
            }
        }
    }

    private void OnChartSelected(SMChart chart)
    {
        manager.OnChartSelected(currentSong, chart);
    }

    public Transform ChartListParentAccessor => ChartListParent;
    public bool ChartsVisible => chartsVisible;
}
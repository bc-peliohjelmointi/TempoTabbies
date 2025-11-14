using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SongButton : MonoBehaviour
{
    public TMP_Text TitleText;
    public TMP_Text ArtistText;
    public Transform ChartListParent;
    public GameObject ChartButtonPrefab;
    public Image bannerImage; // Add this reference
    public Sprite defaultBannerSprite; // Set this in inspector

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

        // Load the banner image
        LoadBanner(sm.Banner);

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

    private void LoadBanner(string bannerPath)
    {
        if (bannerImage == null) return;

        if (!string.IsNullOrEmpty(bannerPath))
        {
            // Construct full path to banner
            string fullBannerPath = Path.Combine(currentSong.DirectoryPath, bannerPath);
            StartCoroutine(LoadBannerCoroutine(fullBannerPath));
        }
        else
        {
            bannerImage.sprite = defaultBannerSprite;
        }
    }

    private IEnumerator LoadBannerCoroutine(string fullBannerPath)
    {
        if (!File.Exists(fullBannerPath))
        {
            Debug.LogWarning($"Banner file not found: {fullBannerPath}");
            bannerImage.sprite = defaultBannerSprite;
            yield break;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(fullBannerPath);
            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(fileData))
            {
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                bannerImage.sprite = sprite;
            }
            else
            {
                bannerImage.sprite = defaultBannerSprite;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading banner: {e.Message}");
            bannerImage.sprite = defaultBannerSprite;
        }

        yield return null;
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
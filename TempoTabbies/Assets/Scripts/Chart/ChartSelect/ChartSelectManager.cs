using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChartSelectManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform SongListParent;
    public GameObject SongButtonPrefab;
    public GameObject InitialButton;
    public ScrollRect songsScrollRect;

    [Header("Song Folder")]
    public string SongsFolder = "Songs";

    private List<SMFile> loadedSongs = new();
    private List<SongButton> songButtons = new();
    private SongButton currentlyExpandedSong = null;
    private List<Selectable> allSelectables = new();
    private GameObject lastSelectedObject;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(InitialButton);
        lastSelectedObject = InitialButton;
        LoadAllSongs();
        PopulateSongList();
    }

    void LoadAllSongs()
    {
        string fullPath = Path.Combine(Application.dataPath, SongsFolder);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError("Songs folder not found at: " + fullPath);
            return;
        }

        foreach (string dir in Directory.GetDirectories(fullPath))
        {
            foreach (string smFile in Directory.GetFiles(dir, "*.sm"))
            {
                SMFile sm = SMParser.Parse(smFile);
                if (sm != null && sm.Charts.Count > 0)
                {
                    loadedSongs.Add(sm);
                }
            }
        }

        Debug.Log($"Loaded {loadedSongs.Count} song(s).");
    }

    void PopulateSongList()
    {
        songButtons.Clear();
        allSelectables.Clear();

        foreach (var sm in loadedSongs)
        {
            GameObject buttonObj = Instantiate(SongButtonPrefab, SongListParent);
            SongButton btn = buttonObj.GetComponent<SongButton>();

            if (btn != null)
            {
                btn.Setup(sm, this);
                songButtons.Add(btn);

                Selectable songSelectable = buttonObj.GetComponent<Selectable>();
                if (songSelectable != null)
                {
                    allSelectables.Add(songSelectable);
                }
            }
        }
    }

    public void SetOtherSongButtonsInteractable(bool interactable, SongButton exceptThisOne)
    {
        List<Selectable> allowedSelectables = new List<Selectable>();

        if (!interactable)
        {
            Selectable expandedSelectable = exceptThisOne.GetComponent<Selectable>();
            if (expandedSelectable != null)
            {
                allowedSelectables.Add(expandedSelectable);
            }

            if (exceptThisOne.ChartListParentAccessor != null)
            {
                foreach (Transform child in exceptThisOne.ChartListParentAccessor)
                {
                    Selectable chartSelectable = child.GetComponent<Selectable>();
                    if (chartSelectable != null)
                    {
                        allowedSelectables.Add(chartSelectable);
                    }
                }
            }

            currentlyExpandedSong = exceptThisOne;
        }
        else
        {
            foreach (var songButton in songButtons)
            {
                Selectable songSelectable = songButton.GetComponent<Selectable>();
                if (songSelectable != null)
                {
                    allowedSelectables.Add(songSelectable);
                }
            }
            currentlyExpandedSong = null;
        }

        UpdateNavigation(allowedSelectables);
    }

    private void UpdateNavigation(List<Selectable> allowedSelectables)
    {
        foreach (var selectable in allSelectables)
        {
            if (selectable != null)
            {
                Navigation nav = selectable.navigation;
                nav.mode = allowedSelectables.Contains(selectable) ? Navigation.Mode.Automatic : Navigation.Mode.None;
                selectable.navigation = nav;
            }
        }

        foreach (var songButton in songButtons)
        {
            if (songButton.ChartListParentAccessor != null)
            {
                foreach (Transform child in songButton.ChartListParentAccessor)
                {
                    Selectable chartSelectable = child.GetComponent<Selectable>();
                    if (chartSelectable != null)
                    {
                        Navigation nav = chartSelectable.navigation;
                        nav.mode = allowedSelectables.Contains(chartSelectable) ? Navigation.Mode.Automatic : Navigation.Mode.None;
                        chartSelectable.navigation = nav;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (currentlyExpandedSong != null && EventSystem.current != null)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != null && !IsSelectableAllowed(currentSelected))
            {
                GameObject firstAllowed = FindFirstAllowedSelectable();
                if (firstAllowed != null)
                {
                    EventSystem.current.SetSelectedGameObject(firstAllowed);
                }
            }
        }

        if (EventSystem.current != null && songsScrollRect != null)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected != null && currentSelected != lastSelectedObject)
            {
                ScrollToSelectedItem(currentSelected);
                lastSelectedObject = currentSelected;
            }
        }
    }

    private void ScrollToSelectedItem(GameObject selectedObject)
    {
        if (songsScrollRect == null || selectedObject == null) return;

        RectTransform selectedRect = selectedObject.GetComponent<RectTransform>();
        if (selectedRect == null) return;

        // Wait until end of frame for layout to update, then scroll
        StartCoroutine(ScrollToSelectedItemCoroutine(selectedRect));
    }

    private System.Collections.IEnumerator ScrollToSelectedItemCoroutine(RectTransform selectedRect)
    {
        yield return new WaitForEndOfFrame();

        if (songsScrollRect == null || selectedRect == null) yield break;

        Canvas.ForceUpdateCanvases();

        RectTransform content = songsScrollRect.content;
        if (content == null) yield break;

        // Get the position of the selected item relative to the content
        Vector3 selectedLocalPos = content.InverseTransformPoint(selectedRect.position);
        float selectedPositionY = selectedLocalPos.y;

        // Get the height of the selected item and viewport
        float selectedHeight = selectedRect.rect.height;
        float viewportHeight = songsScrollRect.viewport.rect.height;
        float contentHeight = content.rect.height;

        // Calculate the normalized position
        // We want the selected item to be in the middle of the viewport
        float targetPosition = -selectedPositionY + (selectedHeight / 2) - (viewportHeight / 2);

        // Convert to normalized position (0 = top, 1 = bottom)
        float normalizedPosition = 1 - (targetPosition / (contentHeight - viewportHeight));

        // Clamp between 0 and 1
        normalizedPosition = Mathf.Clamp01(normalizedPosition);

        // Apply to scrollbar
        songsScrollRect.verticalNormalizedPosition = normalizedPosition;
    }

    private bool IsSelectableAllowed(GameObject obj)
    {
        if (currentlyExpandedSong == null) return true;
        return obj.transform.IsChildOf(currentlyExpandedSong.transform);
    }

    private GameObject FindFirstAllowedSelectable()
    {
        if (currentlyExpandedSong == null) return null;

        if (currentlyExpandedSong.ChartListParentAccessor != null &&
            currentlyExpandedSong.ChartListParentAccessor.childCount > 0)
        {
            return currentlyExpandedSong.ChartListParentAccessor.GetChild(0).gameObject;
        }

        return currentlyExpandedSong.gameObject;
    }

    public void OnChartSelected(SMFile song, SMChart chart)
    {
        PlayerPrefs.SetString("SelectedSongPath", song.Title);
        GameSession.SelectedSong = song;
        GameSession.SelectedChart = chart;
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChartTest");
    }
}
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChartSelectManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform SongListParent;
    public GameObject SongButtonPrefab;
    public ScrollRect songsScrollRect;
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;
    public MenuAnimations anims;

    [Header("Song Folder")]
    public string SongsFolder = "Songs";

    private List<SMFile> loadedSongs = new();
    private List<SongButton> songButtons = new();
    private SongButton currentlyExpandedSong = null;
    private List<Selectable> allSelectables = new();
    private GameObject lastSelectedObject;
    private GameObject lastHoveredSelectable;

    public _GameManager _gm;
    public GameObject scoreImages;

    // [field: HideInInspector]
    public float submitValue;
    public float timer;
    private float timerMax = 0.5f;

    public enum State
    {
        bigButton,
        smallButton
    }
    public State state;

    // if player 1 has selected
    private bool waitingForSecondPlayer = false;

    void Start()
    {
        _gm = FindFirstObjectByType<_GameManager>();
        LoadAllSongs();
        PopulateSongList();
        _gm.EnableControllers();
        _gm.state = _GameManager.GameState.StageSelect;
        _gm.source.volume = 100;
        if (_gm.multiplayer)
        {
            p1ScoreText.text = _gm.p1Score.ToString();
            p2ScoreText.text = _gm.p2Score.ToString();
        }
        else
        {
            p1ScoreText.gameObject.SetActive(false);
            p2ScoreText.gameObject.SetActive(false);
        }
        if (!_gm.multiplayer)
        {
            scoreImages.SetActive(false);
        }
        else
        {
            scoreImages.SetActive(true);
        }
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
                btn.name = $"SongButton_{sm.Title}";
                btn.Setup(sm, this);
                songButtons.Add(btn);

                Selectable songSelectable = buttonObj.GetComponent<Selectable>();
                if (songSelectable != null)
                {
                    allSelectables.Add(songSelectable);
                }
            }
        }

        lastSelectedObject = songButtons[0].transform.GetChild(0).gameObject;
        if (_gm.savedButtonName == "")
        {
            EventSystem.current.SetSelectedGameObject(songButtons[0].transform.GetChild(0).gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(GameObject.Find(_gm.savedButtonName).transform.GetChild(0).gameObject);
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
                nav.mode = allowedSelectables.Contains(selectable) ? Navigation.Mode.Vertical : Navigation.Mode.None;
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
                        nav.mode = allowedSelectables.Contains(chartSelectable) ? Navigation.Mode.Vertical : Navigation.Mode.None;
                        chartSelectable.navigation = nav;
                    }
                }
            }
        }
    }

    void Update()
    {
        // Handle controller/keyboard selection hover: show score popup when a chart button is selected
        if (EventSystem.current != null)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected != lastHoveredSelectable)
            {
                // Hide previous hover
                if (lastHoveredSelectable != null)
                {
                    var prevHover = lastHoveredSelectable.GetComponentInParent<ChartButtonHover>();
                    if (prevHover != null) prevHover.HoverExit();
                    else
                    {
                        var mgr = ScorePopupManager.Instance ?? FindFirstObjectByType<ScorePopupManager>();
                        if (mgr != null) mgr.Hide();
                    }
                }

                // Show new hover
                if (currentSelected != null)
                {
                    var newHover = currentSelected.GetComponentInParent<ChartButtonHover>();
                    if (newHover != null) newHover.HoverEnter();
                }

                lastHoveredSelectable = currentSelected;
            }
        }
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

        if (state == State.bigButton)
        {
            foreach (PlayerScript player in _gm.players)
            {
                submitValue = player.Submit();
                if (submitValue > 0 && timerMax <= timer)
                {
                    anims.scene = "MainMenu";
                    anims.PawStB();
                    timer = 0;
                }
                else if (timerMax > timer)
                {
                    timer += Time.deltaTime;
                }
            }
        }
        else if (state == State.smallButton)
        {
            foreach (PlayerScript player in _gm.players)
            {
                submitValue = player.Submit();
                if (submitValue > 0 && timerMax <= timer)
                {
                    _gm.EnableControllers();
                    state = State.bigButton;
                    EventSystem.current.SetSelectedGameObject(GameObject.Find(_gm.savedButtonName).transform.GetChild(0).gameObject);
                    timer = 0;
                }
                else if (timerMax > timer)
                {
                    timer += Time.deltaTime;
                }
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
        // Save last selection for debugging / persistence
        PlayerPrefs.SetString("SelectedSongPath", song.Title);

        // If multiplayer is off, behave exactly as before
        if (_gm == null)
        {
            _gm = FindFirstObjectByType<_GameManager>();
        }
        _gm.EnableControllers();
        if (!_gm.multiplayer)
        {
            GameSession.SelectedSong = song;
            GameSession.SelectedChart = chart;
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameSingleplayer");
            _gm.source.volume = 0;
            return;
        }

        // first selection goes to player 1, second to player 2
        if (!waitingForSecondPlayer)
        {
            GameSession.SelectedSongP1 = song;
            GameSession.SelectedChartP1 = chart;
            waitingForSecondPlayer = true;

            Debug.Log($"Player 1 selected: {song.Title} / {chart.Description}. Waiting for Player 2 to choose.");
            // tee tahan jotain mika indikoi etta pelaaja 2 vuoro
            return;
        }

        GameSession.SelectedSongP2 = song;
        GameSession.SelectedChartP2 = chart;
        waitingForSecondPlayer = false;

        _gm.EnableControllers();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MultiPlayerChartTest");
        _gm.source.volume = 0;
    }
}
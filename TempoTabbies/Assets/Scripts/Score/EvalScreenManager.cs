using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EvalScreenManager : MonoBehaviour
{
    [Header("Song Info Display")]
    public Image bannerImage;
    public Image backgroundImage;
    public Image backgroundImage2;
    public TMP_Text songTitleText;
    public TMP_Text artistText;
    public TMP_Text difficultyText;
    public TMP_Text meterText;

    [Header("Song Info (Player 2) - assign when using multiplayer")]
    public TMP_Text difficultyTextP2;
    public TMP_Text meterTextP2;
    public Image difficultyBackgroundP2;

    [Header("Score Display (Player 1)")]
    public TMP_Text scoreText;
    public TMP_Text gradeText;
    public TMP_Text comboText;
    public TMP_Text marvelousText;
    public TMP_Text perfectText;
    public TMP_Text greatText;
    public TMP_Text goodText;
    public TMP_Text badText;
    public TMP_Text missText;
    public TMP_Text clearTypeText;

    [Header("Score Display (Player 2) - assign when using multiplayer")]
    public TMP_Text scoreTextP2;
    public TMP_Text gradeTextP2;
    public TMP_Text comboTextP2;
    public TMP_Text marvelousTextP2;
    public TMP_Text perfectTextP2;
    public TMP_Text greatTextP2;
    public TMP_Text goodTextP2;
    public TMP_Text badTextP2;
    public TMP_Text missTextP2;
    public TMP_Text clearTypeTextP2;

    [Header("Multiplayer")]
    public bool multiplayer = false;

    [Header("UI References")]
    public Button returnButton;
    public GameObject initialSelectedButton;
    public Image difficultyBackground;

    public ScoreManager scoreManager;
    public ScoreManager scoreManager2;
    public _GameManager gm;
    private SMFile currentSong;
    private SMChart currentChart;
    private SMFile currentSongP2;
    private SMChart currentChartP2;


    public GameObject winnerScreen;
    public TextMeshProUGUI winnerText;

    [Header("Difficulty Colors")]
    public Color beginnerColor = new Color(0.2f, 0.8f, 0.2f);
    public Color easyColor = new Color(0.2f, 0.6f, 1f);
    public Color mediumColor = new Color(1f, 0.8f, 0.2f);
    public Color hardColor = new Color(1f, 0.3f, 0.2f);
    public Color challengeColor = new Color(0.8f, 0.2f, 1f);
    public Color editColor = new Color(0.6f, 0.6f, 0.6f);

    void Start()
    {
        currentSong = GameSession.SelectedSong;
        currentChart = GameSession.SelectedChart;
        currentSongP2 = GameSession.SelectedSongP2;
        currentChartP2 = GameSession.SelectedChartP2;

        SetupSongInfo();
        SetupScoreDisplay();
        SetupNavigation();

        if (initialSelectedButton != null)
            StartCoroutine(SetInitialSelection());
    }

    private IEnumerator SetInitialSelection()
    {
        yield return new WaitForEndOfFrame();
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(initialSelectedButton);
    }

    public void SetupSongInfo()
    {
        if (currentSong != null)
        {
            songTitleText.text = currentSong.Title ?? "Unknown Title";
            artistText.text = currentSong.Artist ?? "Unknown Artist";

            // Player 1 main display
            if (currentChart != null)
            {
                if (difficultyText != null) difficultyText.text = currentChart.Difficulty ?? "Unknown";
                if (meterText != null) meterText.text = $"Lv. {currentChart.Meter}";
                SetDifficultyColor(currentChart.Difficulty);
            }
            else
            {
                Debug.LogWarning("[EvalScreenManager] currentChart (P1) is null.");
                if (difficultyText != null) difficultyText.text = "Unknown";
                if (meterText != null) meterText.text = "Lv. ?";
            }

            // Player 2 mirrored display (only difficulty, meter, background) if multiplayer
            if (multiplayer)
            {
                if (currentChartP2 != null)
                {
                    if (difficultyTextP2 != null) difficultyTextP2.text = currentChartP2.Difficulty ?? "Unknown";
                    else Debug.LogWarning("[EvalScreenManager] difficultyTextP2 is not assigned in the inspector.");

                    if (meterTextP2 != null) meterTextP2.text = $"Lv. {currentChartP2.Meter}";
                    else Debug.LogWarning("[EvalScreenManager] meterTextP2 is not assigned in the inspector.");

                    // Pass explicit target; if difficultyBackgroundP2 is null, SetDifficultyColor will warn instead of silently using P1 background.
                    SetDifficultyColor(currentChartP2.Difficulty, difficultyBackgroundP2);
                }
                else
                {
                    Debug.LogWarning("[EvalScreenManager] currentChartP2 is null. Ensure Player 2 selected a chart and GameSession.SelectedChartP2 is set.");
                    if (difficultyTextP2 != null) difficultyTextP2.text = "Unknown";
                    if (meterTextP2 != null) meterTextP2.text = "Lv. ?";
                }
            }

            if (!string.IsNullOrEmpty(currentSong.Banner))
                StartCoroutine(LoadBannerImage(currentSong.Banner, currentSong.DirectoryPath));

            StartCoroutine(LoadStageBackground(currentSong.DirectoryPath));
        }
        else
        {
            Debug.LogWarning("No song data available for evaluation screen");
            songTitleText.text = "No Song Data";
            artistText.text = "Unknown";
        }
    }

    private void SetupScoreDisplay()
    {
        if (!multiplayer)
        {
            if (scoreManager != null)
                UpdateScoreDisplay();
            else
                Debug.LogWarning("ScoreManager not found for evaluation screen (singleplayer)");
        }
        else
        {
            // Multiplayer: ensure both managers are present and update both displays if UI fields are assigned.
            if (scoreManager == null)
            {
                Debug.LogWarning("ScoreManager (player 1) not assigned for multiplayer evaluation screen");
            }

            if (scoreManager2 == null)
            {
                Debug.LogWarning("ScoreManager2 (player 2) not assigned for multiplayer evaluation screen");
            }

            UpdateScoreDisplay(); // this will handle both players
        }
    }

    private void UpdateScoreDisplay()
    {
        // Player 1
        if (scoreManager != null)
        {
            UpdateScoreDisplayFor(scoreManager,
                scoreText, gradeText, comboText,
                marvelousText, perfectText, greatText, goodText, badText, missText,
                clearTypeText);
        }

        // Player 2 (if multiplayer)
        if (multiplayer)
        {
            if (scoreManager2 != null)
            {
                // If P2 UI fields are not assigned, try to mirror P1 fields to avoid missing UI
                TMP_Text sText = scoreTextP2 ?? scoreText;
                TMP_Text gText = gradeTextP2 ?? gradeText;
                TMP_Text cText = comboTextP2 ?? comboText;
                TMP_Text mText = marvelousTextP2 ?? marvelousText;
                TMP_Text pText = perfectTextP2 ?? perfectText;
                TMP_Text grText = greatTextP2 ?? greatText;
                TMP_Text goText = goodTextP2 ?? goodText;
                TMP_Text bText = badTextP2 ?? badText;
                TMP_Text miText = missTextP2 ?? missText;
                TMP_Text ctText = clearTypeTextP2 ?? clearTypeText;

                UpdateScoreDisplayFor(scoreManager2,
                    sText, gText, cText,
                    mText, pText, grText, goText, bText, miText,
                    ctText);
            }
            else
            {
                Debug.LogWarning("Multiplayer is enabled but scoreManager2 is null. Player 2 display will not be updated.");
            }
        }
    }

    private void UpdateScoreDisplayFor(ScoreManager sm,
        TMP_Text scoreT, TMP_Text gradeT, TMP_Text comboT,
        TMP_Text marvelousT, TMP_Text perfectT, TMP_Text greatT, TMP_Text goodT, TMP_Text badT, TMP_Text missT,
        TMP_Text clearTypeT)
    {
        if (sm == null) return;

        if (scoreT != null) scoreT.text = $"{sm.currentScore:N0}";
        if (gradeT != null) gradeT.text = $" {sm.GetGrade()}";
        if (comboT != null) comboT.text = $"{sm.maxCombo}x";

        if (marvelousT != null) marvelousT.text = $"{sm.marvelousCount}";
        if (perfectT != null) perfectT.text = $"{sm.perfectCount}";
        if (greatT != null) greatT.text = $"{sm.greatCount}";
        if (goodT != null) goodT.text = $"{sm.goodCount}";
        if (badT != null) badT.text = $"{sm.badCount}";
        if (missT != null) missT.text = $"{sm.missCount}";

        if (clearTypeT != null)
        {
            string clearType = "Unknown";
            if (sm.hpManager != null)
            {
                try
                {
                    clearType = sm.hpManager.GetClearType(sm).ToString();
                }
                catch
                {
                    clearType = "Unknown";
                }
            }
            clearTypeT.text = clearType;
        }
    }

    // CHANGED: more robust difficulty color handling + diagnostics
    // Now supports specifying a target Image for player 2
    private void SetDifficultyColor(string difficulty, Image target = null)
    {
        Image img = target ?? difficultyBackground;

        if (img == null)
        {
            Debug.LogWarning("[EvalScreenManager] difficulty background Image is not assigned on the inspector. Cannot set difficulty color.");
            return;
        }

        // Default color when unknown
        Color colorToUse = editColor;

        if (string.IsNullOrWhiteSpace(difficulty))
        {
            Debug.Log("[EvalScreenManager] Difficulty string empty or null. Using default editColor.");
            img.color = colorToUse;
            return;
        }

        // normalize for comparison
        string normalized = difficulty.Trim().ToLowerInvariant();
        Debug.Log($"[EvalScreenManager] Setting difficulty color. difficulty='{difficulty}' normalized='{normalized}'");

        // Common variants - use contains and startsWith to tolerate variations
        if (normalized.Contains("beginner") || normalized.StartsWith("b")) colorToUse = beginnerColor;
        else if (normalized.Contains("easy") || normalized.StartsWith("e")) colorToUse = easyColor;
        else if (normalized.Contains("medium") || normalized.Contains("normal") || normalized.StartsWith("m")) colorToUse = mediumColor;
        else if (normalized.Contains("hard") || normalized.StartsWith("h")) colorToUse = hardColor;
        else if (normalized.Contains("challenge") || normalized.Contains("insane") || normalized.StartsWith("c") || normalized.StartsWith("x")) colorToUse = challengeColor;
        else if (normalized.Contains("edit")) colorToUse = editColor;
        else
        {
            // If difficulty is just a number (e.g. "7") map meter ranges (optional)
            if (int.TryParse(normalized, out int num))
            {
                if (num <= 3) colorToUse = beginnerColor;
                else if (num <= 5) colorToUse = easyColor;
                else if (num <= 7) colorToUse = mediumColor;
                else if (num <= 9) colorToUse = hardColor;
                else colorToUse = challengeColor;
                Debug.Log($"[EvalScreenManager] Difficulty parsed as number {num}, mapped to color.");
            }
            else
            {
                Debug.Log($"[EvalScreenManager] Difficulty string '{difficulty}' not recognized. Using default editColor.");
            }
        }

        img.color = colorToUse;
    }

    private void SetupNavigation()
    {
        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(OnReturnToStageSelect);
        }
    }

    private IEnumerator LoadBannerImage(string bannerFilename, string songDirectory)
    {
        if (string.IsNullOrEmpty(bannerFilename) || bannerImage == null) yield break;

        string bannerPath = Path.Combine(songDirectory, bannerFilename);
        if (!File.Exists(bannerPath))
        {
            string songsRoot = Path.Combine(Application.dataPath, "Songs");
            string[] foundFiles = Directory.GetFiles(songsRoot, bannerFilename, SearchOption.AllDirectories);
            if (foundFiles.Length > 0) bannerPath = foundFiles[0];
            else { Debug.LogWarning($"Banner file not found: {bannerFilename}"); yield break; }
        }

        byte[] fileData = File.ReadAllBytes(bannerPath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Sprite bannerSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            bannerImage.sprite = bannerSprite;
            bannerImage.preserveAspect = false;
            bannerImage.type = Image.Type.Simple;
        }
        else Debug.LogWarning("Failed to load banner image from file data");
    }

    private IEnumerator LoadStageBackground(string songDirectory)
    {
        if (backgroundImage == null) yield break;

        string stageFilename = "_stagefile.png";
        string bgPath = Path.Combine(songDirectory, stageFilename);

        Debug.Log($"[EvalScreenManager] Looking for stage background at: {bgPath}");

        if (!File.Exists(bgPath))
        {
            // Try to find the file in the Songs folder
            string songsRoot = Path.Combine(Application.dataPath, "Songs");
            string[] foundFiles = Directory.GetFiles(songsRoot, stageFilename, SearchOption.AllDirectories);
            if (foundFiles.Length > 0)
            {
                bgPath = foundFiles[0];
                Debug.Log($"[EvalScreenManager] Found stage background at: {bgPath}");
            }
            else
            {
                Debug.LogWarning($"[EvalScreenManager] Stage background file not found: {stageFilename}");
                yield break;
            }
        }

        string uri = "file:///" + UnityWebRequest.EscapeURL(bgPath.Replace("\\", "/"));

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                // Make sure texture uses expected settings
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                // Create a single sprite instance and assign it to both images
                Sprite bgSprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100f);

                // Ensure primary background image settings
                backgroundImage.sprite = bgSprite;
                backgroundImage.type = Image.Type.Simple;
                backgroundImage.preserveAspect = false;

                // If the second background image exists, explicitly sync all relevant properties
                if (backgroundImage2 != null)
                {
                    backgroundImage2.sprite = bgSprite;
                    backgroundImage2.type = Image.Type.Simple;
                    backgroundImage2.preserveAspect = false;

                    // Match material (if any) and rectTransform size so they render identically
                    backgroundImage2.material = backgroundImage.material;
                    backgroundImage2.rectTransform.sizeDelta = backgroundImage.rectTransform.sizeDelta;
                    backgroundImage2.rectTransform.anchorMin = backgroundImage.rectTransform.anchorMin;
                    backgroundImage2.rectTransform.anchorMax = backgroundImage.rectTransform.anchorMax;
                    backgroundImage2.rectTransform.anchoredPosition = backgroundImage.rectTransform.anchoredPosition;
                    backgroundImage2.rectTransform.pivot = backgroundImage.rectTransform.pivot;
                }

                // Make background darker without changing opacity
                Color bgColor = backgroundImage.color;
                bgColor.r *= 0.4f; // Reduce red channel to 40%
                bgColor.g *= 0.4f; // Reduce green channel to 40%  
                bgColor.b *= 0.4f; // Reduce blue channel to 40%
                backgroundImage.color = bgColor;

                if (backgroundImage2 != null)
                    backgroundImage2.color = bgColor;

                Debug.Log($"[EvalScreenManager] Stage background loaded and darkened. Assigned to backgroundImage and backgroundImage2.");
            }
            else
            {
                Debug.LogWarning($"Failed to load stage background: {www.error}");
            }
        }
    }

    public void OnReturnToStageSelect()
    {
        Debug.Log("[EvalScreenManager] Returning to stage selection");
        GameSession.SelectedSong = null;
        GameSession.SelectedChart = null;
        if (gm == null) gm = FindFirstObjectByType<_GameManager>();

        // Prefer the local multiplayer flag if set, otherwise fall back to gm.multiplayer
        bool isMultiplayer = multiplayer || (gm != null && gm.multiplayer);

        if (isMultiplayer)
        {
            if (winnerScreen != null) winnerScreen.SetActive(true);

            if (scoreManager != null && scoreManager2 != null)
            {
                if (scoreManager.currentScore > scoreManager2.currentScore)
                {
                    if (winnerText != null) winnerText.text = "Player 1 Wins!";
                    if (gm != null) gm.p1Score += 1;
                }
                else if (scoreManager.currentScore < scoreManager2.currentScore)
                {
                    if (winnerText != null) winnerText.text = "Player 2 Wins!";
                    if (gm != null) gm.p2Score += 1;
                }
                else
                {
                    if (winnerText != null) winnerText.text = "It's a Tie!";
                    if (gm != null) { gm.p1Score += 1; gm.p2Score += 1; }
                }
            }
            else
            {
                Debug.LogWarning("Multiplayer end: one or both ScoreManagers are null, cannot determine winner.");
            }
        }

        StartCoroutine(Wait(2));
        if (gm != null && gm.party) UnityEngine.SceneManagement.SceneManager.LoadScene("CardSelect");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("StageSelect");
    }

    public IEnumerator Wait(int time) { yield return new WaitForSeconds(time); }

    void Update() { UpdateScoreDisplay(); }

    public void RefreshDisplay() { SetupSongInfo(); UpdateScoreDisplay(); }
}
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
    public TMP_Text songTitleText;
    public TMP_Text artistText;
    public TMP_Text difficultyText;
    public TMP_Text meterText;

    [Header("Score Display")]
    public TMP_Text scoreText;
    public TMP_Text gradeText;
    public TMP_Text comboText;
    public TMP_Text marvelousText;
    public TMP_Text perfectText;
    public TMP_Text greatText;
    public TMP_Text goodText;
    public TMP_Text badText;
    public TMP_Text missText;

    [Header("UI References")]
    public Button returnButton;
    public GameObject initialSelectedButton;
    public Image difficultyBackground;

    public ScoreManager scoreManager;
    public ScoreManager scoreManager2;//
    public _GameManager gm;//
    private SMFile currentSong;
    private SMChart currentChart;

    [Header("Difficulty Colors")]
    public Color beginnerColor = new Color(0.2f, 0.8f, 0.2f);        // Green
    public Color easyColor = new Color(0.2f, 0.6f, 1f);             // Blue
    public Color mediumColor = new Color(1f, 0.8f, 0.2f);           // Yellow/Orange
    public Color hardColor = new Color(1f, 0.3f, 0.2f);             // Red
    public Color challengeColor = new Color(0.8f, 0.2f, 1f);        // Purple
    public Color editColor = new Color(0.6f, 0.6f, 0.6f);           // Gray

    void Start()
    {

        // Get the current song and chart from GameSession
        currentSong = GameSession.SelectedSong;
        currentChart = GameSession.SelectedChart;

        // Set up the evaluation screen
        SetupSongInfo();
        SetupScoreDisplay();
        SetupNavigation();

        // Set initial selected button for controller support
        if (initialSelectedButton != null)
        {
            StartCoroutine(SetInitialSelection());
        }
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
            // Set text information
            songTitleText.text = currentSong.Title ?? "Unknown Title";
            artistText.text = currentSong.Artist ?? "Unknown Artist";

            // Set chart information
            if (currentChart != null)
            {
                difficultyText.text = currentChart.Difficulty ?? "Unknown";
                meterText.text = $"Lv. {currentChart.Meter}";
                SetDifficultyColor(currentChart.Difficulty);
            }

            // Load and display banner image
            if (!string.IsNullOrEmpty(currentSong.Banner))
            {
                StartCoroutine(LoadBannerImage(currentSong.Banner, currentSong.DirectoryPath));
            }

            // CHANGE THIS: Load _stagefile.png instead of banner for background
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
        if (scoreManager != null)
        {
            UpdateScoreDisplay();
        }
        else
        {
            Debug.LogWarning("ScoreManager not found for evaluation screen");
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreManager == null) return;

        // Update all score information
        scoreText.text = $"{scoreManager.currentScore:N0}";
        gradeText.text = $" {scoreManager.GetGrade()}";
        // CHANGE THIS LINE: Use maxCombo instead of GetComboInfo()
        comboText.text = $"Max Combo: {scoreManager.maxCombo}x";

        // Update judgment breakdown
        if (marvelousText != null)
            marvelousText.text = $"{scoreManager.marvelousCount}";
        if (perfectText != null)
            perfectText.text = $"{scoreManager.perfectCount}";

        if (greatText != null)
            greatText.text = $"{scoreManager.greatCount}";

        if (goodText != null)
            goodText.text = $"{scoreManager.goodCount}";

        if (badText != null)
            badText.text = $"{scoreManager.badCount}";

        if (missText != null)
            missText.text = $"{scoreManager.missCount}";
    }

    private void SetDifficultyColor(string difficulty)
    {
        if (difficultyBackground == null) return;

        Color colorToUse = editColor; // Default fallback

        if (!string.IsNullOrEmpty(difficulty))
        {
            string diffLower = difficulty.ToLower();

            if (diffLower.Contains("beginner")) colorToUse = beginnerColor;
            else if (diffLower.Contains("easy")) colorToUse = easyColor;
            else if (diffLower.Contains("medium")) colorToUse = mediumColor;
            else if (diffLower.Contains("hard")) colorToUse = hardColor;
            else if (diffLower.Contains("challenge")) colorToUse = challengeColor;
            else if (diffLower.Contains("edit")) colorToUse = editColor;
            // Fallback to editColor if no match
        }

        difficultyBackground.color = colorToUse;
    }




    private void SetupNavigation()
    {
        // Set up button event
        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(OnReturnToStageSelect);
        }
    }


    private IEnumerator LoadBannerImage(string bannerFilename, string songDirectory)
    {
        if (string.IsNullOrEmpty(bannerFilename) || bannerImage == null)
            yield break;

        string bannerPath = Path.Combine(songDirectory, bannerFilename);

        if (!File.Exists(bannerPath))
        {
            // Try to find the file in the Songs folder
            string songsRoot = Path.Combine(Application.dataPath, "Songs");
            string[] foundFiles = Directory.GetFiles(songsRoot, bannerFilename, SearchOption.AllDirectories);
            if (foundFiles.Length > 0)
            {
                bannerPath = foundFiles[0];
            }
            else
            {
                Debug.LogWarning($"Banner file not found: {bannerFilename}");
                yield break;
            }
        }

        // Load the file directly as bytes to avoid compression
        byte[] fileData = File.ReadAllBytes(bannerPath);
        Texture2D texture = new Texture2D(2, 2);

        // Load the image without compression
        if (texture.LoadImage(fileData))
        {
            // Disable compression for this texture
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            // Create sprite
            Sprite bannerSprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);

            bannerImage.sprite = bannerSprite;

            // Don't preserve aspect - let it stretch to fill the RectTransform
            bannerImage.preserveAspect = false;
            bannerImage.type = Image.Type.Simple;

            Debug.Log($"[EvalScreenManager] Banner loaded without aspect preservation: {texture.width}x{texture.height}");
        }
        else
        {
            Debug.LogWarning("Failed to load banner image from file data");
        }
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
                Sprite bgSprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                backgroundImage.sprite = bgSprite;

                // Make background darker without changing opacity
                Color bgColor = backgroundImage.color;
                bgColor.r *= 0.4f; // Reduce red channel to 40%
                bgColor.g *= 0.4f; // Reduce green channel to 40%  
                bgColor.b *= 0.4f; // Reduce blue channel to 40%
                backgroundImage.color = bgColor;

                Debug.Log($"[EvalScreenManager] Stage background loaded and darkened");
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

        // Reset game session data
        GameSession.SelectedSong = null;
        GameSession.SelectedChart = null;
        if (gm == null)//////////////
        {
            gm = FindFirstObjectByType<_GameManager>();
        }
        if (gm.multiplayer)
        {
            if (scoreManager.currentScore > scoreManager2.currentScore)
            {
                gm.p1Score += 1;
                gm.p1.DisableOthers();
            }
            else if (scoreManager.currentScore < scoreManager2.currentScore)
            {
                gm.p2Score += 1;
                gm.p2.DisableOthers();
            }
            else if (scoreManager.currentScore == scoreManager2.currentScore)
            {
                gm.p1Score += 1;
                gm.p2Score += 1;
            }
        }///////////////////////////////////////////

        // Load stage select scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("StageSelect");
    }

    // Update display in case scores change (though they shouldn't after game ends)
    void Update()
    {
        UpdateScoreDisplay();
    }

    // Public method to refresh display (can be called from GameEndManager)
    public void RefreshDisplay()
    {
        SetupSongInfo();
        UpdateScoreDisplay();
    }
}
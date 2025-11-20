using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameEndManager : MonoBehaviour
{
    [Header("Game End Settings")]
    public float endDelayAfterLastNote = 2f;
    public float fadeDuration = 1.5f;

    [Header("UI References")]
    public GameObject evalScreen;
    public Image fadeOverlay;
    public string stageSelectSceneName = "StageSelect";

    public NoteSpawner noteSpawner;
    public AudioSource music;
    public EvalScreenManager evalScreenManager;
    private bool gameEnding = false;
    private bool gameEnded = false;
    private bool allNotesSpawned = false;

    public static GameEndManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        // Initialize fade overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            Color color = fadeOverlay.color;
            color.a = 0f;
            fadeOverlay.color = color;
        }

        if (evalScreen != null)
            evalScreen.SetActive(false);
    }

    void Update()
    {
        if (gameEnding || gameEnded) return;

        // Check if all notes have been spawned
        if (noteSpawner != null && !allNotesSpawned && noteSpawner.IsChartComplete())
        {
            allNotesSpawned = true;
            Debug.Log($"[GameEndManager] All notes spawned, ending game in {endDelayAfterLastNote} seconds");

            // Start the end sequence after delay
            Invoke(nameof(StartEndSequence), endDelayAfterLastNote);
        }
    }

    private void StartEndSequence()
    {
        if (!gameEnding && !gameEnded)
        {
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        gameEnding = true;
        Debug.Log("[GameEndManager] Starting game end sequence with fade");

        // Start fade effects
        StartCoroutine(FadeMusic());
        yield return StartCoroutine(FadeToBlack());

        // Wait a moment in black screen
        yield return new WaitForSeconds(0.5f);

        // Show evaluation screen and fade back in
        ShowEvalScreen();
        yield return StartCoroutine(FadeFromBlack());

        gameEnded = true;
    }

    private IEnumerator FadeMusic()
    {
        if (music == null) yield break;

        float startVolume = music.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            music.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        music.volume = 0f;
        music.Stop();
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeOverlay == null) yield break;

        float timer = 0f;
        Color color = fadeOverlay.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            color.a = Mathf.Lerp(0f, 1f, progress);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeOverlay.color = color;
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeOverlay == null) yield break;

        float timer = 0f;
        Color color = fadeOverlay.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            color.a = Mathf.Lerp(1f, 0f, progress);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeOverlay.color = color;

        
        if (fadeOverlay.TryGetComponent<Image>(out var fadeImage))
        {
            fadeImage.raycastTarget = false;
        }
    }

    private void ShowEvalScreen()
    {
        if (evalScreen != null)
        {
            evalScreen.SetActive(true);

            EvalScreenManager evalManager = evalScreen.GetComponent<EvalScreenManager>();
            if (evalManager != null)
            {
                evalManager.RefreshDisplay();
            }
            else
            {
                Debug.LogWarning("[GameEndManager] EvalScreenManager component not found on eval screen!");
            }

            Debug.Log("[GameEndManager] Evaluation screen shown and refreshed");
        }
        else
        {
            Debug.LogWarning("[GameEndManager] No evaluation screen assigned!");
        }
    }

    // Called from the evaluation screen button
    public void ReturnToStageSelect()
    {
        Debug.Log("[GameEndManager] Returning to stage selection");

        // RESET THE GAME SESSION DATA
        GameSession.SelectedSong = null;
        GameSession.SelectedChart = null;
        Debug.Log("[GameEndManager] GameSession data cleared");

        // Load the stage select scene
        if (!string.IsNullOrEmpty(stageSelectSceneName))
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
        else
        {
            Debug.LogWarning("[GameEndManager] No stage select scene name specified!");
        }
    }

    // Force end game (for testing)
    public void ForceEndGame()
    {
        if (!gameEnding && !gameEnded)
        {
            StartCoroutine(EndGame());
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class GameEndManager : MonoBehaviour
{
    [Header("Game End Settings")]
    public float endDelayAfterLastNote = 2f;
    public float fadeDuration = 1.5f;

    [Header("UI References")]
    public GameObject evalScreen;
    public GameObject returnButton;
    public Image fadeOverlay;
    public string stageSelectSceneName = "StageSelect";

    public NoteSpawner noteSpawner;
    public AudioSource music;
    public EvalScreenManager evalScreenManager;
    private bool gameEnding = false;
    private bool gameEnded = false;
    private bool allNotesSpawned = false;

    public static GameEndManager Instance { get; private set; }
    private _GameManager gm;

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

        if (evalScreenManager != null && evalScreenManager.backgroundImage2 == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject go = new GameObject("Background2_Startup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(canvas.transform, false);

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;

                Image img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;
                img.color = Color.white;

                // Assign to EvalScreenManager so it will be updated when sprite loads
                evalScreenManager.backgroundImage2 = img;

                Debug.Log("[GameEndManager] Created Background2_Startup and assigned to EvalScreenManager.backgroundImage2");
            }
            else
            {
                Debug.LogWarning("[GameEndManager] No Canvas found to parent runtime background image.");
            }
        }
    }

    void Update()
    {
        if (gameEnding || gameEnded) return;

        // Prefer to wait until all judgments have been processed (all notes hit or missed).
        // This ensures long-note releases (which score on release) are counted before ending.
        // If any ScoreManager instances exist in scene (multiplayer), wait until all have processed their judgments
        var allSMS = FindObjectsByType<ScoreManager>(FindObjectsSortMode.None);
        if (allSMS != null && allSMS.Length > 0)
        {
            int totalNotesSum = 0;
            int notesHitSum = 0;
            foreach (var s in allSMS)
            {
                if (s == null) continue;
                if (s.totalNotes > 0)
                {
                    totalNotesSum += s.totalNotes;
                    notesHitSum += s.notesHit;
                }
            }

            if (!allNotesSpawned && totalNotesSum > 0 && notesHitSum >= totalNotesSum)
            {
                allNotesSpawned = true;
                Debug.Log($"[GameEndManager] All judgments processed ({notesHitSum}/{totalNotesSum}), ending game in {endDelayAfterLastNote} seconds");

                // Start the end sequence after delay
                Invoke(nameof(StartEndSequence), endDelayAfterLastNote);
            }
        }
        else if (noteSpawner != null && !allNotesSpawned && noteSpawner.IsChartComplete())
        {
            // Fallback to spawn-complete behavior if no ScoreManager is available
            allNotesSpawned = true;
            Debug.Log($"[GameEndManager] All notes spawned (fallback), ending game in {endDelayAfterLastNote} seconds");

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

        gm = FindFirstObjectByType<_GameManager>();
        gm.tutorial = false;

        // Finalize all ScoreManagers in the scene. Multiplayer may have multiple ScoreManager instances
        // (one per player). ScoreManager.FinalizeScore already guards against double-saving.
        var allScoreManagers = FindObjectsByType<ScoreManager>(FindObjectsSortMode.None);
        if (allScoreManagers != null && allScoreManagers.Length > 0)
        {
            foreach (var sm in allScoreManagers)
            {
                if (sm == null) continue;
                sm.FinalizeScore();
                Debug.Log($"[GameEndManager] Finalized score. Max Combo: {sm.maxCombo} (Player #{sm.playerToUse})");
            }
        }
        else
        {
            Debug.LogWarning("[GameEndManager] No ScoreManager instances found in scene to finalize.");
        }

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

    private void ShowEvalScreen()
    {
        if (evalScreen != null)
        {
            // Use coroutine to activate UI, wait a frame for children to enable, then refresh.
            StartCoroutine(ShowEvalScreenRoutine());
        }
        else
        {
            Debug.LogWarning("[GameEndManager] No evaluation screen assigned!");
        }
    }

    private IEnumerator ShowEvalScreenRoutine()
    {
        // Activate eval screen
        evalScreen.SetActive(true);

        // Wait one frame so any child GameObjects / Image components become active and OnEnable runs
        yield return null;

        // Force canvas/layout update to ensure Image components rebuild before assigning sprites
        Canvas.ForceUpdateCanvases();

        // Set the selected UI element for controller/keyboard navigation
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(returnButton);

        // Refresh the evaluation manager display (assigns sprites, text, etc.)
        EvalScreenManager evalManager = evalScreen.GetComponent<EvalScreenManager>();
        if (evalManager != null)
        {
            evalManager.RefreshDisplay();

            // Wait another frame to let any cached assignments take effect (covers cases where backgroundImage2
            // was inactive and only became active when evalScreen was activated)
            yield return null;
            Canvas.ForceUpdateCanvases();

            // Call RefreshDisplay again to be safe (idempotent) so images/layouts are synced
            evalManager.RefreshDisplay();


            // give one more frame for UI to update
            yield return null;
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Debug.LogWarning("[GameEndManager] EvalScreenManager component not found on eval screen!");
        }

        Debug.Log("[GameEndManager] Evaluation screen shown and refreshed");
    }
}
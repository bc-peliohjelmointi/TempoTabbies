using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class MultiGameManager : MonoBehaviour
{
    [Header("References")]
    public NoteSpawner Spawner_P1;
    public NoteSpawner Spawner_P2;
    public AudioSource Music;
    public MultiHitManager HitManager;

    [Header("Player 1 Prefabs")]
    public GameObject NotePrefab_TypeA_P1;
    public GameObject NotePrefab_TypeB_P1;
    public GameObject NotePrefab_TypeC_P1;
    public GameObject NotePrefab_TypeD_P1;

    [Header("Player 2 Prefabs")]
    public GameObject NotePrefab_TypeA_P2;
    public GameObject NotePrefab_TypeB_P2;
    public GameObject NotePrefab_TypeC_P2;
    public GameObject NotePrefab_TypeD_P2;

    [Header("Player 1 Layout")]
    public Transform LaneParent_P1;
    public Transform HitLine_P1;

    [Header("Player 2 Layout")]
    public Transform LaneParent_P2;
    public Transform HitLine_P2;

    public static MultiGameManager Instance { get; private set; }
    public static float SongTime
    {
        get
        {
            if (Instance != null && Instance.Music != null && Instance.Music.isPlaying)
            {
                return Instance.Music.time;
            }
            return 0f;
        }
    }

    private bool initialized = false;

    void Awake()
    {
        Instance = this;
        Debug.Log("[MultiGameManager] Awake called");
    }

    void Start()
    {
        Debug.Log("[MultiGameManager] Start called - beginning initialization");
        // Don't initialize immediately - wait one frame for everything to be set up
        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        // Wait for one frame to ensure all components are loaded
        yield return null;

        Debug.Log("[MultiGameManager] Starting delayed initialization");
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (initialized) return;

        Debug.Log("[MultiGameManager] ValidateReferences started");
        // Validate critical references first
        if (!ValidateReferences())
        {
            Debug.LogError("Critical references are missing! Cannot initialize game.");
            return;
        }

        Debug.Log("[MultiGameManager] Checking GameSession data");
        if (GameSession.SelectedSong == null || GameSession.SelectedChart == null)
        {
            Debug.LogError("No song or chart selected! Please load from Song Select first.");
            return;
        }

        SMFile sm = GameSession.SelectedSong;
        SMChart chart = GameSession.SelectedChart;

        Debug.Log($"[MultiGameManager] Now playing: {sm.Title} by {sm.Artist}");
        Debug.Log($"[MultiGameManager] Chart measures: {chart.Measures?.Count ?? 0}");

        try
        {
            Debug.Log("[MultiGameManager] Initializing Player 1");
            // Initialize Player 1
            InitializePlayer(0, Spawner_P1, LaneParent_P1, HitLine_P1,
                NotePrefab_TypeA_P1, NotePrefab_TypeB_P1, NotePrefab_TypeC_P1, NotePrefab_TypeD_P1);

            Debug.Log("[MultiGameManager] Initializing Player 2");
            // Initialize Player 2
            InitializePlayer(1, Spawner_P2, LaneParent_P2, HitLine_P2,
                NotePrefab_TypeA_P2, NotePrefab_TypeB_P2, NotePrefab_TypeC_P2, NotePrefab_TypeD_P2);

            Debug.Log("[MultiGameManager] Loading chart for Player 1");
            // Load chart for both players
            Spawner_P1.LoadChart(sm, chart);
            Debug.Log("[MultiGameManager] Loading chart for Player 2");
            Spawner_P2.LoadChart(sm, chart);

            Debug.Log("[MultiGameManager] Starting music loading");
            // Start music loading
            StartCoroutine(LoadAndStartMusic(sm));

            initialized = true;
            Debug.Log("[MultiGameManager] Initialized successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize game: {e.Message}\n{e.StackTrace}");
        }
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (Music == null)
        {
            Debug.LogError("Music AudioSource is not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("[MultiGameManager] Music reference: OK");
        }

        if (Spawner_P1 == null)
        {
            Debug.LogError("Spawner_P1 is not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("[MultiGameManager] Spawner_P1 reference: OK");
        }

        if (Spawner_P2 == null)
        {
            Debug.LogError("Spawner_P2 is not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("[MultiGameManager] Spawner_P2 reference: OK");
        }

        if (LaneParent_P1 == null)
        {
            Debug.LogError("LaneParent_P1 is not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log($"[MultiGameManager] LaneParent_P1 reference: OK ({LaneParent_P1.childCount} children)");
        }

        if (LaneParent_P2 == null)
        {
            Debug.LogError("LaneParent_P2 is not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log($"[MultiGameManager] LaneParent_P2 reference: OK ({LaneParent_P2.childCount} children)");
        }

        if (HitLine_P1 == null)
        {
            Debug.LogError("HitLine_P1 is not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("[MultiGameManager] HitLine_P1 reference: OK");
        }

        if (HitLine_P2 == null)
        {
            Debug.LogError("HitLine_P2 is not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("[MultiGameManager] HitLine_P2 reference: OK");
        }

        return valid;
    }

    private void InitializePlayer(int playerIndex, NoteSpawner spawner, Transform laneParent, Transform hitLine,
        GameObject prefabA, GameObject prefabB, GameObject prefabC, GameObject prefabD)
    {
        if (spawner == null || laneParent == null || hitLine == null)
        {
            Debug.LogError($"Player {playerIndex + 1} has missing components!");
            return;
        }

        Debug.Log($"[MultiGameManager] Initializing Player {playerIndex + 1} with {laneParent.childCount} lanes");

        Transform[] lanes = new Transform[laneParent.childCount];
        for (int i = 0; i < laneParent.childCount; i++)
        {
            lanes[i] = laneParent.GetChild(i);
            Debug.Log($"[MultiGameManager] Player {playerIndex + 1} Lane {i}: {lanes[i].name}");
        }

        spawner.Music = Music;
        spawner.HitLine = hitLine;
        spawner.Lanes = lanes;

        spawner.NotePrefab_TypeA = prefabA;
        spawner.NotePrefab_TypeB = prefabB;
        spawner.NotePrefab_TypeC = prefabC;
        spawner.NotePrefab_TypeD = prefabD;

        Debug.Log($"Player {playerIndex + 1} initialized successfully");
    }

    private IEnumerator LoadAndStartMusic(SMFile sm)
    {
        Debug.Log("[MultiGameManager] LoadAndStartMusic coroutine started");

        yield return new WaitForSeconds(0.1f);

        string musicFile = sm.MusicFile;
        if (string.IsNullOrEmpty(musicFile))
        {
            Debug.LogError($"No MUSIC tag found in SM file for {sm.Title}");
            yield break;
        }

        // Find the music file - use a different method name
        string fullPath = LocateMusicFile(sm);
        if (string.IsNullOrEmpty(fullPath))
        {
            Debug.LogError($"Music file not found: {musicFile}");
            yield break;
        }

        Debug.Log($"[MultiGameManager] Found audio file at: {fullPath}");

        // Properly format the URL for WWW
        string url = "file:///" + fullPath.Replace("\\", "/");

        Debug.Log($"[MultiGameManager] Loading audio from URL: {url}");

        AudioClip loadedClip = null;

        // Use WWW for better compatibility
        using (WWW www = new WWW(url))
        {
            Debug.Log("[MultiGameManager] WWW loading started...");
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError($"WWW loading error: {www.error}");
                Debug.LogError($"URL attempted: {url}");
                Debug.LogError($"File exists: {File.Exists(fullPath)}");
                yield break;
            }

            Debug.Log("[MultiGameManager] WWW loading complete, getting AudioClip...");

            try
            {
                loadedClip = www.GetAudioClip(false, false);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MultiGameManager] Error creating AudioClip: {e.Message}");
                yield break;
            }
        }

        // Wait for the AudioClip to load (outside the try-catch)
        if (loadedClip != null)
        {
            while (loadedClip.loadState == AudioDataLoadState.Loading)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (loadedClip.loadState == AudioDataLoadState.Loaded)
            {
                Debug.Log($"[MultiGameManager] AudioClip loaded: {loadedClip.name} ({loadedClip.length}s)");
                Music.clip = loadedClip;
            }
            else
            {
                Debug.LogError($"[MultiGameManager] AudioClip failed to load: {loadedClip.loadState}");
                yield break;
            }
        }
        else
        {
            Debug.LogError("[MultiGameManager] loadedClip is null!");
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        if (Music.clip != null && Music.clip.loadState == AudioDataLoadState.Loaded)
        {
            Debug.Log("[MultiGameManager] Starting music playback...");
            Music.Play();
            Debug.Log($"[MultiGameManager] Music started successfully!");
        }
        else
        {
            Debug.LogError("[MultiGameManager] Cannot play - Music.clip is not properly loaded!");
        }
    }

    // Renamed method to avoid conflict
    private string LocateMusicFile(SMFile sm)
    {
        string songDir = Path.GetDirectoryName(sm.FilePath);
        string songsRoot = Path.Combine(Application.dataPath, "Songs");
        string musicFile = sm.MusicFile;

        // Try the song directory first
        string fullPath = Path.Combine(songDir, musicFile);

        if (File.Exists(fullPath))
        {
            return fullPath;
        }

        // Try searching in Songs root
        string[] found = Directory.GetFiles(songsRoot, musicFile, SearchOption.AllDirectories);
        if (found.Length > 0)
        {
            return found[0];
        }

        // Try with different search pattern
        found = Directory.GetFiles(songsRoot, "*" + musicFile + "*", SearchOption.AllDirectories);
        if (found.Length > 0)
        {
            return found[0];
        }

        Debug.LogError($"Music file not found: {musicFile}");
        Debug.LogError($"Searched in: {songDir} and {songsRoot}");
        return null;
    }

    private string FindMusicFile(SMFile sm)
    {
        string songDir = Path.GetDirectoryName(sm.FilePath);
        string songsRoot = Path.Combine(Application.dataPath, "Songs");
        string musicFile = sm.MusicFile;

        // Try the song directory first
        string fullPath = Path.Combine(songDir, musicFile);

        if (File.Exists(fullPath))
        {
            return fullPath;
        }

        // Try searching in Songs root
        string[] found = Directory.GetFiles(songsRoot, musicFile, SearchOption.AllDirectories);
        if (found.Length > 0)
        {
            return found[0];
        }

        // Try with different search pattern
        found = Directory.GetFiles(songsRoot, "*" + musicFile + "*", SearchOption.AllDirectories);
        if (found.Length > 0)
        {
            return found[0];
        }

        Debug.LogError($"Music file not found: {musicFile}");
        Debug.LogError($"Searched in: {songDir} and {songsRoot}");
        return null;
    }


    public bool BothChartsComplete()
    {
        return Spawner_P1 != null && Spawner_P2 != null &&
               Spawner_P1.IsChartComplete() && Spawner_P2.IsChartComplete();
    }
}
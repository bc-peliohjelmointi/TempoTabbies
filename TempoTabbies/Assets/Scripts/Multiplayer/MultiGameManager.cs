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
    }

    void Start()
    {
        // Don't initialize immediately - wait one frame for everything to be set up
        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        // Wait for one frame to ensure all components are loaded
        yield return null;
        
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (initialized) return;

        // Validate critical references first
        if (!ValidateReferences())
        {
            Debug.LogError("Critical references are missing! Cannot initialize game.");
            return;
        }

        if (GameSession.SelectedSong == null || GameSession.SelectedChart == null)
        {
            Debug.LogError("No song or chart selected! Please load from Song Select first.");
            return;
        }

        SMFile sm = GameSession.SelectedSong;
        SMChart chart = GameSession.SelectedChart;

        Debug.Log($"Now playing: {sm.Title} by {sm.Artist}");

        try
        {
            // Initialize Player 1
            InitializePlayer(0, Spawner_P1, LaneParent_P1, HitLine_P1, 
                NotePrefab_TypeA_P1, NotePrefab_TypeB_P1, NotePrefab_TypeC_P1, NotePrefab_TypeD_P1);

            // Initialize Player 2
            InitializePlayer(1, Spawner_P2, LaneParent_P2, HitLine_P2,
                NotePrefab_TypeA_P2, NotePrefab_TypeB_P2, NotePrefab_TypeC_P2, NotePrefab_TypeD_P2);

            // Load chart for both players
            Spawner_P1.LoadChart(sm, chart);
            Spawner_P2.LoadChart(sm, chart);

            // Start music loading
            StartCoroutine(LoadAndStartMusic(sm));

            initialized = true;
            Debug.Log("MultiGameManager initialized successfully!");
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

        if (Spawner_P1 == null)
        {
            Debug.LogError("Spawner_P1 is not assigned!");
            valid = false;
        }

        if (Spawner_P2 == null)
        {
            Debug.LogError("Spawner_P2 is not assigned!");
            valid = false;
        }

        if (LaneParent_P1 == null)
        {
            Debug.LogError("LaneParent_P1 is not assigned!");
            valid = false;
        }

        if (LaneParent_P2 == null)
        {
            Debug.LogError("LaneParent_P2 is not assigned!");
            valid = false;
        }

        if (HitLine_P1 == null)
        {
            Debug.LogError("HitLine_P1 is not assigned!");
            valid = false;
        }

        if (HitLine_P2 == null)
        {
            Debug.LogError("HitLine_P2 is not assigned!");
            valid = false;
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

        Transform[] lanes = new Transform[laneParent.childCount];
        for (int i = 0; i < laneParent.childCount; i++)
            lanes[i] = laneParent.GetChild(i);

        spawner.Music = Music;
        spawner.HitLine = hitLine;
        spawner.Lanes = lanes;

        spawner.NotePrefab_TypeA = prefabA;
        spawner.NotePrefab_TypeB = prefabB;
        spawner.NotePrefab_TypeC = prefabC;
        spawner.NotePrefab_TypeD = prefabD;

        Debug.Log($"Player {playerIndex + 1} initialized with {lanes.Length} lanes");
    }

    private IEnumerator LoadAndStartMusic(SMFile sm)
    {
        // Wait a bit more to ensure everything is set up
        yield return new WaitForSeconds(0.1f);

        string songDir = Path.GetDirectoryName(sm.FilePath);
        string songsRoot = Path.Combine(Application.dataPath, "Songs");

        string musicFile = sm.MusicFile;
        if (string.IsNullOrEmpty(musicFile))
        {
            Debug.LogError($"No MUSIC tag found in SM file for {sm.Title}");
            yield break;
        }

        string fullPath = Path.Combine(songDir, musicFile);

        if (!File.Exists(fullPath))
        {
            string[] found = Directory.GetFiles(songsRoot, Path.GetFileName(musicFile), SearchOption.AllDirectories);
            if (found.Length > 0)
            {
                fullPath = found[0];
                Debug.Log($"[SM Loader] Found audio by search: {fullPath}");
            }
            else
            {
                Debug.LogError($"Audio file not found anywhere: {musicFile}");
                yield break;
            }
        }

        fullPath = Path.GetFullPath(fullPath);
        string uri = "file:///" + UnityWebRequest.EscapeURL(fullPath.Replace("\\", "/"));

        Debug.Log($"[SM Loader] Loading audio from: {uri}");

        AudioType audioType = AudioType.MPEG;
        string ext = Path.GetExtension(fullPath).ToLower();
        if (ext == ".ogg") audioType = AudioType.OGGVORBIS;
        else if (ext == ".wav") audioType = AudioType.WAV;

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load audio: {www.error}");
                yield break;
            }

            Music.clip = DownloadHandlerAudioClip.GetContent(www);
        }

        // Wait a bit more before starting music
        yield return new WaitForSeconds(0.5f);

        Music.Play();
        Debug.Log($"[MultiGameManager] Music started for both players");
    }

    public bool BothChartsComplete()
    {
        return Spawner_P1 != null && Spawner_P2 != null && 
               Spawner_P1.IsChartComplete() && Spawner_P2.IsChartComplete();
    }
}
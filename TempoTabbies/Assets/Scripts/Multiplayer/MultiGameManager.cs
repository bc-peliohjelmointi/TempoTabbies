using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class MultiGameManager : MonoBehaviour
{
    [Header("References")]
    public MultiNoteSpawner Spawner;
    public AudioSource Music;

    [Header("Prefabs")]
    public GameObject NotePrefab_TypeA;
    public GameObject NotePrefab_TypeB;
    public GameObject NotePrefab_TypeC;
    public GameObject NotePrefab_TypeD;

    [Header("Layout - Player 1")]
    public Transform LaneParent_P1;
    public Transform HitLine_P1;

    [Header("Layout - Player 2")]
    public Transform LaneParent_P2;
    public Transform HitLine_P2;

    private float audioOffset = 0f;
    public static float GlobalMusicStartTime; // when audio actually starts
    public static float ChartStartTime;       // when chart started (notes spawn relative to this)

    public static MultiGameManager Instance { get; private set; }

    // Property to get corrected song time (without offset)
    public static float SongTime
    {
        get
        {
            if (Instance != null && Instance.Music != null && Instance.Music.isPlaying)
            {
                return Instance.Music.time;
            }
            return Time.time - ChartStartTime;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameSession.SelectedSong == null || GameSession.SelectedChart == null)
        {
            Debug.LogError("No song or chart selected! Please load from Song Select first.");
            return;
        }

        SMFile sm = GameSession.SelectedSong;
        SMChart chart = GameSession.SelectedChart;

        Debug.Log($"Now playing: {sm.Title} by {sm.Artist}");
        Debug.Log($"Chart: {chart.Description} ({chart.Difficulty}) - {chart.Measures.Count} measures");

        // Initialize both players' lanes
        if (LaneParent_P1 == null || LaneParent_P2 == null)
        {
            Debug.LogError("LaneParents are not assigned!");
            return;
        }

        // Set up Player 1
        Transform[] lanes_P1 = new Transform[LaneParent_P1.childCount];
        for (int i = 0; i < LaneParent_P1.childCount; i++)
            lanes_P1[i] = LaneParent_P1.GetChild(i);

        // Set up Player 2
        Transform[] lanes_P2 = new Transform[LaneParent_P2.childCount];
        for (int i = 0; i < LaneParent_P2.childCount; i++)
            lanes_P2[i] = LaneParent_P2.GetChild(i);

        // Initialize spawner for both players
        Spawner.Music = Music;
        Spawner.Lanes_P1 = lanes_P1;
        Spawner.Lanes_P2 = lanes_P2;
        Spawner.HitLine_P1 = HitLine_P1;
        Spawner.HitLine_P2 = HitLine_P2;

        Spawner.NotePrefab_TypeA = NotePrefab_TypeA;
        Spawner.NotePrefab_TypeB = NotePrefab_TypeB;
        Spawner.NotePrefab_TypeC = NotePrefab_TypeC;
        Spawner.NotePrefab_TypeD = NotePrefab_TypeD;

        Spawner.LoadChart(sm, chart);

        // Set chart start time before loading music
        ChartStartTime = Time.time;

        StartCoroutine(LoadAndStartMusic(sm));
    }

    private IEnumerator LoadAndStartMusic(SMFile sm)
    {
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

        Music.Play();
        GlobalMusicStartTime = Time.time;

        Debug.Log($"[GameManager] Music started at time 0, notes have offset applied");
    }
}
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (GameSession.SelectedSong == null || GameSession.SelectedChart == null)
        {
            Debug.LogError("[MultiGame] No song or chart selected!");
            return;
        }

        SMFile sm = GameSession.SelectedSong;
        SMChart chart = GameSession.SelectedChart;

        // Init P1
        InitializePlayer(Spawner_P1, LaneParent_P1, HitLine_P1,
            NotePrefab_TypeA_P1, NotePrefab_TypeB_P1, NotePrefab_TypeC_P1, NotePrefab_TypeD_P1, "Player 1");

        // Init P2
        InitializePlayer(Spawner_P2, LaneParent_P2, HitLine_P2,
            NotePrefab_TypeA_P2, NotePrefab_TypeB_P2, NotePrefab_TypeC_P2, NotePrefab_TypeD_P2, "Player 2");

        // Load charts
        Spawner_P1.LoadChart(sm, chart);
        Spawner_P2.LoadChart(sm, chart);

        // Reset timing (DON’T start counting yet)
        GameManager.ChartStartTime = Time.time;
        GameManager.GlobalMusicStartTime = 0f;

        StartCoroutine(LoadAndStartMusic(sm));
    }

    private void InitializePlayer(NoteSpawner spawner, Transform laneParent, Transform hitLine,
        GameObject prefabA, GameObject prefabB, GameObject prefabC, GameObject prefabD, string playerName)
    {
        if (spawner == null)
        {
            Debug.LogError($"{playerName}: Spawner is not assigned!");
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
    }

    private IEnumerator LoadAndStartMusic(SMFile sm)
    {
        string musicFile = sm.MusicFile;
        if (string.IsNullOrEmpty(musicFile))
        {
            Debug.LogError("[MultiGame] Chart has no #MUSIC file.");
            yield break;
        }

        // Locate actual audio file
        string fullPath = FindMusicFile(sm);
        if (fullPath == null)
        {
            Debug.LogError("[MultiGame] Could not find audio file!");
            yield break;
        }

        Debug.Log("[MultiGame] Found audio: " + fullPath);

        // Convert to safe URI
        Uri uri = new Uri(fullPath);
        string url = uri.AbsoluteUri;

        AudioType audioType = DetectAudioType(fullPath);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError("[MultiGame] Music load error: " + www.error);
                yield break;
            }

            Music.clip = DownloadHandlerAudioClip.GetContent(www);
        }

        // ------- CRASH FIX -------
        // Wait one frame so Music.isPlaying becomes TRUE
        yield return null;

        Music.Play();
        GameManager.GlobalMusicStartTime = Time.time;

        Debug.Log("[MultiGame] Music started!");
    }

    private string FindMusicFile(SMFile sm)
    {
        string songDir = Path.GetDirectoryName(sm.FilePath);
        string audioPath = Path.Combine(songDir, sm.MusicFile);

        if (File.Exists(audioPath))
            return audioPath;

        // Search fallback
        string songsRoot = Path.Combine(Application.dataPath, "Songs");
        string[] found = Directory.GetFiles(songsRoot, sm.MusicFile, SearchOption.AllDirectories);

        if (found.Length > 0)
            return found[0];

        return null;
    }

    private AudioType DetectAudioType(string file)
    {
        string ext = Path.GetExtension(file).ToLower();

        if (ext == ".ogg") return AudioType.OGGVORBIS;
        if (ext == ".wav") return AudioType.WAV;
        return AudioType.MPEG; // mp3 default
    }

    public bool BothChartsComplete()
    {
        bool p1 = Spawner_P1 != null && Spawner_P1.IsChartComplete();
        bool p2 = Spawner_P2 != null && Spawner_P2.IsChartComplete();
        return p1 && p2;
    }
}

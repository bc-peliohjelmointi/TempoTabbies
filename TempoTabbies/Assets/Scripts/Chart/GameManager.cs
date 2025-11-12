using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public NoteSpawner Spawner;
    public AudioSource Music;

    [Header("Prefabs")]
    public GameObject NotePrefab_TypeA;
    public GameObject NotePrefab_TypeB;
    public GameObject NotePrefab_TypeC;
    public GameObject NotePrefab_TypeD;

    [Header("Layout")]
    public Transform LaneParent;
    public Transform HitLine;

    private float audioOffset = 0f;
    public static float GlobalMusicStartTime; // when audio actually starts
    public static float ChartStartTime;       // when chart started (notes spawn relative to this)
                                              // ADD THIS - Make GameManager a singleton for easy access
    public static GameManager Instance { get; private set; }

    // ADD THIS - Property to get corrected song time (without offset)
    public static float SongTime
    {
        get
        {
            if (Instance != null && Instance.Music != null && Instance.Music.isPlaying)
            {
                // For positive offsets, Music.time starts from 0 after the delay
                // For negative offsets, Music.time starts from the offset value
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


        if (LaneParent == null)
        {
            Debug.LogError("LaneParent is not assigned!");
            return;
        }

        Transform[] lanes = new Transform[LaneParent.childCount];
        for (int i = 0; i < LaneParent.childCount; i++)
            lanes[i] = LaneParent.GetChild(i);

        Spawner.Music = Music;
        Spawner.HitLine = HitLine;
        Spawner.Lanes = lanes;

        Spawner.NotePrefab_TypeA = NotePrefab_TypeA;
        Spawner.NotePrefab_TypeB = NotePrefab_TypeB;
        Spawner.NotePrefab_TypeC = NotePrefab_TypeC;
        Spawner.NotePrefab_TypeD = NotePrefab_TypeD;

        Spawner.LoadChart(sm, chart);

        // --- Set chart start time before loading music ---
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


    /*private IEnumerator StartMusicWithOffset()
    {
        // AudioSource already exists in the scene
        if (audioOffset > 0)
        {
            yield return new WaitForSeconds(audioOffset);
            Music.Play();
            GlobalMusicStartTime = Time.time;
        }
        else if (audioOffset < 0)
        {
            Music.time = Mathf.Abs(audioOffset);
            Music.Play();
            GlobalMusicStartTime = Time.time;
        }
        else
        {
            Music.Play();
            GlobalMusicStartTime = Time.time;
        }
    }*/
}

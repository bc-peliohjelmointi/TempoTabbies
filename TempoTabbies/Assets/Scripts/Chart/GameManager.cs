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
    public static float GlobalMusicStartTime;


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

        audioOffset = 0f;

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

        // Load audio file dynamically (.mp3 or .ogg)
        StartCoroutine(LoadAndStartMusic(sm));
    }

    private IEnumerator LoadAndStartMusic(SMFile sm)
    {
        // --- Find the directory where the SM file is located ---
        string songDir = Path.GetDirectoryName(sm.FilePath);
        string songsRoot = Path.Combine(Application.dataPath, "Songs");

        string musicFile = sm.MusicFile;
        if (string.IsNullOrEmpty(musicFile))
        {
            Debug.LogError($"No MUSIC tag found in SM file for {sm.Title}");
            yield break;
        }

        // --- Try path relative to the .sm file first ---
        string fullPath = Path.Combine(songDir, musicFile);

        if (!File.Exists(fullPath))
        {
            // If not found, search the entire Songs folder for the file
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

        // --- Normalize path for UnityWebRequest ---
        fullPath = Path.GetFullPath(fullPath);
        string uri = "file:///" + UnityWebRequest.EscapeURL(fullPath.Replace("\\", "/"));

        Debug.Log($"[SM Loader] Loading audio from: {uri}");

        // --- Detect file type ---
        AudioType audioType = AudioType.MPEG;
        string ext = Path.GetExtension(fullPath).ToLower();
        if (ext == ".ogg") audioType = AudioType.OGGVORBIS;
        else if (ext == ".wav") audioType = AudioType.WAV;

        // --- Load the audio ---
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

        // --- Wait for offset and start playback ---
        yield return StartCoroutine(StartMusicWithOffset());
    }



    private IEnumerator StartMusicWithOffset()
    {
        if (Music.clip == null)
        {
            Debug.LogError("Audio clip not loaded!");
            yield break;
        }

        if (audioOffset > 0)
        {
            Debug.Log($"Delaying music start by {audioOffset}s");
            yield return new WaitForSeconds(audioOffset);
            Music.Play();
            GameManager.GlobalMusicStartTime = Time.time - audioOffset;
        }
        else if (audioOffset < 0)
        {
            float startTime = Mathf.Abs(audioOffset);
            Debug.Log($"Starting music early by {startTime}s");
            Music.time = startTime;
            Music.Play();
            GameManager.GlobalMusicStartTime = Time.time + audioOffset;
        }
        else
        {
            Music.Play();
            GameManager.GlobalMusicStartTime = Time.time;
        }
    }




}

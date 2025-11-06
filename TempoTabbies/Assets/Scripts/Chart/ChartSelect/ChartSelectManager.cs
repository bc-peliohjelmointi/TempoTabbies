using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChartSelectManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform SongListParent;
    public GameObject SongButtonPrefab;

    [Header("Song Folder")]
    public string SongsFolder = "Songs";

    private List<SMFile> loadedSongs = new();

    void Start()
    {
        LoadAllSongs();
        PopulateSongList();
    }

    void LoadAllSongs()
    {
        string fullPath = Path.Combine(Application.dataPath, SongsFolder);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError("Songs folder not found at: " + fullPath);
            return;
        }

        foreach (string dir in Directory.GetDirectories(fullPath))
        {
            foreach (string smFile in Directory.GetFiles(dir, "*.sm"))
            {
                SMFile sm = SMParser.Parse(smFile);
                if (sm != null && sm.Charts.Count > 0)
                {
                    loadedSongs.Add(sm);
                }
            }
        }

        Debug.Log($"Loaded {loadedSongs.Count} song(s).");
    }

    void PopulateSongList()
    {
        foreach (var sm in loadedSongs)
        {
            GameObject buttonObj = Instantiate(SongButtonPrefab, SongListParent);
            SongButton btn = buttonObj.GetComponent<SongButton>();

            if (btn != null)
                btn.Setup(sm, this);
        }
    }

    public void OnChartSelected(SMFile song, SMChart chart)
    {
        // Pass selected song/chart to GameManager
        PlayerPrefs.SetString("SelectedSongPath", song.Title); // optional
        GameSession.SelectedSong = song;
        GameSession.SelectedChart = chart;

        // Load gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChartTest");
    }
}

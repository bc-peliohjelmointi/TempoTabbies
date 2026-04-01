using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class SongButtonsActualButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public SongButton songButton;
    public AudioSource Music;
    public void OnDeselect(BaseEventData eventData)
    {
        Music.Stop();
    }
    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(LoadAndStartMusic(songButton.currentSong));
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

        Debug.Log($"[GameManager] Music started at time 0, notes have offset applied");
    }
}

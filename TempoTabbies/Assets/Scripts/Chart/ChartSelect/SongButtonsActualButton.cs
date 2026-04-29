using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class SongButtonsActualButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public SongButton songButton;
    public AudioSource Music;
    private _GameManager _gm;

    public void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(StopMusic());
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!Music.isPlaying)
        {
            StartCoroutine(ChangeSongs());
        }
    }

    private IEnumerator StopMusic()
    {
        yield return new WaitForSeconds(0.01f);
        if (Music.isPlaying)
        {
            Music.Stop();
        }
    }

    private IEnumerator ChangeSongs()
    {
        yield return new WaitForSeconds(0.001f);
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            songButton.SaveThisButton();
            yield return new WaitForSeconds(0.5f);
            if (_gm == null) _gm = FindFirstObjectByType<_GameManager>();
            if (_gm.allAudioSources.Count < 4)
            {
                foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                {
                    _gm.allAudioSources.Add(source);
                }
            }
            StartCoroutine(LoadAndStartMusic(songButton.currentSong));
        }
    }

    private IEnumerator LoadAndStartMusic(SMFile sm)
    {
        if (Music.clip == null)
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
                DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)www.downloadHandler;
                dlHandler.streamAudio = true;

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to load audio: {www.error}");
                    yield break;
                }

                Music.clip = dlHandler.audioClip;
            }
        }
        foreach (AudioSource source in _gm.allAudioSources)
        {
            if (source != Music)
            {
                source.Stop();
            }
        }
        Music.time = sm.chartStartOffset;
        Music.Play();

        Debug.Log($"[GameManager] Music started at time 0, notes have offset applied");
    }
}

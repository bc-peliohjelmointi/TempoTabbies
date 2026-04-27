using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class Tutorial : MonoBehaviour
{
    private _GameManager gm;
    public NoteSpawner noteSpawner;

    public GameObject tutorial;
    public VideoPlayer videoPlayer;

    public bool goNext;

    public enum TutorialStage
    {
        Video,
        Game
    }
    public TutorialStage stage;

    private void Start()
    {
        gm = FindFirstObjectByType<_GameManager>();
        videoPlayer = GetComponent<VideoPlayer>();
        if (gm.tutorial)
        {
            tutorial.SetActive(true);
            stage = TutorialStage.Video;
        }
        else
        {
            tutorial.SetActive(false);
            videoPlayer.Stop();
        }
    }

    void EndReached(VideoPlayer vp)
    {
        stage = TutorialStage.Game;
    }

    private void Update()
    {
        gm.p1.showButtons = true;
        if (gm.tutorial)
        {
            switch (stage)
            {
                case TutorialStage.Video:
                    videoPlayer.loopPointReached += EndReached;
                    break;

                case TutorialStage.Game:
                    tutorial.SetActive(false);
                    break;
            }
        }
    }
}

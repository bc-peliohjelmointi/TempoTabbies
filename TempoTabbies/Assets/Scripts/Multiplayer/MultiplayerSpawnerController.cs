using UnityEngine;
public class MultiplayerSpawnerController : MonoBehaviour
{
    [Header("Spawners")]
    public NoteSpawner PrimarySpawner;
    public NoteSpawner SecondarySpawner;

    
    public SMFile PrimarySM;
    public SMChart PrimaryChart;
    public SMFile SecondarySM;
    public SMChart SecondaryChart;

    public float SecondaryScrollSpeed = 0f;
    public Transform[] SecondaryLanes;
    public Transform SecondaryHitLine;
    public HitManager SecondaryHitManager;

    void Start()
    {
        
        StartMultiplayer();
    }

    // Call to configure and start multiplayer spawners.
    public void StartMultiplayer()
    {
        if (PrimarySpawner == null || SecondarySpawner == null)
        {
            Debug.LogError("[MultiplayerSpawnerController] Assign both PrimarySpawner and SecondarySpawner in inspector.");
            return;
        }

        // Read GameSession values set at selection time
        if (PrimarySM == null || PrimaryChart == null)
        {
            if (GameSession.SelectedSongP1 != null && GameSession.SelectedChartP1 != null)
            {
                PrimarySM = GameSession.SelectedSongP1;
                PrimaryChart = GameSession.SelectedChartP1;
            }
        }

        if (SecondarySM == null || SecondaryChart == null)
        {
            if (GameSession.SelectedSongP2 != null && GameSession.SelectedChartP2 != null)
            {
                SecondarySM = GameSession.SelectedSongP2;
                SecondaryChart = GameSession.SelectedChartP2;
            }
        }

        SecondarySpawner.Music = PrimarySpawner.Music;

        // Apply optional overrides for secondary spawner
        if (SecondaryScrollSpeed > 0f)
            SecondarySpawner.ScrollSpeed = SecondaryScrollSpeed;

        if (SecondaryLanes != null && SecondaryLanes.Length > 0)
            SecondarySpawner.Lanes = SecondaryLanes;

        if (SecondaryHitLine != null)
            SecondarySpawner.HitLine = SecondaryHitLine;

        if (SecondaryHitManager != null)
            SecondarySpawner.hitManager = SecondaryHitManager;

        // Load charts into each spawner (they parse and keep their own notes lists)
        if (PrimarySM != null && PrimaryChart != null)
            PrimarySpawner.LoadChart(PrimarySM, PrimaryChart);
        else
            Debug.LogWarning("[MultiplayerSpawnerController] Primary chart not assigned or found in GameSession.");

        if (SecondarySM != null && SecondaryChart != null)
            SecondarySpawner.LoadChart(SecondarySM, SecondaryChart);
        else
            Debug.LogWarning("[MultiplayerSpawnerController] Secondary chart not assigned or found in GameSession.");

        if (PrimarySpawner.Music != null && !PrimarySpawner.Music.isPlaying)
        {
            PrimarySpawner.Music.Play();
        }
    }
}
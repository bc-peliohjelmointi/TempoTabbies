using UnityEngine;

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

    private float audioOffset = 0f; // <-- from chart or player setting

    void Start()
    {
        string path = Application.dataPath + "/Songs/zunda/zundasolo.sm";
        SMFile sm = SMParser.Parse(path);

        if (sm.Charts.Count == 0)
        {
            Debug.LogError("No dance-solo chart found in SM file!");
            return;
        }

        SMChart chart = sm.Charts[0];
        Debug.Log($"Loaded chart with {chart.Measures.Count} measures.");

        //  Read offset from SM file if available
        audioOffset = sm.Offset; // StepMania OFFSET = time before chart starts (usually negative)

        // Setup lanes
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

        //  Start playback with offset correction
        StartCoroutine(StartMusicWithOffset());
    }

    private System.Collections.IEnumerator StartMusicWithOffset()
    {
        if (audioOffset > 0)
        {
            // StepMania: positive offset means music starts later
            yield return new WaitForSeconds(audioOffset);
            Music.Play();
        }
        else
        {
            // Negative offset means start music earlier
            Music.time = Mathf.Abs(audioOffset);
            Music.Play();
        }
    }

}

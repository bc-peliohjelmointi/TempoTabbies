using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Audio + Timing")]
    public AudioSource Music;
    public float ScrollSpeed = 6f;
    public float SpawnLeadTime = 2f;

    [Header("Lane Setup")]
    public Transform[] Lanes;
    public Transform HitLine;

    [Header("Score Reference")]
    public HitManager hitManager;

    [Header("Tap Prefabs by Lane Group")]
    public GameObject NotePrefab_TypeA; // lanes 0,3
    public GameObject NotePrefab_TypeB; // lanes 1,2
    public GameObject NotePrefab_TypeC; // lane 4
    public GameObject NotePrefab_TypeD; // lane 5

    [Header("Hold Prefabs by Lane Group")]
    public GameObject HoldBodyPrefab_TypeA;
    public GameObject HoldBodyPrefab_TypeB;
    public GameObject HoldBodyPrefab_TypeC;
    public GameObject HoldBodyPrefab_TypeD;

    public GameObject HoldEndPrefab_TypeA;
    public GameObject HoldEndPrefab_TypeB;
    public GameObject HoldEndPrefab_TypeC;
    public GameObject HoldEndPrefab_TypeD;

    private List<SMTiming.ParsedNote> notes;
    private int nextIndex = 0;
    private HashSet<int> skipIndices = new HashSet<int>();

    public void LoadChart(SMFile sm, SMChart chart)
    {
        notes = SMTiming.GetNoteTimes(sm, chart);
        notes.Sort((a, b) => a.time.CompareTo(b.time));
        nextIndex = 0;
        skipIndices.Clear();

        // Get EXACT judgment note count from the parser
        int judgmentNotes = SMParser.CountJudgmentNotes(chart);

        Debug.Log($"[Score Init] Total judgment notes: {judgmentNotes}");

        // INITIALIZE SCORE MANAGER with exact count
        if (hitManager != null)
        {
            hitManager.InitializeChart(judgmentNotes);
        }
        else
        {
            Debug.LogError("HitManager reference is null in NoteSpawner!");
        }

        Debug.Log($"[NoteSpawner] Loaded chart with {judgmentNotes} judgment notes");
    }

    void Update()
    {
        if (notes == null || notes.Count == 0) return;
        if (!Music || !HitLine || Lanes == null || Lanes.Length == 0) return;

        if (!Music.isPlaying) return;

        float songTime = GameManager.SongTime;

        // ADD THIS DEBUG LOG:
        if (nextIndex == 0 && notes.Count > 0)
        {
            Debug.Log($"[NoteSpawner] First note time: {notes[0].time}, Current song time: {songTime}, Music.time: {Music.time}");
        }
        while (nextIndex < notes.Count && notes[nextIndex].time - songTime < SpawnLeadTime)
        {
            if (skipIndices.Contains(nextIndex))
            {
                nextIndex++;
                continue;
            }

            var noteData = notes[nextIndex];

            if (AssistTickManager.Instance != null)
            {
                // Play ticks for tap notes AND hold starts, but NOT hold ends
                if (noteData.type == '1' || noteData.type == '2') // Taps and hold starts only
                {
                    AssistTickManager.Instance.ScheduleTick(noteData.time);
                }
                if (noteData.lane < 0 || noteData.lane >= Lanes.Length)
                {
                    nextIndex++;
                    continue;
                }

                Transform lane = Lanes[noteData.lane];
                float timeUntilHit = noteData.time - songTime;
                float spawnY = HitLine.position.y + timeUntilHit * ScrollSpeed;
                Vector3 spawnPos = new Vector3(lane.position.x, spawnY, lane.position.z);

                // Handle hold notes as before
                if (noteData.type == '2')
                {
                    var endNote = FindHoldEnd(noteData.lane, nextIndex);
                    if (endNote.HasValue)
                    {
                        int endIndex = notes.IndexOf(endNote.Value);
                        if (endIndex >= 0) skipIndices.Add(endIndex);

                        GameObject holdRoot = new GameObject($"HoldNote_Lane{noteData.lane}");
                        holdRoot.transform.parent = transform;
                        holdRoot.transform.position = lane.position;

                        HoldNote hold = holdRoot.AddComponent<HoldNote>();
                        hold.StartTime = noteData.time;
                        hold.EndTime = endNote.Value.time;
                        hold.ScrollSpeed = ScrollSpeed;
                        hold.Music = Music;
                        hold.HitLine = HitLine;
                        hold.Lane = noteData.lane;

                        GameObject headPrefab = GetTapPrefabForLane(noteData.lane);
                        GameObject bodyPrefab = GetHoldBodyPrefabForLane(noteData.lane);
                        GameObject endPrefab = GetHoldEndPrefabForLane(noteData.lane);

                        hold.Head = Instantiate(headPrefab, holdRoot.transform);
                        hold.Body = Instantiate(bodyPrefab, holdRoot.transform);
                        hold.End = Instantiate(endPrefab, holdRoot.transform);

                        nextIndex++;
                        continue;
                    }
                }

                // Tap notes
                GameObject tapPrefab = GetTapPrefabForLane(noteData.lane);
                if (tapPrefab == null)
                {
                    Debug.LogWarning($"No tap prefab assigned for lane {noteData.lane}");
                    nextIndex++;
                    continue;
                }

                GameObject note = Instantiate(tapPrefab, spawnPos, Quaternion.identity, transform);
                Note n = note.GetComponent<Note>();
                if (n != null)
                {
                    n.TargetTime = noteData.time;
                    n.ScrollSpeed = ScrollSpeed;
                    n.Music = Music;
                    n.HitLine = HitLine;
                    n.Lane = noteData.lane;
                }

                nextIndex++;
            }
        }
    }

    private SMTiming.ParsedNote? FindHoldEnd(int lane, int startIndex)
    {
        for (int i = startIndex + 1; i < notes.Count; i++)
        {
            if (notes[i].lane == lane && notes[i].type == '3')
                return notes[i];
        }
        return null;
    }

    private GameObject GetTapPrefabForLane(int lane)
    {
        return lane switch
        {
            0 or 3 => NotePrefab_TypeA,
            1 or 2 => NotePrefab_TypeB,
            4 => NotePrefab_TypeC,
            5 => NotePrefab_TypeD,
            _ => null,
        };
    }

    private GameObject GetHoldBodyPrefabForLane(int lane)
    {
        return lane switch
        {
            0 or 3 => HoldBodyPrefab_TypeA,
            1 or 2 => HoldBodyPrefab_TypeB,
            4 => HoldBodyPrefab_TypeC,
            5 => HoldBodyPrefab_TypeD,
            _ => null,
        };
    }

    private GameObject GetHoldEndPrefabForLane(int lane)
    {
        return lane switch
        {
            0 or 3 => HoldEndPrefab_TypeA,
            1 or 2 => HoldEndPrefab_TypeB,
            4 => HoldEndPrefab_TypeC,
            5 => HoldEndPrefab_TypeD,
            _ => null,
        };
    }
}

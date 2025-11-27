using System.Collections.Generic;
using UnityEngine;

public class MultiNoteSpawner : MonoBehaviour
{ }
   /* [Header("Audio + Timing")]
    public AudioSource Music;
    public float ScrollSpeed = 6f;
    public float SpawnLeadTime = 2f;

    [Header("Lane Setup - Player 1")]
    public Transform[] Lanes_P1;
    public Transform HitLine_P1;

    [Header("Lane Setup - Player 2")]
    public Transform[] Lanes_P2;
    public Transform HitLine_P2;

    [Header("Score Reference")]
    public MultiHitManager hitManager;

    [Header("Tap Prefabs by Lane Group")]
    public GameObject NotePrefab_TypeA;
    public GameObject NotePrefab_TypeB;
    public GameObject NotePrefab_TypeC;
    public GameObject NotePrefab_TypeD;

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
        if (!IsAudioAndLanesValid()) return;
        if (!Music.isPlaying) return;

        float songTime = MultiGameManager.SongTime;

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
                if (noteData.type == '1' || noteData.type == '2')
                {
                    AssistTickManager.Instance.ScheduleTick(noteData.time);
                }
            }

            if (noteData.lane < 0 || noteData.lane >= Lanes_P1.Length)
            {
                nextIndex++;
                continue;
            }

            // Spawn notes for both players
            SpawnNoteForPlayer(0, noteData, songTime);
            SpawnNoteForPlayer(1, noteData, songTime);

            nextIndex++;
        }
    }

    private bool IsAudioAndLanesValid()
    {
        if (Music == null) return false;
        if (Lanes_P1 == null || Lanes_P1.Length == 0 || Lanes_P2 == null || Lanes_P2.Length == 0) return false;
        if (HitLine_P1 == null || HitLine_P2 == null) return false;

        // Check if any lane transforms are destroyed
        foreach (var lane in Lanes_P1)
        {
            if (lane == null) return false;
        }
        foreach (var lane in Lanes_P2)
        {
            if (lane == null) return false;
        }

        return true;
    }

    private void SpawnNoteForPlayer(int playerIndex, SMTiming.ParsedNote noteData, float songTime)
    {
        Transform[] lanes = playerIndex == 0 ? Lanes_P1 : Lanes_P2;
        Transform hitLine = playerIndex == 0 ? HitLine_P1 : HitLine_P2;

        // Add comprehensive null checks
        if (lanes == null || hitLine == null)
        {
            Debug.LogError($"Lanes or HitLine is null for player {playerIndex}");
            return;
        }

        if (noteData.lane < 0 || noteData.lane >= lanes.Length)
        {
            Debug.LogWarning($"Invalid lane index {noteData.lane} for player {playerIndex}");
            return;
        }

        Transform lane = lanes[noteData.lane];
        if (lane == null)
        {
            Debug.LogError($"Lane {noteData.lane} is null for player {playerIndex}");
            return;
        }

        // Check if the objects are still valid
        if (lane == null || hitLine == null)
        {
            Debug.LogError("Lane or HitLine has been destroyed");
            return;
        }

        float timeUntilHit = noteData.time - songTime;
        float spawnY = hitLine.position.y + timeUntilHit * ScrollSpeed;
        Vector3 spawnPos = new Vector3(lane.position.x, spawnY, lane.position.z);

        // Handle hold notes
        if (noteData.type == '2')
        {
            var endNote = FindHoldEnd(noteData.lane, nextIndex);
            if (endNote.HasValue)
            {
                int endIndex = notes.IndexOf(endNote.Value);
                if (endIndex >= 0) skipIndices.Add(endIndex);

                GameObject holdRoot = new GameObject($"HoldNote_Player{playerIndex + 1}_Lane{noteData.lane}");
                holdRoot.transform.parent = transform;
                holdRoot.transform.position = lane.position;

                HoldNote hold = holdRoot.AddComponent<HoldNote>();
                hold.StartTime = noteData.time;
                hold.EndTime = endNote.Value.time;
                hold.ScrollSpeed = ScrollSpeed;
                hold.Music = Music;
                hold.HitLine = hitLine;
                hold.Lane = noteData.lane;

                GameObject headPrefab = GetTapPrefabForLane(noteData.lane);
                GameObject bodyPrefab = GetHoldBodyPrefabForLane(noteData.lane);
                GameObject endPrefab = GetHoldEndPrefabForLane(noteData.lane);

                if (headPrefab != null) hold.Head = Instantiate(headPrefab, holdRoot.transform);
                if (bodyPrefab != null) hold.Body = Instantiate(bodyPrefab, holdRoot.transform);
                if (endPrefab != null) hold.End = Instantiate(endPrefab, holdRoot.transform);

                return;
            }
        }

        // Tap notes
        if (noteData.type == '1')
        {
            GameObject tapPrefab = GetTapPrefabForLane(noteData.lane);
            if (tapPrefab == null)
            {
                Debug.LogWarning($"No tap prefab assigned for lane {noteData.lane}");
                return;
            }

            GameObject note = Instantiate(tapPrefab, spawnPos, Quaternion.identity, transform);
            note.name = $"Note_Player{playerIndex + 1}_Lane{noteData.lane}";

            Note n = note.GetComponent<Note>();
            if (n != null)
            {
                n.TargetTime = noteData.time;
                n.ScrollSpeed = ScrollSpeed;
                n.Music = Music;
                n.HitLine = hitLine;
                n.Lane = noteData.lane;
            }
        }
    }

    public bool IsChartComplete()
    {
        return notes != null && nextIndex >= notes.Count;
    }

    public float LastNoteTime
    {
        get
        {
            if (notes == null || notes.Count == 0)
                return 0f;
            return notes[notes.Count - 1].time;
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
}*/
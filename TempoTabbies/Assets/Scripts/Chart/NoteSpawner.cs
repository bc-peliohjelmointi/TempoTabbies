using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public int playerID;
    [Header("Audio + Timing")]
    public AudioSource Music;
    public _GameManager gm;
    public HitPointManager hpManager;
    public float ScrollSpeed;
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
    public SpriteRenderer tacoSr;

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

    // SPAWN THROTTLE: maximum number of note GameObjects to instantiate per frame
    [Header("Spawn Throttle (safety)")]
    [Tooltip("Max number of notes this spawner will instantiate per frame to avoid freezes.")]
    public int maxSpawnsPerFrame = 16;

    // Optional runtime debug
    private int lastFrameSpawnedCount = 0;
    private bool _loggedSpawnSpeedAtSpawn = false;

    private void Awake()
    {
        // Get reference to GameManager
        gm = _GameManager.instance;

        // Default fallback values for diagnostics
        float gmScroll = -999f;
        float p1Scroll = -999f;
        float p2Scroll = -999f;
        string chosenSource = "none";

        if (gm != null)
        {
            if (playerID == 1)
            {
                if (gm.p1 != null)
                {
                    p1Scroll = gm.p1.scrollSpeed;
                    ScrollSpeed = p1Scroll;
                    chosenSource = "p1.scrollSpeed";
                }
                else
                {
                    ScrollSpeed = 8;
                    chosenSource = "_GameManager.scrollSpeed (fallback, p1 null)";
                }
            }
            else if (playerID == 2)
            {
                if (gm.p2 != null)
                {
                    p2Scroll = gm.p2.scrollSpeed;
                    ScrollSpeed = p2Scroll;
                    chosenSource = "p2.scrollSpeed";
                }
                else
                {
                    ScrollSpeed = 8;
                    chosenSource = "_GameManager.scrollSpeed (fallback, p2 null)";
                }
            }
            else
            {
                // any other playerID uses global scrollSpeed
                ScrollSpeed = 8;
                chosenSource = "_GameManager.scrollSpeed (playerID out of expected range)";
            }
        }
        else
        {
            Debug.LogWarning("[NoteSpawner.Awake] _GameManager.instance is null - using ScrollSpeed as-is (inspector/default).");
        }

        Debug.Log($"[NoteSpawner.Awake] gameObject='{gameObject.name}' id={gameObject.GetInstanceID()} playerID={playerID} => ScrollSpeed={ScrollSpeed} (source={chosenSource}) | gm.scrollSpeed={gmScroll} p1.scroll={p1Scroll} p2.scroll={p2Scroll}");
    }
    public void LoadChart(SMFile sm, SMChart chart)
    {
        Debug.Log($"[NoteSpawner] LoadChart started - SMFile: {sm != null}, SMChart: {chart != null}");

        try
        {
            notes = SMTiming.GetNoteTimes(sm, chart);
            Debug.Log($"[NoteSpawner] Got {notes?.Count ?? 0} parsed notes");

            if (notes != null)
            {
                notes.Sort((a, b) => a.time.CompareTo(b.time));
                Debug.Log($"[NoteSpawner] Notes sorted");
            }

            if (hpManager != null)
            {
                hpManager.diffMultiplierList = sm.SubtitleValues;
                hpManager.difficulty = chart.Difficulty;
            }

            nextIndex = 0;
            skipIndices.Clear();

            // Get EXACT judgment note count from the parser
            int judgmentNotes = SMParser.CountJudgmentNotes(chart);
            Debug.Log($"[NoteSpawner] Judgment notes count: {judgmentNotes}");

            // Ensure HitManager has a ScoreManager assigned (fallback to any in-scene ScoreManager)
            if (hitManager != null && hitManager.scoreManager == null)
            {
                var found = FindFirstObjectByType<ScoreManager>();
                if (found != null)
                {
                    hitManager.scoreManager = found;
                    Debug.Log("[NoteSpawner] Assigned Scene ScoreManager to HitManager as fallback.");
                }
                else
                {
                    Debug.LogWarning("[NoteSpawner] No ScoreManager assigned to HitManager and none found in scene.");
                }
            }

            // INITIALIZE HITMANAGER/SCORE MANAGER with exact count
            if (hitManager != null)
            {
                hitManager.InitializeChart(judgmentNotes);
                Debug.Log($"[NoteSpawner] HitManager initialized with {judgmentNotes} notes");
            }
            else
            {
                Debug.LogError("HitManager reference is null in NoteSpawner!");
            }

            Debug.Log($"[NoteSpawner] Loaded chart with {judgmentNotes} judgment notes");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NoteSpawner] Error loading chart: {e.Message}\n{e.StackTrace}");
        }
    }

    void Update()
    {
        // Basic early-exits
        if (notes == null || notes.Count == 0) return;
        if (!Music || !HitLine || Lanes == null || Lanes.Length == 0) return;

        // MUST NOT RUN until music is playing (prevents early massive spawn)
        if (Music == null || !Music.isPlaying)
            return;

        float songTime = GameManager.SongTime;

        // Debug for first note
        if (nextIndex == 0 && notes.Count > 0)
        {
            Debug.Log($"[NoteSpawner] First note time: {notes[0].time}, Current song time: {songTime}, Music.time: {Music.time}");
        }

        lastFrameSpawnedCount = 0;

        // Spawn up to maxSpawnsPerFrame notes this frame
        while (nextIndex < notes.Count && notes[nextIndex].time - songTime < SpawnLeadTime)
        {
            // Throttle: if we've spawned enough this frame, break and continue next frame
            if (lastFrameSpawnedCount >= maxSpawnsPerFrame)
            {
                break;
            }

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
                // Log the ScrollSpeed actually used for spawn (only once to avoid spam)
                if (!_loggedSpawnSpeedAtSpawn)
                {
                    Debug.Log($"[NoteSpawner] SPAWN time - gameObject='{gameObject.name}' id={gameObject.GetInstanceID()} playerID={playerID} - timeUntilHit={timeUntilHit:F3} ScrollSpeed={ScrollSpeed}");
                    _loggedSpawnSpeedAtSpawn = true;
                }
                float spawnY = HitLine.position.y + timeUntilHit * ScrollSpeed;
                Vector3 spawnPos = new Vector3(lane.position.x, spawnY, lane.position.z);

                // Handle hold notes
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

                        

                        // Null-check prefabs before instantiate
                        if (headPrefab != null) hold.Head = Instantiate(headPrefab, holdRoot.transform);
                        if (bodyPrefab != null) hold.Body = Instantiate(bodyPrefab, holdRoot.transform);
                        if (endPrefab != null) hold.End = Instantiate(endPrefab, holdRoot.transform);

                        if (hold != null && gm.taco && (hold.Lane == 4 || hold.Lane == 5))
                        {
                            hold.swipe = true;
                        }
                        if (hold != null && gm.taco && (hold.Lane != 4 || hold.Lane == 5))
                        {
                            hold.Head.GetComponent<SpriteRenderer>().sprite = tacoSr.sprite;
                        }

                        // Assign per-player managers (score, hit effects) and owner so HoldNote uses the correct input device
                        if (hitManager != null)
                        {
                            hold.hitEffectManager = hitManager.hitEffectManager;
                            hold.scoreManager = hitManager.scoreManager;
                            hold.OwnerHitManager = hitManager;
                        }

                        nextIndex++;
                        lastFrameSpawnedCount++;
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

                if (n != null && gm.taco && (n.Lane != 4 && n.Lane != 5))
                {
                    n.gameObject.GetComponent<SpriteRenderer>().sprite = tacoSr.sprite;
                }

                nextIndex++;
                lastFrameSpawnedCount++;
            }
            else
            {
                // If AssistTickManager is null, advance nextIndex to avoid infinite loops
                nextIndex++;
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
}

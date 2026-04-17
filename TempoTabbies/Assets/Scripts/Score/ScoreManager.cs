using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public PlayerScript player;
    public int playerToUse;
    [Header("Scoring Settings")]
    public int maxScore = 1010000;    // All Marvelous
    public int perfectScore = 1000000; // All Perfect

    [Header("Current Score")]
    public int currentScore = 0;
    public int totalNotes = 0;
    public int notesHit = 0;

    // Judgment counters
    public int marvelousCount = 0;
    public int perfectCount = 0;
    public int greatCount = 0;
    public int goodCount = 0;
    public int badCount = 0;
    public int missCount = 0;

    public int maxCombo = 0;

    public bool diva;
    public bool reaper;
    public bool eightLives;
    public bool catCompressions;

    private int pointsPerNote;
    private bool hasSavedScore = false;

    public HitPointManager hpManager;
    public _GameManager gm;

    // Events for real-time updates
    public System.Action<int> OnScoreChanged;
    public System.Action<float> OnAccuracyChanged;
    public System.Action<string> OnGradeChanged;
    public System.Action<int> OnComboChanged;

    public void InitializeScore(int totalNotesInChart)
    {
        totalNotes = totalNotesInChart;
        notesHit = 0;

        if (totalNotes > 0)
        {
            // Calculate how many points to deduct for each judgment type
            pointsPerNote = perfectScore / totalNotes;

            // Start with maximum possible score (all Marvelous)
            currentScore = maxScore;
        }
        else
        {
            pointsPerNote = 0;
            currentScore = 0;
        }

        ResetJudgmentCounters();

        Debug.Log($"Score initialized: {totalNotes} notes, {pointsPerNote} points per note");

        // Trigger initial updates
        OnScoreChanged?.Invoke(currentScore);
        OnAccuracyChanged?.Invoke(GetAccuracy());
        OnGradeChanged?.Invoke(GetGrade());
    }

    public void AddJudgment(string judgment)
    {
        if (judgment == "MISS" && eightLives)
        {
            eightLives = false;
            judgment = "MARVELOUS";
        }
        // Increment counters
        switch (judgment)
        {
            case "MARVELOUS": marvelousCount++; break;
            case "PERFECT": perfectCount++; break;
            case "GREAT": greatCount++; break;
            case "GOOD": goodCount++; break;
            case "BAD": badCount++; break;
            case "MISS": missCount++; break;
        }

        notesHit++;

        // Update combo system
        UpdateCombo(judgment);

        if (hpManager != null)
        {
            hpManager.HPChange(judgment);
            Debug.Log(judgment);
        }

        // Calculate points based on judgment
        int pointsEarned = CalculatePoints(judgment);
        currentScore = pointsEarned; // Just set to calculated total

        Debug.Log($"{judgment}: Score: {currentScore}, Combo: {player.Combo}");

        // Trigger updates
        OnScoreChanged?.Invoke(currentScore);
        OnAccuracyChanged?.Invoke(GetAccuracy());
        OnGradeChanged?.Invoke(GetGrade());
    }

    public void UpdateCombo(string judgment)
    {
        // Reset combo on miss or bad
        if (judgment == "MISS" || judgment == "BAD")
        {
            if (player.Combo > maxCombo)
            {
                maxCombo = player.Combo;
            }
            player.Combo = 0;
        }
        else
        {
            // Increment combo for other judgments
            player.Combo++;
        }

        // Update combo UI/display
        OnComboChanged?.Invoke(player.Combo);
    }

    // Call this when the chart/song is complete
    public void FinalizeScore()
    {
        if (hasSavedScore)
            return;

        // Don't finalize/save the score until all notes have been judged (hit or missed).
        // Some other systems may call FinalizeScore when all notes are spawned —
        // ensure we only save once we've processed judgments for every note.
        if (totalNotes > 0 && notesHit < totalNotes)
        {
            Debug.Log($"[ScoreManager] FinalizeScore called early: notesHit {notesHit}/{totalNotes}. Skipping save.");
            return;
        }

        hasSavedScore = true;

        if (player.Combo > maxCombo)
            maxCombo = player.Combo;

        // Choose song/chart based on which player this ScoreManager belongs to (multiplayer support)
        SMFile selectedSong = null;
        SMChart selectedChart = null;

        if (playerToUse == 2)
        {
            selectedSong = GameSession.SelectedSongP2;
            selectedChart = GameSession.SelectedChartP2;
        }
        else
        {
            selectedSong = GameSession.SelectedSongP1;
            selectedChart = GameSession.SelectedChartP1;
        }

        // Fallback to legacy singleplayer accessors if necessary
        selectedSong ??= GameSession.SelectedSong;
        selectedChart ??= GameSession.SelectedChart;

        if (selectedSong == null || selectedChart == null)
        {
            Debug.LogWarning("[ScoreManager] No song/chart info for this player. Skipping DB save.");
            return;
        }

        string profileName = "Unknown";
        var gm = _GameManager.instance ?? FindFirstObjectByType<_GameManager>();
        if (gm != null)
        {
            PlayerScript chosen = null;
            if (playerToUse == 1 && gm.p1 != null) chosen = gm.p1;
            else if (playerToUse == 2 && gm.p2 != null) chosen = gm.p2;

            // fallback to any available player
            chosen ??= gm.p1 ?? gm.p2;

            if (chosen != null)
            {
                profileName = chosen.name;
            }
        }
        string mapName = selectedSong.Title;
        string difficulty = selectedChart.Difficulty;

        string clearType = "Failed";

        if (hpManager != null)
            clearType = hpManager.GetClearType(this).ToString();

        Debug.Log(
            $"[FINALIZE SAVE] {profileName} | {mapName} | {difficulty} | {currentScore} | {clearType}"
        );
        Debug.Log($"saved score at {currentScore}");

        // Find existing entry for this profile on this map/difficulty
        var existingEntries = ScoreDatabase.GetScores(mapName, difficulty);
        bool found = false;
        ScoreEntry existingEntry = default;
        if (existingEntries != null)
        {
            foreach (var e in existingEntries)
            {
                if (e.profileName == profileName)
                {
                    found = true;
                    existingEntry = e;
                    break;
                }
            }
        }

        // If no existing entry, save full score (first time for this profile)
        if (!found)
        {
            ScoreDatabase.SaveScore(
                profileName,
                mapName,
                difficulty,
                currentScore,
                GetAccuracy(),
                GetGrade(),
                maxCombo,
                clearType
            );
            return;
        }

        // If the new score is greater or equal than the existing stored score, save full entry
        if (currentScore >= existingEntry.score)
        {
            // Make sure we don't downgrade the stored clearType when saving a higher score.
            string clearToSave = clearType;
            if (Enum.TryParse<HitPointManager.ClearType>(existingEntry.clearType, out var existingClear) &&
                Enum.TryParse<HitPointManager.ClearType>(clearType, out var newClear))
            {
                if (existingClear > newClear)
                {
                    // preserve the higher existing clear
                    clearToSave = existingEntry.clearType;
                }
            }

            ScoreDatabase.SaveScore(
                profileName,
                mapName,
                difficulty,
                currentScore,
                GetAccuracy(),
                GetGrade(),
                maxCombo,
                clearToSave
            );
            Debug.Log($"[FINALIZE SAVE] Saved full score for {profileName} (score {currentScore}).");
            return;
        }

        // New score is lower than existing. Only update clearType if new clear is strictly higher than existing clear.
        if (Enum.TryParse<HitPointManager.ClearType>(clearType, out var newClr) &&
            Enum.TryParse<HitPointManager.ClearType>(existingEntry.clearType, out var existingClr))
        {
            if (newClr > existingClr)
            {
                ScoreDatabase.UpdateClearType(profileName, mapName, difficulty, clearType);
                Debug.Log($"[FINALIZE SAVE] Updated clear type for {profileName} from {existingClr} to {newClr}.");
            }
            else
            {
                Debug.Log($"[FINALIZE SAVE] Not saving: clear type {newClr} is not higher than existing {existingClr} for profile {profileName}.");
            }
        }
        else
        {
            Debug.Log($"[FINALIZE SAVE] Clear type parsing failed; not updating.");
        }
    }



    private int CalculatePoints(string judgment)
    {
        // Calculate total points from all judgments so far
        int totalPoints = 0;
        float marv = 1.01f;
        float perf = 1f;
        float great = 0.75f;
        float good = 0.3f;
        float bad = 0.15f;
        float miss = 0f;
        if (diva)
        {
            marv += 0.04f;
        }
        if (reaper)
        {
            great -= 0.05f;
            good -= 0.05f;
            bad -= 0.05f;
            miss -= 0.15f;
        }
        if (catCompressions)
        {
            bad += 0.05f;
            miss += 0.05f;
        }
        totalPoints += marvelousCount * Mathf.RoundToInt(pointsPerNote * marv);
        totalPoints += perfectCount * Mathf.RoundToInt(pointsPerNote * perf);
        totalPoints += greatCount * Mathf.RoundToInt(pointsPerNote * great);
        totalPoints += goodCount * Mathf.RoundToInt(pointsPerNote * good);
        totalPoints += badCount * Mathf.RoundToInt(pointsPerNote * bad);
        totalPoints += missCount * Mathf.RoundToInt(pointsPerNote * miss);

        return totalPoints;
    }

    // Calculate accuracy percentage
    public float GetAccuracy()
    {
        if (totalNotes == 0) return 0f;

        float weightedScore =
            (marvelousCount * 1.01f) +
            (perfectCount * 1.0f) +
            (greatCount * 0.75f) +
            (goodCount * 0.30f) +
            (badCount * 0.15f);

        return (weightedScore / totalNotes) * 100f;
    }

    // Get current grade based on accuracy
    public string GetGrade()
    {
        float accuracy = GetAccuracy();

        if (currentScore >= 1010000) return "MAX";
        if (currentScore >= 1009000) return "SSS+";
        if (currentScore >= 1007500) return "SSS";
        if (currentScore >= 1005000) return "SS+";
        if (currentScore >= 1000000) return "SS";
        if (currentScore >= 990000) return "S+";
        if (currentScore >= 975000) return "S";
        if (currentScore >= 950000) return "AAA";
        if (currentScore >= 925000) return "AA";
        if (currentScore >= 900000) return "A";
        if (currentScore >= 800000) return "BBB";
        if (currentScore >= 700000) return "BB";
        if (currentScore >= 600000) return "B";
        if (currentScore >= 500000) return "C";
        return "D";
    }

    public void ResetScore()
    {
        currentScore = 0;
        totalNotes = 0;
        notesHit = 0;
        player.Combo = 0;
        ResetJudgmentCounters();

        OnScoreChanged?.Invoke(currentScore);
        OnAccuracyChanged?.Invoke(GetAccuracy());
        OnGradeChanged?.Invoke(GetGrade());
    }

    private void ResetJudgmentCounters()
    {
        marvelousCount = 0;
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        badCount = 0;
        ResetCombo();
    }

    public void ResetCombo()
    {
        player.Combo = 0;
        maxCombo = 0;
        OnComboChanged?.Invoke(player.Combo);
    }

    public string GetScoreBreakdown()
    {
        return $"Score: {currentScore:N0}\nAccuracy: {GetAccuracy():F2}%\nGrade: {GetGrade()}";
    }

    public string GetComboInfo()
    {
        return $"{player.Combo}x";
    }

    private void Awake()
    {
        gm = FindFirstObjectByType<_GameManager>();
        if (playerToUse == 1)
        {
            player = gm.p1;
        }
        else
        {
            player = gm.p2;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [Header("Scoring Settings")]
    public int maxScore = 1010000;    // All Marvelous
    public int perfectScore = 1000000; // All Perfect

    [Header("Current Score")]
    public int currentScore = 0;
    public int totalNotes = 0;
    public int notesHit = 0;
    public int currentCombo = 0;


    // Judgment counters
    public int marvelousCount = 0;
    public int perfectCount = 0;
    public int greatCount = 0;
    public int goodCount = 0;
    public int badCount = 0;
    public int missCount = 0;

    public int maxCombo = 0;

    private int pointsPerNote;

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

        // Calculate points based on judgment
        int pointsEarned = CalculatePoints(judgment);
        currentScore = pointsEarned; // Just set to calculated total

        Debug.Log($"{judgment}: Score: {currentScore}, Combo: {currentCombo}");

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
            if (currentCombo > maxCombo)
            {
                maxCombo = currentCombo;
            }
            currentCombo = 0;
        }
        else
        {
            // Increment combo for other judgments
            currentCombo++;
        }

        // Update combo UI/display
        OnComboChanged?.Invoke(currentCombo);
    }

    // Call this when the chart/song is complete
    public void FinalizeScore()
    {
        // Finalize max combo - check the current combo at the end
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }
    }

    private int CalculatePoints(string judgment)
    {
        // Calculate total points from all judgments so far
        int totalPoints = 0;

        totalPoints += marvelousCount * Mathf.RoundToInt(pointsPerNote * 1.01f);
        totalPoints += perfectCount * Mathf.RoundToInt(pointsPerNote * 1.00f);
        totalPoints += greatCount * Mathf.RoundToInt(pointsPerNote * 0.75f);
        totalPoints += goodCount * Mathf.RoundToInt(pointsPerNote * 0.30f);
        totalPoints += badCount * Mathf.RoundToInt(pointsPerNote * 0.15f);
        // Misses add 0 points

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
        currentCombo = 0;
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
        currentCombo = 0;
        maxCombo = 0;
        OnComboChanged?.Invoke(currentCombo);
    }

    public string GetScoreBreakdown()
    {
        return $"Score: {currentScore:N0}\nAccuracy: {GetAccuracy():F2}%\nGrade: {GetGrade()}";
    }

    public string GetComboInfo()
    {
        return $"{currentCombo}x";
    }
}
using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
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

    private int pointsPerNote;

    // Events for real-time updates
    public System.Action<int> OnScoreChanged;
    public System.Action<float> OnAccuracyChanged;
    public System.Action<string> OnGradeChanged;

    public void InitializeScore(int totalNotesInChart)
    {
        totalNotes = totalNotesInChart;
        notesHit = 0;

        if (totalNotes > 0)
        {
            // Calculate how many points to deduct for each judgment type
            pointsPerNote = maxScore / totalNotes;

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

        // Calculate points based on judgment
        int pointsEarned = CalculatePoints(judgment);
        currentScore = pointsEarned; // Just set to calculated total

        Debug.Log($"{judgment}: Score: {currentScore}");

        // Trigger updates
        OnScoreChanged?.Invoke(currentScore);
        OnAccuracyChanged?.Invoke(GetAccuracy());
        OnGradeChanged?.Invoke(GetGrade());
    }

    private int CalculatePoints(string judgment)
    {
        // Calculate total points from all judgments so far
        int totalPoints = 0;

        totalPoints += marvelousCount * Mathf.RoundToInt(pointsPerNote * 1.01f);
        totalPoints += perfectCount * Mathf.RoundToInt(pointsPerNote * 1.00f);
        totalPoints += greatCount * Mathf.RoundToInt(pointsPerNote * 0.66f);
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
            (greatCount * 0.66f) +
            (goodCount * 0.30f) +
            (badCount * 0.15f);

        return (weightedScore / totalNotes) * 100f;
    }

    // Get current grade based on accuracy
    public string GetGrade()
    {
        float accuracy = GetAccuracy();

        if (accuracy >= 100.5f) return "AAAA";
        if (accuracy >= 99f) return "AAA";
        if (accuracy >= 95f) return "AA";
        if (accuracy >= 90f) return "A";
        if (accuracy >= 80f) return "B";
        if (accuracy >= 70f) return "C";
        if (accuracy >= 60f) return "D";
        return "E";
    }

    public void ResetScore()
    {
        currentScore = 0;
        totalNotes = 0;
        notesHit = 0;
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
        missCount = 0;
    }

    public string GetScoreBreakdown()
    {
        return $"Score: {currentScore:N0}\nAccuracy: {GetAccuracy():F2}%\nGrade: {GetGrade()}";
    }
}
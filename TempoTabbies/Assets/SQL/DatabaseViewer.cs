using UnityEngine;
using System.Collections.Generic;

public class ScoreDatabaseViewer : MonoBehaviour
{
    void Start()
    {
        List<ScoreEntry> scores = ScoreDatabase.GetAllScores();

        if (scores.Count == 0)
        {
            Debug.Log("No scores in database yet.");
            return;
        }

        Debug.Log("=== SCORE DATABASE CONTENTS ===");

        foreach (var s in scores)
        {
            Debug.Log(
                $"Profile: {s.profileName} | " +
                $"Map: {s.mapName} | " +
                $"Diff: {s.difficulty} | " +
                $"Score: {s.score:N0} | " +
                $"Acc: {s.accuracy:F2}% | " +
                $"Grade: {s.grade} | " +
                $"Combo: {s.maxCombo} | " +
                $"Clear: {s.clearType} | " +
                $"Plays: {s.playCount}"
            );
        }
    }
}

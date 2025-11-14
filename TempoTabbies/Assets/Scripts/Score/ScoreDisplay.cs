using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text accuracyText;
    public TMP_Text gradeText;
    public TMP_Text judgmentText;

    [Header("Score Reference")]
    public ScoreManager scoreManager;

    void Update()
    {
        if (scoreManager != null)
        {
            // Update all UI elements in real-time
            scoreText.text = $"Score: {scoreManager.currentScore:N0}";
            accuracyText.text = $"Accuracy: {scoreManager.GetAccuracy():F2}%";
            gradeText.text = $"Grade: {scoreManager.GetGrade()}";

            // Update judgment counters
            judgmentText.text = $"MARV: {scoreManager.marvelousCount}\n" +
                               $"PERF: {scoreManager.perfectCount}\n" +
                               $"GREAT: {scoreManager.greatCount}\n" +
                               $"GOOD: {scoreManager.goodCount}\n" +
                               $"BAD: {scoreManager.badCount}\n" +
                               $"MISS: {scoreManager.missCount}";
        }
    }
}
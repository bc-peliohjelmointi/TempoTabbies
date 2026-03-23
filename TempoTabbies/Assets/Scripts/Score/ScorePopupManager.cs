using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ScorePopupManager : MonoBehaviour
{
    public static ScorePopupManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject panelRoot;
    public TMP_Text headerText;
    public TMP_Text contentText;
    public TMP_Text gradetext;
    public TMP_Text cleartypetext;
    public TMP_Text playcounttext;
    public TMP_Text scoretext;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panelRoot != null) panelRoot.SetActive(false);
        Debug.Log("[ScorePopupManager] Awake - instance set");
    }

    public void ShowScores(string mapName, string difficulty)
    {
        Debug.Log($"[ScorePopupManager] ShowScores called for {mapName} / {difficulty}");
        if (panelRoot == null)
        {
            Debug.LogWarning("[ScorePopupManager] panelRoot is not assigned. Cannot show popup.");
            return;
        }
        if (contentText == null)
        {
            Debug.LogWarning("[ScorePopupManager] contentText is not assigned. Popup will still be shown but no content text will appear.");
        }

        List<ScoreEntry> entries = ScoreDatabase.GetScores(mapName, difficulty);

        if (headerText != null)
            headerText.text = $"{mapName} - {difficulty}";

        if (entries == null || entries.Count == 0)
        {
            // Friendly message when no scores exist for this map/difficulty
            if (contentText != null)
                contentText.text = "No scores yet for this chart.";
            if (gradetext != null) gradetext.text = "N/A";
            if (cleartypetext != null) cleartypetext.text = "Clear: N/A";
            if (playcounttext != null) playcounttext.text = "Play count: 0";
            if (scoretext != null) scoretext.text = "Score: N/A";
            Debug.Log("[ScorePopupManager] No scores found.");
            panelRoot.SetActive(true);
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (var e in entries)
        {
            sb.AppendLine($"{e.profileName} | Score: {e.score:N0} | Acc: {e.accuracy:F2}% | Grade: {e.grade} | Combo: {e.maxCombo} | Plays: {e.playCount}");
        }

        contentText.text = sb.ToString();
        gradetext.text = $"{entries[0].grade}";
        cleartypetext.text = $"Clear: {entries[0].clearType}";
        playcounttext.text = $"Play count: {entries[0].playCount}";
        scoretext.text = $"Score: {entries[0].score:N0}";
        panelRoot.SetActive(true);
        Debug.Log($"[ScorePopupManager] Displaying {entries.Count} score(s)");
    }

    public void Hide()
    {
        Debug.Log("[ScorePopupManager] Hide called");
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}

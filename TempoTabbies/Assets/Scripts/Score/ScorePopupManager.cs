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
    [Header("Leaderboard Rows")]
    public LeaderboardRow[] leaderboardRows;
    public LeaderboardRow currentProfileRow;

    [System.Serializable]
    public struct LeaderboardRow
    {
        public GameObject rowRoot; // optional parent root to enable/disable
        public TMP_Text indexText;
        public TMP_Text playerText;
        public TMP_Text gradeText;
        public TMP_Text scoreText;
    }

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

        // If leaderboard rows are assigned, query limited top scores for those rows
        List<ScoreEntry> entries;
        int rowCount = (leaderboardRows != null && leaderboardRows.Length > 0) ? leaderboardRows.Length : 0;
        if (rowCount > 0)
        {
            entries = ScoreDatabase.GetTopScores(mapName, difficulty, rowCount);
        }
        else
        {
            // fallback to all scores (old behavior)
            entries = ScoreDatabase.GetScores(mapName, difficulty);
        }

        if (headerText != null)
            headerText.text = $"{mapName} - {difficulty}";

        if (entries == null || entries.Count == 0)
        {
            // Friendly message when no scores exist for this map/difficulty
            // Populate rows with empty/no-score state
            if (rowCount > 0)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    var row = leaderboardRows[i];
                    if (row.rowRoot != null) row.rowRoot.SetActive(false);
                    if (row.indexText != null) row.indexText.text = string.Empty;
                    if (row.playerText != null) row.playerText.text = string.Empty;
                    if (row.gradeText != null) row.gradeText.text = string.Empty;
                    if (row.scoreText != null) row.scoreText.text = string.Empty;
                }

                // current profile row
                if (currentProfileRow.rowRoot != null) currentProfileRow.rowRoot.SetActive(false);
            }
            else
            {
                if (contentText != null) contentText.text = "No scores yet for this chart.";
            }

            if (gradetext != null) gradetext.text = "N/A";
            if (cleartypetext != null) cleartypetext.text = "Clear: N/A";
            if (playcounttext != null) playcounttext.text = "Play count: 0";
            if (scoretext != null) scoretext.text = "Score: N/A";
            Debug.Log("[ScorePopupManager] No scores found.");
            panelRoot.SetActive(true);
            return;
        }

        // If leaderboard rows are set, populate them; otherwise fallback to contentText list
        if (rowCount > 0)
        {
            for (int i = 0; i < rowCount; i++)
            {
                var row = leaderboardRows[i];
                if (i < entries.Count)
                {
                    var e = entries[i];
                    if (row.rowRoot != null) row.rowRoot.SetActive(true);
                    if (row.indexText != null) row.indexText.text = (i + 1).ToString();
                    if (row.playerText != null) row.playerText.text = e.profileName;
                    if (row.gradeText != null) row.gradeText.text = e.grade;
                    if (row.scoreText != null) row.scoreText.text = e.score.ToString("N0");
                }
                else
                {
                    if (row.rowRoot != null) row.rowRoot.SetActive(false);
                    if (row.indexText != null) row.indexText.text = string.Empty;
                    if (row.playerText != null) row.playerText.text = string.Empty;
                    if (row.gradeText != null) row.gradeText.text = string.Empty;
                    if (row.scoreText != null) row.scoreText.text = string.Empty;
                }
            }

            // Fill current profile row: find matching entry by profile name
            string currentProfileName = null;
            if (_GameManager.instance != null && _GameManager.instance.p1 != null)
                currentProfileName = _GameManager.instance.p1.name;

            bool found = false;
            for (int i = 0; i < entries.Count; i++)
            {
                if (!string.IsNullOrEmpty(currentProfileName) && entries[i].profileName == currentProfileName)
                {
                    if (currentProfileRow.rowRoot != null) currentProfileRow.rowRoot.SetActive(true);
                    if (currentProfileRow.indexText != null) currentProfileRow.indexText.text = (i + 1).ToString();
                    if (currentProfileRow.playerText != null) currentProfileRow.playerText.text = entries[i].profileName;
                    if (currentProfileRow.gradeText != null) currentProfileRow.gradeText.text = entries[i].grade;
                    if (currentProfileRow.scoreText != null) currentProfileRow.scoreText.text = entries[i].score.ToString("N0");
                    found = true;
                    break;
                }
            }

            if (!found && currentProfileRow.rowRoot != null)
            {
                currentProfileRow.rowRoot.SetActive(false);
            }

            // populate top-entry summary fields from first entry
            if (entries.Count > 0)
            {
                if (gradetext != null) gradetext.text = entries[0].grade;
                if (cleartypetext != null) cleartypetext.text = $"Clear: {entries[0].clearType}";
                if (playcounttext != null) playcounttext.text = $"Play count: {entries[0].playCount}";
                if (scoretext != null) scoretext.text = $"Score: {entries[0].score:N0}";
            }
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in entries)
            {
                sb.AppendLine($"{e.profileName} | Score: {e.score:N0} | Acc: {e.accuracy:F2}% | Grade: {e.grade} | Combo: {e.maxCombo} | Plays: {e.playCount}");
            }

            if (contentText != null) contentText.text = sb.ToString();
            if (entries.Count > 0)
            {
                if (gradetext != null) gradetext.text = entries[0].grade;
                if (cleartypetext != null) cleartypetext.text = $"Clear: {entries[0].clearType}";
                if (playcounttext != null) playcounttext.text = $"Play count: {entries[0].playCount}";
                if (scoretext != null) scoretext.text = $"Score: {entries[0].score:N0}";
            }
        }
        panelRoot.SetActive(true);
        Debug.Log($"[ScorePopupManager] Displaying {entries.Count} score(s)");
    }

    public void Hide()
    {
        Debug.Log("[ScorePopupManager] Hide called");
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}

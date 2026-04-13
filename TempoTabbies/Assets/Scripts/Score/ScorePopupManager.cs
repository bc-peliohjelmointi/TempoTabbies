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
            // fetch the full set of scores for this map/difficulty 
            List<ScoreEntry> allEntries = ScoreDatabase.GetScores(mapName, difficulty) ?? new List<ScoreEntry>();

            int profileIndexInAll = -1;
            ScoreEntry profileEntry = default;
            string currentProfileName = null;
            if (_GameManager.instance != null && _GameManager.instance.p1 != null)
                currentProfileName = _GameManager.instance.p1.name;
            // check p2 if p1 empty
            if (string.IsNullOrEmpty(currentProfileName) && _GameManager.instance != null && _GameManager.instance.p2 != null)
                currentProfileName = _GameManager.instance.p2.name;

            if (!string.IsNullOrEmpty(currentProfileName))
            {
                for (int i = 0; i < allEntries.Count; i++)
                {
                    if (allEntries[i].profileName == currentProfileName)
                    {
                        profileIndexInAll = i;
                        profileEntry = allEntries[i];
                        break;
                    }
                }
            }

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

            if (profileIndexInAll >= 0)
            {
                if (currentProfileRow.rowRoot != null) currentProfileRow.rowRoot.SetActive(true);
                if (currentProfileRow.indexText != null) currentProfileRow.indexText.text = (profileIndexInAll + 1).ToString();
                if (currentProfileRow.playerText != null) currentProfileRow.playerText.text = profileEntry.profileName;
                if (currentProfileRow.gradeText != null) currentProfileRow.gradeText.text = profileEntry.grade;
                if (currentProfileRow.scoreText != null) currentProfileRow.scoreText.text = profileEntry.score.ToString("N0");
            }
            else
            {
                if (currentProfileRow.rowRoot != null) currentProfileRow.rowRoot.SetActive(false);
            }

            // populate top-entry summary fields. Prefer current profile's best entry if present,
            if (entries.Count > 0)
            {
                // default to top overall
                var summaryEntry = entries[0];

                // Prefer the profileEntry from the full DB if found; otherwise use top overall
                if (profileIndexInAll >= 0)
                {
                    summaryEntry = profileEntry;
                }

                if (gradetext != null) gradetext.text = summaryEntry.grade;
                if (cleartypetext != null) cleartypetext.text = $"Clear: {summaryEntry.clearType}";
                if (playcounttext != null) playcounttext.text = $"Play count: {summaryEntry.playCount}";
                if (scoretext != null) scoretext.text = $"Score: {summaryEntry.score:N0}";
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
                // default to top overall
                var summaryEntry = entries[0];

                // Try to prefer current profile's best entry
                string currentProfileName = null;
                if (_GameManager.instance != null)
                {
                    if (_GameManager.instance.p1 != null) currentProfileName = _GameManager.instance.p1.name;
                    if (string.IsNullOrEmpty(currentProfileName) && _GameManager.instance.p2 != null) currentProfileName = _GameManager.instance.p2.name;
                }

                if (!string.IsNullOrEmpty(currentProfileName))
                {
                    foreach (var e in entries)
                    {
                        if (e.profileName == currentProfileName)
                        {
                            summaryEntry = e;
                            break;
                        }
                    }
                }

                if (gradetext != null) gradetext.text = summaryEntry.grade;
                if (cleartypetext != null) cleartypetext.text = $"Clear: {summaryEntry.clearType}";
                if (playcounttext != null) playcounttext.text = $"Play count: {summaryEntry.playCount}";
                if (scoretext != null) scoretext.text = $"Score: {summaryEntry.score:N0}";
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

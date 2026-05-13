using UnityEngine;
using UnityEngine.EventSystems;

public class ChartButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SMFile song;
    private SMChart chart;
    private string mapName;
    private string difficulty;

    public void Initialize(SMFile songFile, SMChart chartData)
    {
        song = songFile;
        chart = chartData;
        if (song != null) mapName = song.Title;
        if (chart != null) difficulty = chart.Difficulty;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Re-run initialization on every hover to ensure data is set
        Initialize(song, chart);
        // Ensure we have map and difficulty, try fallbacks
        string m = mapName;
        string d = difficulty;

        if ((string.IsNullOrEmpty(m) || string.IsNullOrEmpty(d)) && song != null && chart != null)
        {
            m = song.Title;
            d = chart.Difficulty;
        }

        Debug.Log($"[ChartButtonHover] PointerEnter on map='{m}' diff='{d}'");

        if (string.IsNullOrEmpty(m) || string.IsNullOrEmpty(d)) return;

        // Try singleton first, otherwise find an instance in scene
        var mgr = ScorePopupManager.Instance;
        if (mgr == null)
        {
            mgr = FindFirstObjectByType<ScorePopupManager>();
            if (mgr == null)
            {
                Debug.LogWarning("[ChartButtonHover] No ScorePopupManager found in scene.");
                return;
            }
        }

        // Pass song.Artist when available so ScorePopupManager can show artist text
        mgr.ShowScores(m, d, song != null ? song.Artist : null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var mgr = ScorePopupManager.Instance ?? FindFirstObjectByType<ScorePopupManager>();
        if (mgr != null)
        {
            mgr.Hide();
        }
    }

    // Called when selection changes via controller/keyboard navigation
    public void HoverEnter()
    {
        // Ensure initialization runs
        Initialize(song, chart);

        string m = mapName;
        string d = difficulty;

        if ((string.IsNullOrEmpty(m) || string.IsNullOrEmpty(d)) && song != null && chart != null)
        {
            m = song.Title;
            d = chart.Difficulty;
        }

        Debug.Log($"[ChartButtonHover] HoverEnter on map='{m}' diff='{d}'");

        if (string.IsNullOrEmpty(m) || string.IsNullOrEmpty(d)) return;

        var mgr = ScorePopupManager.Instance ?? FindFirstObjectByType<ScorePopupManager>();
        if (mgr == null)
        {
            Debug.LogWarning("[ChartButtonHover] No ScorePopupManager found in scene.");
            return;
        }

        // Pass song.Artist when available so ScorePopupManager can show artist text
        mgr.ShowScores(m, d, song != null ? song.Artist : null);
    }

    public void HoverExit()
    {
        var mgr = ScorePopupManager.Instance ?? FindFirstObjectByType<ScorePopupManager>();
        if (mgr != null)
        {
            mgr.Hide();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SongButton : MonoBehaviour
{
    public TMP_Text TitleText;
    public TMP_Text ArtistText;
    public Transform ChartListParent;
    public GameObject ChartButtonPrefab;

    private SMFile currentSong;
    private ChartSelectManager manager;

    public void Setup(SMFile sm, ChartSelectManager mgr)
    {
        currentSong = sm;
        manager = mgr;

        TitleText.text = sm.Title;
        ArtistText.text = sm.Artist;

        foreach (var chart in sm.Charts)
        {
            GameObject btnObj = Instantiate(ChartButtonPrefab, ChartListParent);
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TMP_Text>();
            txt.text = $"{chart.Difficulty} ({chart.Meter})";

            btn.onClick.AddListener(() => manager.OnChartSelected(sm, chart));
        }
    }
}

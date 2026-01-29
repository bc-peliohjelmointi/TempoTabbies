using UnityEngine;
using UnityEngine.UI;

public class Scorebar : MonoBehaviour
{
    public Image mask;
    public int totalscore;
    public ScoreManager Player2Score;
    public ScoreManager Player1Score;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        getCurrentFill();
    }

   
    void CaluclatePlayerScores()
    {
        totalscore = Player1Score.currentScore + Player2Score.currentScore;
    }

    void getCurrentFill()
    {
        float fillamount = Player1Score / totalscore;
        mask.fillAmount = Mathf.Clamp01(fillamount);
    }
}

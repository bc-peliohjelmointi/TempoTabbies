using UnityEngine;
using UnityEngine.UI;

public class Scorebar : MonoBehaviour
{
    public Image mask;
    public int totalscore;
    public ScoreManager Player2Score;
    public ScoreManager Player1Score;
    public float fillamount = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Player1Score == null || Player2Score == null || mask == null)
        {
            Debug.LogWarning("Scorebar: Player scores or mask not assigned.");
            return;
        }

        CalculatePlayerScores();
        getCurrentFill();
    }

    public void CalculatePlayerScores()
    {
        totalscore = Player1Score.currentScore + Player2Score.currentScore;
    }

    void getCurrentFill()
    {
        if (totalscore <= 0)
        {
            fillamount = 0f;
        }
        else
        {
            // Cast to float to avoid integer division
            fillamount = (float)Player1Score.currentScore / (float)totalscore;
        }

        Debug.Log("Fill Amount: " + fillamount);
        mask.fillAmount = Mathf.Clamp01(fillamount);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scorebar : MonoBehaviour
{
    public Image player1mask;
    public Image player2mask;
    public int totalscore;
    public ScoreManager Player2Score;
    public ScoreManager Player1Score;

    public int player1LeadAmount;
    public int player2LeadAmount;

    public int leadstate; // 1 if player 1 is leading, 2 if player 2 is leading, 0 if draw

    public TextMeshProUGUI player1Lead;
    public TextMeshProUGUI player2Lead;

    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    public float fillamount = 0.5f;

    [Header("Visuals")]
    [Tooltip("Base multiplier applied when a player has a small lead. Final multiplier is interpolated between 1 and MaxLeadMultiplier based on lead.")]
    public float leadFontSizeMultiplier = 1.15f;
    [Tooltip("Maximum multiplier the leading score can reach.")]
    public float maxLeadMultiplier = 1.5f;
    [Tooltip("Lead (in points) at which the leading score reaches the maximum multiplier.")]
    public float leadThresholdForMaxMultiplier = 500f;

    // store original font sizes so we can restore them
    private float player1BaseFontSize = 0f;
    private float player2BaseFontSize = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player1ScoreText != null)
            player1BaseFontSize = player1ScoreText.fontSize;
        if (player2ScoreText != null)
            player2BaseFontSize = player2ScoreText.fontSize;

        // Hide lead margin texts initially
        if (player1Lead != null) player1Lead.gameObject.SetActive(false);
        if (player2Lead != null) player2Lead.gameObject.SetActive(false);

        // Ensure both masks are disabled initially unless someone is leading
        if (player1mask != null) player1mask.gameObject.SetActive(false);
        if (player2mask != null) player2mask.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Player1Score == null || Player2Score == null)
        {
            Debug.LogWarning("Scorebar: Player scores not assigned.");
            return;
        }

        CalculatePlayerScores();

        getCurrentFill();

        UpdateLeadVisuals();
    }

    public void CalculatePlayerScores()
    {
        if (Player1Score.currentScore > Player2Score.currentScore)
        {
            // Player 1 leading
            player1LeadAmount = Player1Score.currentScore - Player2Score.currentScore;
            player2LeadAmount = player1LeadAmount; // losing margin for player2
            leadstate = 1;
        }
        else if (Player2Score.currentScore > Player1Score.currentScore)
        {
            // Player 2 leading
            player2LeadAmount = Player2Score.currentScore - Player1Score.currentScore;
            player1LeadAmount = player2LeadAmount; // losing margin for player1
            leadstate = 2;
        }
        else
        {
            // Draw
            leadstate = 0;
            player1LeadAmount = 0;
            player2LeadAmount = 0;
        }

        totalscore = Player1Score.currentScore + Player2Score.currentScore;
    }

    void getCurrentFill()
    {
        if (totalscore <= 0)
        {           
            fillamount = 0f;
        }

        if (leadstate == 1)
        {
            fillamount = (float)player1LeadAmount / 100000f;
        }
        else if (leadstate == 2)
        {
            fillamount = (float)player2LeadAmount / 100000f;
        }
        else
        {
            // draw
            fillamount = 0f;
        }

        fillamount = Mathf.Clamp01(fillamount);
    }

    void UpdateLeadVisuals()
    {
        // Safety checks
        if (player1ScoreText == null || player2ScoreText == null)
            return;

        // Restore base sizes by default
        float p1Size = player1BaseFontSize;
        float p2Size = player2BaseFontSize;

        // Hide lead margin labels by default
        if (player1Lead != null) player1Lead.gameObject.SetActive(false);
        if (player2Lead != null) player2Lead.gameObject.SetActive(false);

        // Hide masks by default
        bool showP1Mask = false;
        bool showP2Mask = false;
        float p1Fill = 0f;
        float p2Fill = 0f;

        if (leadstate == 1)
        {
            // Player 1 leading:
            // - Scale player1 score based on lead amount (interpolated)
            float multiplier = Mathf.Lerp(1f, maxLeadMultiplier, Mathf.Clamp01((float)player1LeadAmount / leadThresholdForMaxMultiplier));
            // apply at least the small base multiplier if desired
            multiplier = Mathf.Max(multiplier, leadFontSizeMultiplier);

            p1Size = player1BaseFontSize * multiplier;
            p2Size = player2BaseFontSize;

            // Show losing margin only on the losing player's label (player2)
            if (player2Lead != null)
            {
                player2Lead.text = "- " + player1LeadAmount.ToString();
                player2Lead.gameObject.SetActive(true);
            }

            // Show only the leading player's progress mask
            showP1Mask = player1mask != null;
            showP2Mask = false;

            p1Fill = fillamount;
            p2Fill = 0f;
        }
        else if (leadstate == 2)
        {
            // Player 2 leading
            float multiplier = Mathf.Lerp(1f, maxLeadMultiplier, Mathf.Clamp01((float)player2LeadAmount / leadThresholdForMaxMultiplier));
            multiplier = Mathf.Max(multiplier, leadFontSizeMultiplier);

            p2Size = player2BaseFontSize * multiplier;
            p1Size = player1BaseFontSize;

            // Show losing margin only on the losing player's label (player1)
            if (player1Lead != null)
            {
                player1Lead.text = "- " + player2LeadAmount.ToString();
                player1Lead.gameObject.SetActive(true);
            }

            // Show only the leading player's progress mask
            showP1Mask = false;
            showP2Mask = player2mask != null;

            p1Fill = 0f;
            p2Fill = fillamount;
        }
        else
        {
            // Draw: keep both normal and hide masks and lead labels
            p1Size = player1BaseFontSize;
            p2Size = player2BaseFontSize;
            showP1Mask = false;
            showP2Mask = false;
            p1Fill = 0f;
            p2Fill = 0f;
        }

        // Apply font sizes
        player1ScoreText.fontSize = p1Size;
        player2ScoreText.fontSize = p2Size;

        // Update the displayed score text from the ScoreManager currentScore
        // (Assigning these ensures the UI shows the current numeric scores every frame)
        player1ScoreText.text = Player1Score.currentScore.ToString();
        player2ScoreText.text = Player2Score.currentScore.ToString();

        // Apply mask visibility and fills
        if (player1mask != null)
        {
            if (player1mask.gameObject.activeSelf != showP1Mask)
                player1mask.gameObject.SetActive(showP1Mask);

            player1mask.fillAmount = Mathf.Clamp01(p1Fill);
        }

        if (player2mask != null)
        {
            if (player2mask.gameObject.activeSelf != showP2Mask)
                player2mask.gameObject.SetActive(showP2Mask);

            player2mask.fillAmount = Mathf.Clamp01(p2Fill);
        }
    }
}

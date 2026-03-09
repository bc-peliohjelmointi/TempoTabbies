using UnityEngine;
using static CardDataScript;

public class EightLives : MonoBehaviour
{
    CardData data;
    CardEffectGiver giver;

    public int playerIndex;
    public ScoreManager scoreManager1;
    public ScoreManager scoreManager2;
    private void Start()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.CatReaper);
        }
        
        data.activeP1 = false;
        data.activeP2 = false;
    }
    private void Update()
    {
        if (data.activeP1 && playerIndex == 0)
        {
            scoreManager1.eightLives = true;
            playerIndex = 999;
        }
        if (data.activeP2 && playerIndex == 1)
        {
            scoreManager2.eightLives = true;
            playerIndex = 999;
        }
    }
}

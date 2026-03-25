using UnityEngine;
using static CardDataScript;

public class EightLives : MonoBehaviour
{
    CardDataScript data;
    CardEffectGiver giver;

    public int playerIndex;
    public ScoreManager scoreManager1;
    public ScoreManager scoreManager2;
    _GameManager gm;
    private void Start()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.EightLives);
        }
        if (gm == null)
        {
            gm = FindFirstObjectByType<_GameManager>();
        }

        data.activeP1 = false;
        data.activeP2 = false;
    }
    private void Update()
    {
        if (!gm.multiplayer)
        {
            if (data.activeP1 || data.activeP2)
            {
                scoreManager1.eightLives = true;
            }
            else
            {
                scoreManager1.eightLives = false;
            }
        }
        else
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
}

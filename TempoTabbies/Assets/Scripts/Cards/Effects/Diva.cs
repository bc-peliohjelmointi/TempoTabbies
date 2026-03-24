using UnityEngine;
using static CardDataScript;

public class Diva : MonoBehaviour
{
    CardData data;
    CardEffectGiver giver;

    public int playerIndex;
    public ScoreManager scoreManager1;
    public ScoreManager scoreManager2;
    public _GameManager gm;

    private void Start()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.Diva);
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
                scoreManager1.diva = true;
            }
            else
            {
                scoreManager1.diva = false;
            }
        }
        else
        {
            if ((data.activeP1 || data.activeP2) && (!scoreManager1.diva || !scoreManager2.diva))
            {
                scoreManager1.diva = true;
                scoreManager2.diva = true;
            }
            else
            {
                scoreManager1.diva = false;
                scoreManager2.diva = false;
            }
        }
    }
}

using UnityEngine;
using static CardDataScript;

public class CatReaper : MonoBehaviour
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
        if ((data.activeP1 || data.activeP2) && (!scoreManager1.reaper || !scoreManager2.reaper))
        {
            scoreManager1.reaper = true;
            scoreManager2.reaper = true;
        }
        else
        {
            scoreManager1.reaper = false;
            scoreManager2.reaper = false;
        }
    }
}

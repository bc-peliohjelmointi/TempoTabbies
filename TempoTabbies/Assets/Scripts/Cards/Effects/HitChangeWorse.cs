using UnityEngine;
using static CardDataScript;
using static TimingWindows;

public class HitChangeWorse : MonoBehaviour
{
    CardData data;
    public CardEffectGiver giver;
    public int playerIndex;


    private void Start()
    {
        giver = FindFirstObjectByType<CardEffectGiver>();
        data = giver.GetEffectDataforCard(EffectType.HitChangeWorse);
        if (data.activeP1 || data.activeP2)
        {
            setMultiplier(0.75f);
        }
    }
}

using UnityEngine;
using static CardDataScript;
using static TimingWindows;

public class HitChangeBetter : MonoBehaviour
{
    CardData data;
    public CardEffectGiver giver;
    public int playerIndex;

    private void Start()
    {
        giver = FindFirstObjectByType<CardEffectGiver>();
        data = giver.GetEffectDataforCard(EffectType.HitChangeBetter);
        if (data.activeP1 || data.activeP2)
        {
            setMultiplier(1.25f);
        }
        else
        {
            setMultiplier(1f);
        }
    }
}

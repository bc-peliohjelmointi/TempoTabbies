using UnityEngine;
using static CardDataScript;

public class TacoCat : MonoBehaviour
{
    CardDataScript data;
    CardEffectGiver giver;
    _GameManager gm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.TacoCat);
        }
        if (gm == null)
        {
            gm = FindFirstObjectByType<_GameManager>();
        }
        data.activeP1 = false;
        data.activeP2 = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (data.activeP1 || data.activeP2)
        {
            gm.taco = true;
        }
    }
}

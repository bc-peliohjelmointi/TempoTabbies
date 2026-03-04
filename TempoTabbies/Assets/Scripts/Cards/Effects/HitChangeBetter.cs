using UnityEngine;
using static CardDataScript;

public class HitChangeBetter : MonoBehaviour
{
    PlayerScript holder;
    float CoolDown;
    float KorttiAjastus;
    EffectStatus CurrentStatus;
    CardData data;
    public CardEffectGiver giver;
    public PlayerScript Combo;


    public void Activate(PlayerScript holder)
    {
        this.holder = holder;
        giver = FindFirstObjectByType<CardEffectGiver>();
        data = giver.GetEffectDataforCard(EffectType.HitChange);
        CurrentStatus = EffectStatus.waiting;
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentStatus)
        {
            case EffectStatus.waiting:
                if (holder.Combo > data.triggerThreshold)
                {
                    KorttiAjastus = 0;
                    TimingWindows.setMultiplier(data.value);
                    CurrentStatus = EffectStatus.active;

                }
                break;

            case EffectStatus.active:

                if (KorttiAjastus < data.duration)
                {
                    KorttiAjastus += Time.deltaTime;

                }
                else
                {
                    CurrentStatus = EffectStatus.cooldown;
                    TimingWindows.setMultiplier(1);
                    CoolDown = 0;
                }
                break;

            case EffectStatus.cooldown:

                if (CoolDown < data.cooldown)
                {
                    CoolDown += Time.deltaTime;
                }
                else
                {
                    CurrentStatus = EffectStatus.waiting;
                }
                break;
        }
    }
}

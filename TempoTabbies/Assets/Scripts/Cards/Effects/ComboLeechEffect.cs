using UnityEngine;
using UnityEngine.UI;
using static CardDataScript;

public class ComboLeechEffect : MonoBehaviour
{
    CardData data;
    float HyokkausAika;
    float CoolDown;
    EffectStatus CurrentStatus;
    CardEffectGiver giver;

    public _GameManager gm;
    public PlayerScript holder;
    public PlayerScript target;
    public int playerIndex;
    private void Start()
    {
        giver = FindFirstObjectByType<CardEffectGiver>();
        data = giver.GetEffectDataforCard(EffectType.DiscoCat);
        data.activeP1 = false;
        data.activeP2 = false;
    }

    private void Update()
    {
        if (gm == null)
        {
            gm = FindFirstObjectByType<_GameManager>();
            if (playerIndex == 0)
            {
                holder = gm.p1;
                target = gm.p2;
            }
            else
            {
                holder = gm.p2;
                target = gm.p1;
            }
        }
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.ComboLeech);
        }
        if ((playerIndex == 0 && data.activeP1) || (playerIndex == 1 && data.activeP2))
        {
            switch (CurrentStatus)
            {
                case EffectStatus.waiting:
                    if (target.Combo > data.triggerThreshold)// combo menee rikki
                    {
                        CurrentStatus = EffectStatus.active;
                        HyokkausAika = 0;
                    }
                    break;

                case EffectStatus.active:

                    if (HyokkausAika < data.duration)//aika on käynnissä
                    {
                        HyokkausAika += Time.deltaTime;
                        if (target.Combo > 30)
                        {
                            target.Combo -= 30;
                            holder.Combo += 30;

                        }
                        else
                        {
                            CoolDown = 0;
                            CurrentStatus = EffectStatus.cooldown;
                        }
                    }
                    else
                    {
                        CoolDown = 0;
                        CurrentStatus = EffectStatus.cooldown;
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
}

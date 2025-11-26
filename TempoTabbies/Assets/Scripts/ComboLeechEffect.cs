using UnityEngine;
using static CardDataScript;

public class ComboLeechEffect : MonoBehaviour
{
    PlayerScript holder;
    PlayerScript attaked;
    CardData data;
    float HyokkausAika;
    float CoolDown;
    EffectStatus CurrentStatus;
    CardManagerScript CardManager;

    public void Activate(PlayerScript holder, PlayerScript attaked)
    {
        this.holder = holder;
        this.attaked = attaked;
        this.CardManager = GameObject.FindFirstObjectByType<CardManagerScript>();
        this.data = CardManager.GetEffectDataforCard(EffectType.ComboLeech);
    }

    private void Update()
    {
        switch (CurrentStatus)
        {
            case EffectStatus.waiting:
                if (attaked.Combo > data.triggerThreshold)// combo menee rikki
                {
                    CurrentStatus = EffectStatus.active;
                    HyokkausAika = 0;
                }
                break;

            case EffectStatus.active:

                if (HyokkausAika < data.duration)//aika on käynnissä
                {
                    HyokkausAika += Time.deltaTime;
                    if (attaked.Combo > 1)
                    {
                        attaked.Combo -= 1;
                        holder.Combo += 1;
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
        /* if (attaked.Combo > data.triggerThreshold)// combo menee rikki
         {
             HyokkausAika = 0;
             CoolDown = 0;
             while (HyokkausAika < data.duration)//aika on käynnissä
             {
                 HyokkausAika += Time.deltaTime;
                 if (attaked.Combo > 1)
                 {
                     attaked.Combo -= 1;
                     holder.Combo += 1;
                 }
             }
         }
         while (CoolDown < data.cooldown)
         {
             CoolDown += Time.deltaTime;
         }*/
        // liikaa kombo 
        // kombo siirtyy tietyn ajan aikana
        // aloita toinen laskuri joka katsoo cooldownin
        // Jos pelaajalla on x: kombo ja cooldown on kulunut aloita uudestaan 
    }
}

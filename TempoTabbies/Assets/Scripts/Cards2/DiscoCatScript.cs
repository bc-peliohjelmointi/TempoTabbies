using UnityEngine;
using static CardDataScript;

public class DiscoCatScript : MonoBehaviour
{
    PlayerScript holder;
    CardData data;
    float kauttoAika;
    float CoolDown;
    EffectStatus CurrentStatus;
    CardManagerScript CardManager;

    void Start()
    {
        this.CardManager = GameObject.FindFirstObjectByType<CardManagerScript>();
         this.data = CardManager.GetEffectDataforCard(EffectType.DiscoCat);
        CurrentStatus = EffectStatus.cooldown;
    }
    
    public void Activate(PlayerScript holder)
    {
        this.holder = holder;
        this.CardManager = GameObject.FindFirstObjectByType<CardManagerScript>();
        this.data = CardManager.GetEffectDataforCard(EffectType.DiscoCat);
        CurrentStatus = EffectStatus.waiting; 
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentStatus)
        {
            case EffectStatus.waiting:

                // Start UI timer and enable canvas, then enter active state
                if (DiscoCatPanel.instance != null)
                {
                    // Pass duration to the panel if it supports it
                    DiscoCatPanel.instance.StartTimer(data.duration);
                    if (DiscoCatPanel.instance.canvas != null)
                        DiscoCatPanel.instance.canvas.gameObject.SetActive(true);
                }

                kauttoAika = 0f;
                CurrentStatus = EffectStatus.active;
                break;


            case EffectStatus.active:

                // Show active visuals and count down the effect duration
                kauttoAika += Time.deltaTime;
                if (DiscoCatPanel.instance != null && DiscoCatPanel.instance.panelImage != null)
                    DiscoCatPanel.instance.panelImage.gameObject.SetActive(true);

                if (kauttoAika >= data.duration)
                {
                    // End effect visuals
                    if (DiscoCatPanel.instance != null)
                    {
                        if (DiscoCatPanel.instance.panelImage != null)
                            DiscoCatPanel.instance.panelImage.gameObject.SetActive(false);
                        if (DiscoCatPanel.instance.canvas != null)
                            DiscoCatPanel.instance.canvas.gameObject.SetActive(false);
                    }

                    // Enter cooldown
                    CoolDown = 0f;
                    CurrentStatus = EffectStatus.cooldown;
                }
                break;

            case EffectStatus.cooldown:
                // Accumulate cooldown; when reached, go to waiting (ready to activate)
                CoolDown += Time.deltaTime;
                if (CoolDown >= data.cooldown)
                {
                    CoolDown = 0f;
                    CurrentStatus = EffectStatus.waiting;
                }
                break;
        }

    }
}

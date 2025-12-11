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
        /* this.CardManager = GameObject.FindFirstObjectByType<CardManagerScript>();
         this.data = CardManager.GetEffectDataforCard(EffectType.DiscoCat);*/
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

                DiscoCatPanel.instance.StartTimer();
                DiscoCatPanel.instance.canvas.gameObject.SetActive(true);
                CurrentStatus = EffectStatus.active;
                
                break;

            case EffectStatus.active:

                //DiscoCatPanel.instance.panelImage.gameObject.SetActive(true);
                //DiscoCatPanel.instance.canvas.gameObject.SetActive(true);
                break;

            case EffectStatus.cooldown:
                    CoolDown += Time.deltaTime;
                if (CoolDown < data.cooldown)
                {
                    CurrentStatus = EffectStatus.waiting;
                }
                else
                {}
                
                break;
        }

    }
}

using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static CardDataScript;

public class TapTap : MonoBehaviour
{
    CardData data;
    float CoolDown;
    EffectStatus CurrentStatus;
    CardEffectGiver giver;
    public GameObject paw;
    public Animator animator;
    public bool played;

    public _GameManager gm;
    public PlayerScript target;
    public int playerIndex;

    private void Start()
    {
        giver = FindFirstObjectByType<CardEffectGiver>();
        data = giver.GetEffectDataforCard(EffectType.TapTap);
        CurrentStatus = EffectStatus.waiting;
        data.activeP1 = false;
        data.activeP2 = false;
    }

    private void Update()
    {
        if (gm == null)
        {
            gm = FindFirstObjectByType<_GameManager>();
        }
        if (animator == null)
        {
            animator = paw.GetComponent<Animator>();
        }
        if (gm != null)
        {
            if (playerIndex == 0 && gm.multiplayer)
            {
                target = gm.p2;
            }
            else if (playerIndex == 1 || !gm.multiplayer)
            {
                target = gm.p1;
            }
        }
        if ((playerIndex == 0 && data.activeP1) || (playerIndex == 1 && data.activeP2))
        {
            switch (CurrentStatus)
            {
                case EffectStatus.waiting:
                    ResetAnim();
                    Debug.LogError("Waiting");
                    if (target.Combo > data.triggerThreshold)// combo menee rikki
                    {
                        played = false;
                        CurrentStatus = EffectStatus.active;
                    }
                    break;
                case EffectStatus.active:
                    Debug.LogError("Active");
                    if (!played)
                    {
                        TapTapAnim();
                        played = true;
                    }
                    else if (animator.GetCurrentAnimatorStateInfo(0).IsName("TapTap"))
                    {
                    }
                    else
                    {
                        CoolDown = 0;
                        played = false;
                        CurrentStatus = EffectStatus.cooldown;
                    }
                        break;

                case EffectStatus.cooldown:
                    Debug.LogError("Cooldown");
                    if (CoolDown < data.cooldown)
                    {
                        CoolDown += Time.deltaTime;
                    }
                    else
                    {
                        data.cooldown = Random.Range(3, 10);
                        CurrentStatus = EffectStatus.waiting;
                    }
                    break;
            }
        }
    }
    public void TapTapAnim()
    {
        animator.SetTrigger("Active");
    }

    public void ResetAnim()
    {
        animator.ResetTrigger("Active");
    }
}

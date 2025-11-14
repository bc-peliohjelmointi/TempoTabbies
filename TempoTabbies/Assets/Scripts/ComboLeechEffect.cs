using Unity.VisualScripting;
using UnityEngine;
using static CardDataScript;

public class ComboLeechEffect : MonoBehaviour
{
    PlayerScript holder;
    PlayerScript attaked;
    CardData data;
    float HyokkausAika;
    float CoolDown;
    CardManagerScript CardManager;

    public void Activate(PlayerScript holder, PlayerScript attaked)
    {
        this.holder = holder;
        this.attaked = attaked;
        this.CardManager = GameObject.FindFirstObjectByType<CardManagerScript>();
        this.data=CardManager.GetEffectDataforCard(EffectType.ComboLeech);
    }

    private void Update()
    {
        if (attaked.Combo > data.triggerThreshold)// combo menee rikki
        {
            HyokkausAika = 0;
            CoolDown = 0;
            while (HyokkausAika < data.duration)//aika on k‰ynniss‰
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
        }
        // liikaa kombo 
        // kombo siirtyy tietyn ajan aikana
        // aloita toinen laskuri joka katsoo cooldownin
        // Jos pelaajalla on x: kombo ja cooldown on kulunut aloita uudestaan 
    }
    /*public RhythmGameManager Game;

    /// <summary>
    /// K‰ynnist‰‰ efektin (esim. kun kortti valitaan).
    /// </summary>


    [Header("Effect Settings")]
    public float duration = 5f;      // efektin kesto sekunteina
    public float interval = 1f;      // kuinka usein leechaus tapahtuu
    public int comboDrainAmount = 5; // montako comboa siirret‰‰n / sekunti

    private bool isActive;

    public void Activate()
    {
        if (!isActive && Game != null)
            StartCoroutine(LeechRoutine());
        else
            Debug.LogWarning("ComboLeechScript: GameManager ei ole asetettu!");
    }

    private IEnumerator LeechRoutine()
    {
        isActive = true;
        float elapsed = 0f;

        // PlayerStats owner = RhythmGameManager.Instance.GetPlayer("PlayerA");
        // PlayerStats target = RhythmGameManager.Instance.GetPlayer("PlayerB");
        var owner = Game.GetPlayer(ownerPlayer);
        var target = Game.GetPlayer(targetPlayer);

        if (owner == null || target == null)
        {
            Debug.LogError("ComboLeechEffect: Player reference missing!");
            yield break;
        }

        Debug.Log($"[ComboLeech] {owner.PlayerName} started draining {target.PlayerName}");

        while (elapsed < duration)
        {
            // siirr‰ comboa
            int drained = Mathf.Min(comboDrainAmount, target.Combo);
            target.AddCombo(-drained);
            player.AddCombo(drained);

            Debug.Log($"[ComboLeech] {player.PlayerName} leeched {drained} combo from {target.PlayerName}");

            RhythmGameManager.Instance.ShowComboStatus();

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        Debug.Log($"[ComboLeech] Effect ended for {player.PlayerName}");
        isActive = false;
    }*/
}

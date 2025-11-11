using System;
using UnityEngine;
using static CardDataScript;
using static PlayerStatsScript;
using static RhythmGameManager;
using static ComboLeechScript;

public class CardActivationManager : MonoBehaviour
{
    /*public CardManagerScript CardManager;
    public RhythmGameManager Game;

    void Start()
    {
        // aktivoi kummankin pelaajan valitsema kortti
        ActivateCardForPlayer(Game.PlayerA, CardManager.PlayerAChoice, PlayerId.PlayerA, PlayerId.PlayerB);
        ActivateCardForPlayer(Game.PlayerB, CardManager.PlayerBChoice, PlayerId.PlayerB, PlayerId.PlayerA);
    }

    private void ActivateCardForPlayer(PlayerStatsScript playerB1, CardDataScript playerBChoice, PlayerId playerB2, PlayerId playerA)
    {
        throw new NotImplementedException();
    }

    private void ActivateCardForPlayer(PlayerStats ownerStats, CardData card, PlayerId owner, PlayerId target)
    {
        if (card == null)
        {
            Debug.LogWarning($"[CardActivation] Player {owner} has no card!");
            return;
        }

        Debug.Log($"[CardActivation] Activating {card.CardName} (type={card.EffectType}) for {owner}");

        // ?? TÄSSÄ KOHTAA KATSOTAAN EFFECT TYPE ??
        switch (card.EffectType)
        {
            case "ComboLeech":
                // luo uusi GameObject, jossa ComboLeechEffect-komponentti
                var leech = new GameObject($"ComboLeech_{owner}").AddComponent<ComboLeechEffect>();
                leech.Game = Game;
                leech.Owner = owner;
                leech.Target = target;
                leech.DrainPercent = card.value;
                leech.Duration = card.duration;
                leech.TickInterval = 1f;
                leech.Activate();
                break;

            case "ScoreBoost":
                // toinen efekti voisi tulla tähän
                Debug.Log("[CardActivation] ScoreBoost not implemented yet!");
                break;

            default:
                Debug.LogWarning($"[CardActivation] Unknown effect type: {card.EffectType}");
                break;
        }
    }*/
}

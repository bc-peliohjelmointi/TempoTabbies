using UnityEngine;
using static CardDataScript;

public class CardActivationManager : MonoBehaviour
{
    public CardManagerScript CardManager;
    public RhythmGameManager Game;

    void Start()
    {

    }

    private void ActivateCardForPlayer(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning($"[CardActivation] Player has no card!");
            return;
        }

        Debug.Log($"[CardActivation] Activating {card.CardName} (type={card.effectType}) for");

        // ?? TÄSSÄ KOHTAA KATSOTAAN EFFECT TYPE ??
        switch (card.effectType)
        {
            case EffectType.ComboLeech:

                break;
            case EffectType.DiscoCat:
                // toinen efekti voisi tulla tähän
                Debug.Log("[CardActivation] ScoreBoost not implemented yet!");
                break;

            default:
                Debug.LogWarning($"[CardActivation] Unknown effect type: {card.effectType}");
                break;
        }
    }
}

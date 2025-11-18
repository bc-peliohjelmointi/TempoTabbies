using UnityEngine;
using UnityEngine.UIElements;

public class CardDisplayScript : MonoBehaviour
{
    public Image cardImage; // Raahaa UI Image tähän

    public void SetCard(CardDataScript.CardData card)
    {
        if (card == null)
        {
            cardImage.sprite = null;
            return;
        }

        cardImage.sprite = card.icon;
    }
}

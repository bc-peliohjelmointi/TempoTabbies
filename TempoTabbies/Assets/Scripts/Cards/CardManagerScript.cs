using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CardDataScript;

public class CardManagerScript : MonoBehaviour
{
    [Header("Card Database")]
    public CardEffectGiver giver;
    List<CardData> KorttiLista; //Lista arvotuista korteista

    [Header("Player Card Choices")]
    public PlayerScript Score;
    public _GameManager gm;
    public PlayerScript Player1Cards;
    public PlayerScript Player2Cards;

    [Header("UI asiat")]
    public TextMeshProUGUI ValittuPelaaja;//UI teksti joka kertoo pelaajan vuoron

    [Header("Cards")]
    public CardAnimations card1;
    public CardAnimations card2;
    public CardAnimations card3;


    void Start()
    {
        KorttiLista = new List<CardData>();

        gm = _GameManager.instance;
        if (gm.p1.AllCards.Count > 0)
        {
            gm.p1.AllCards.Clear();
        }
        if (gm.p2.AllCards.Count > 0)
        {
            gm.p2.AllCards.Clear();
        }
        giver = FindFirstObjectByType<CardEffectGiver>();
        RandomizeCard();
    }

    private PlayerScript FindPlayerNoCard()
    {
        if (gm.p1.AllCards.Count == 0)
        {
            ValittuPelaaja.text = "Player 1";
            Debug.Log("Pelaaja 1");
            return gm.p1;
        }
        else if (gm.p2.AllCards.Count == 0)
        {
            ValittuPelaaja.text = "Player 2";
            Debug.Log("Pelaaja 2");
            return gm.p2;
        }
        return null;
    }

    public void RandomizeCard()
    {
        KorttiLista.Clear();
        card1.ResetCard();
        card2.ResetCard();
        card3.ResetCard();
        for (int i = 0; i < 3; i++) //alkaa 0; niinpitkään kuin on alle 3; lisää aina 1
        {
            int cardNumber = Random.Range(0, giver.AllCards.Count); //riippuu korttien vaihtoehto määrästä

            CardData Arvottu = giver.AllCards[cardNumber]; //luo aina uuden CardDatan
            foreach (CardData card in KorttiLista) //käy läpi kaikki kortit jotka on jo arvottu
            {
                if (card == Arvottu) //jos sama kortti on jo arvottu, arvo uudestaan
                {
                    cardNumber = Random.Range(0, giver.AllCards.Count);
                    Arvottu = giver.AllCards[cardNumber];
                }
            }
            KorttiLista.Add(Arvottu);
        }
        GameObject.Find("Valinta1").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[0].CardName;
        GameObject.Find("Valinta1").GetComponentInChildren<UnityEngine.UI.Image>().overrideSprite = KorttiLista[0].icon;
        GameObject.Find("Valinta2").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[1].CardName;
        GameObject.Find("Valinta2").GetComponentInChildren<UnityEngine.UI.Image>().overrideSprite = KorttiLista[1].icon;
        GameObject.Find("Valinta3").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[2].CardName;
        GameObject.Find("Valinta3").GetComponentInChildren<UnityEngine.UI.Image>().overrideSprite = KorttiLista[2].icon;
        card1.DrawThis();
    }

    public void Button1Press()
    {
        Debug.Log("Button 1");
        GiveCardToPlayer(KorttiLista[0]);
    }
    public void Button2Press()
    {
        Debug.Log("Button 2");
        GiveCardToPlayer(KorttiLista[1]);
    }
    public void Button3Press()
    {
        Debug.Log("Button 3");
        GiveCardToPlayer(KorttiLista[2]);
    }

    public void GiveCardToPlayer(CardData card)
    {
        if (gm != null)
        {
            PlayerScript NoCard = FindPlayerNoCard();
            if (NoCard != null)
            {
                NoCard.AllCards.Add(card);
                Debug.Log("Kortti annettu pelaajalle " + NoCard._playerIndex);
                if (NoCard == gm.p2)
                {
                    SceneManager.LoadScene("StageSelect");
                }
                else
                {
                    RandomizeCard();
                }
            }
        }
        else
        {
            Debug.Log("Korttia ei voitu antaa");
        }
    }
}

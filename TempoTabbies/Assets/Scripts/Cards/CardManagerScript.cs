using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CardDataScript;

public class CardManagerScript : MonoBehaviour
{
    [Header("Card Database")]
    public CardEffectGiver giver;
    List<CardDataScript> KorttiLista; //Lista arvotuista korteista

    [Header("Player Card Choices")]
    public PlayerScript Score;
    public _GameManager gm;
    public PlayerScript Player1Cards;
    public PlayerScript Player2Cards;

    private int player;
    private float submit;

    [Header("UI asiat")]
    public TextMeshProUGUI ValittuPelaaja;//UI teksti joka kertoo pelaajan vuoron

    [Header("Cards")]
    public CardAnimations card1;
    public CardAnimations card2;
    public CardAnimations card3;

    void Start()
    {
        player = 0;
        KorttiLista = new List<CardDataScript>();

        ValittuPelaaja.text = "Player 1";

        gm = _GameManager.instance;
        if (gm.p1.AllCards.Count > 0 && !gm.crazy)
        {
            gm.p1.AllCards.Clear();
        }
        if (gm.multiplayer)
        {
            if (gm.p2.AllCards.Count > 0 && !gm.crazy)
            {
                gm.p2.AllCards.Clear();
            }
        }
        giver = FindFirstObjectByType<CardEffectGiver>();
        RandomizeCard();
    }

    private PlayerScript FindPlayerNoCard()
    {
        if (player == 0)
        {
            ValittuPelaaja.text = "Player 1";
            Debug.Log("Pelaaja 1");
            player++;
            return gm.p1;
        }
        else if (player == 1 && gm.multiplayer)
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
            int cardNumber = Random.Range(0, giver.AllCards.Length); //riippuu korttien vaihtoehto määrästä

            CardDataScript Arvottu = giver.AllCards[cardNumber]; //luo aina uuden CardDatan
            if (gm.epilepsy && Arvottu.epilepsy)
            {
                cardNumber = Random.Range(0, giver.AllCards.Length);
                Arvottu = giver.AllCards[cardNumber];
            }
            foreach (CardDataScript card in KorttiLista) //käy läpi kaikki kortit jotka on jo arvottu
            {
                if (card == Arvottu) //jos sama kortti on jo arvottu, arvo uudestaan
                {
                    cardNumber = Random.Range(0, giver.AllCards.Length);
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

    public void GiveCardToPlayer(CardDataScript card)
    {
        if (gm != null)
        {
            PlayerScript NoCard = FindPlayerNoCard();
            if (NoCard != null)
            {
                NoCard.AllCards.Add(card);
                Debug.Log("Kortti annettu pelaajalle " + NoCard._playerIndex);
                if (NoCard == gm.p2 || !gm.multiplayer)
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

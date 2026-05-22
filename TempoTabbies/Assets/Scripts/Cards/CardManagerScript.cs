using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CardManagerScript : MonoBehaviour
{
    [Header("Card Database")]
    public CardEffectGiver giver;
    List<CardDataScript> KorttiLista; //Lista arvotuista korteista
    List<CardDataScript> epilepsyList;
    List<CardDataScript> ownedListp1;
    List<CardDataScript> ownedListp2;

    [Header("Player Card Choices")]
    public PlayerScript Score;
    public _GameManager gm;
    public PlayerScript Player1Cards;
    public PlayerScript Player2Cards;

    private int player = 0;
    private float submit;
    public MenuAnimations anims;

    [Header("UI asiat")]
    public TextMeshProUGUI ValittuPelaaja;//UI teksti joka kertoo pelaajan vuoron

    [Header("Cards")]
    public CardAnimations card1;
    public CardAnimations card2;
    public CardAnimations card3;

    private GameObject lastSelected;

    void Start()
    {
        player = 0;
        KorttiLista = new List<CardDataScript>();
        epilepsyList = new List<CardDataScript>();
        giver = FindFirstObjectByType<CardEffectGiver>();
        if (gm == null)
        {
            gm = FindFirstObjectByType<_GameManager>();
        }

        foreach (CardDataScript card in giver.AllCards)
        {
            if (card.epilepsy)
            {
                epilepsyList.Add(card);
            }
            /*if (!Player1Cards.AllCards.Contains(card))
            {
                ownedListp1.Add(card);
            }
            if (!Player2Cards.AllCards.Contains(card))
            {
                ownedListp2.Add(card);
            }*/
        }

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
        RandomizeCard();
    }

    private PlayerScript FindPlayerNoCard()
    {
        if (player == 0)
        {
            ValittuPelaaja.text = "Player 2";
            Debug.Log("Pelaaja 2 vuoro");
            player = 1;
            return gm.p1;
        }
        else if (player == 1 && gm.multiplayer)
        {
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
            int cardNumber;
            if (!gm.epilepsy)
            {
                cardNumber = Random.Range(0, giver.AllCards.Length); //riippuu korttien vaihtoehto määrästä
            }
            else
            {
                cardNumber = Random.Range(0, epilepsyList.Count); //riippuu korttien vaihtoehto määrästä
            }
            CardDataScript Arvottu = giver.AllCards[cardNumber]; //luo aina uuden CardDatan
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
    private void Update()
    {
        foreach (PlayerScript player in gm.players)
        {
            if (player.Submit() > 0)
            {
                anims.scene = "MainMenu";
                anims.PawStB();
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (lastSelected != EventSystem.current.currentSelectedGameObject)
            {
                EventSystem.current.SetSelectedGameObject(lastSelected);
            }
        }
        if (lastSelected != EventSystem.current.currentSelectedGameObject)
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }
}

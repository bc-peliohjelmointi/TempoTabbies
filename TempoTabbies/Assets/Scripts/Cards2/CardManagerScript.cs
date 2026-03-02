using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CardDataScript;

public class CardManagerScript : MonoBehaviour
{
    [Header("Card Database")]
    public List<CardDataScript.CardData> AllCards; // Kaikki olemassa olvat kortit
    List<CardDataScript.CardData> KorttiLista; //Lista arvotuista korteista

    [Header("Player Card Choices")]
    public PlayerScript Score;
    public _GameManager gm;
    public PlayerScript Player1Cards;
    public PlayerScript Player2Cards;

    [Header("UI asiat")]
    public TextMeshProUGUI ValittuPelaaja;//UI teksti joka kertoo pelaajan vuoron


    void Start()
    {
        KorttiLista = new List<CardDataScript.CardData>();

        gm = _GameManager.instance;
        RandomizeCard();
    }

    void Update()
    {
    }

    void findPlayerActive()
    {
        gm.FindPlayers();
        Player1Cards = new PlayerScript();
        Player2Cards = new PlayerScript();
        //GameObject[] players = gm.players
        
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
        for (int i = 0; i < 3; i++) //alkaa 0; niinpitkään kuin on alle 3; lisää aina 1
        {
            int cardNumber = Random.Range(0, AllCards.Count); //riippuu korttien vaihtoehto määrästä

            CardData Arvottu = AllCards[cardNumber]; //luo aina uuden CardDatan
            KorttiLista.Add(Arvottu);
        }
        GameObject.Find("Valinta1").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[0].CardName;
        GameObject.Find("Valinta1").GetComponentInChildren<UnityEngine.UI.Image>().overrideSprite = KorttiLista[0].icon;
        GameObject.Find("Valinta2").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[1].CardName;
        GameObject.Find("Valinta2").GetComponentInChildren<UnityEngine.UI.Image>().overrideSprite = KorttiLista[1].icon;
        GameObject.Find("Valinta3").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[2].CardName;
        GameObject.Find("Valinta3").GetComponentInChildren<UnityEngine.UI.Image>().overrideSprite = KorttiLista[2].icon;
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
        //script = findPlayerActive();

        if (gm != null)
        {
            PlayerScript NoCard = FindPlayerNoCard();
            if (NoCard != null)
            {
                NoCard.AllCards.Add(card);
                Debug.Log("Kortti annettu pelaajalle " + NoCard._playerIndex);
                if (NoCard.name == gm.p2.name)
                {
                    SceneManager.LoadScene("StageSelect");
                }
            }
        }
        else
        {
            Debug.Log("Korttia ei voitu antaa");

        }
    }
    public CardData GetEffectDataforCard(EffectType effectType)//hakee effektit
    {

        for (int i = 0; i < AllCards.Count; i++)//luuppaa
        {
            bool CorrectEffect = AllCards[i].effectType == effectType;//kysyy onko effecti oikein
            if (CorrectEffect)//jos on palauttaa effectin listasta
            {
                return AllCards[i];
            }
        }

        Debug.LogError("Efectiä ei löytynyt");
        return null;
    }
}

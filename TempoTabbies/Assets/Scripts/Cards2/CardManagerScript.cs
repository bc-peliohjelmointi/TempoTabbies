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

    [Header("UI asiat")]
    public TextMeshProUGUI ValittuPelaaja;

    void Start()
    {
        KorttiLista = new List<CardDataScript.CardData>();

        RandomizeCard();
    }

    void Update()
    {
        PlayerScript active = findPlayerActive(); //etsii pelaaja pisteiden perusteella
        if (active != null)//pisteiden perusteillä löytyi ekaksi menevä
        {
            PlayerScript NoCardPlayer = findPlayerNoCard(active);//löytää pelaajan ilman korttia
            if (NoCardPlayer != null) //jos on pelaaja ilman korttia 
            {
                //vaihtaa UI tektin pelaajaan vuoroon
                ValittuPelaaja.text = NoCardPlayer._playerIndex + 1 + " valitse kortti";
                //odotta

            }
            else //voi mennä eteenpäin
            {
                // change scene !!!!
                SceneManager.LoadScene("Korttiscene");
            }
        }
        else
        {
            //ei tule mitään, koska ei ole pelaajia ja tämä menee vain ohi
        }

        if (AllCards == null || AllCards.Count == 0) return;
    }
    public void RandomizeCard()
    {
        for (int i = 0; i < 3; i++) //alkaa 0; niinpitkään kuin on alle 3; lisää aina 1
        {
            int cardNumber = Random.Range(0, AllCards.Count); //riippuu korttien vaihtoehto määrästä

            CardDataScript.CardData Arvottu = AllCards[cardNumber]; //luo aina uuden CardDatan
            KorttiLista.Add(Arvottu);
        }
        GameObject.Find("Valinta1").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[0].CardName;
        GameObject.Find("Valinta2").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[1].CardName;
        GameObject.Find("Valinta3").GetComponentInChildren<TextMeshProUGUI>().text = KorttiLista[2].CardName;
    }

    PlayerScript FindPlayerOther(PlayerScript Chosen)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 1)
        {
            return Chosen;
        }
        else
        {
            PlayerScript script1 = players[0].GetComponent<PlayerScript>();
            PlayerScript script2 = players[1].GetComponent<PlayerScript>();
            if (Chosen == script1)
            {
                return script2;
            }
            else
            {
                return script1;
            }
        }
    }

    PlayerScript findPlayerNoCard(PlayerScript Chosen)
    {
        if (Chosen.AllCards.Count == 0)
        {
            return Chosen;
        }
        else
        {
            PlayerScript Other = FindPlayerOther(Chosen);
            if (Other != null)
            {
                if (Other.AllCards.Count == 0)
                {
                    return Other;
                }
            }
        }
        return null;

    }

    PlayerScript findPlayerActive()
    {
        //jos on toinen pelaaja arvo uudestaan
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            return null;//ei ole pelaajia
        }
        if (players.Length == 1)
        {
            return players[0].GetComponent<PlayerScript>();
        }
        else
        {
            PlayerScript script1 = players[0].GetComponent<PlayerScript>();
            PlayerScript script2 = players[1].GetComponent<PlayerScript>();

            if (script1.Score >= script2.Score)
            {
                return script1;
            }
            else
            {
                return script2;
            }
        }
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
        PlayerScript script = findPlayerActive();
        if (script != null)
        {
            PlayerScript NoCard = findPlayerNoCard(script);
            if (NoCard != null)
            {
                NoCard.AllCards.Add(card);
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

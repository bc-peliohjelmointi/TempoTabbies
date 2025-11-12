using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static CardDataScript;

public class CardManagerScript : MonoBehaviour
{
    [Header("Card Database")]
    public List<CardDataScript.CardData> AllCards;

    [Header("Player Card Choices")]
    public PlayerScript Score;


    public CardDataScript.CardData PlayerAChoice;
    public CardDataScript.CardData PlayerBChoice;

    private Gamepad padA;
    private Gamepad padB;
    private int indexA = 0;
    private int indexB = 0;
    private bool lockedA = false;
    private bool lockedB = false;

    List<CardDataScript.CardData> KorttiLista; //Lista arvotuista korteista


    void Start()
    {
        KorttiLista = new List<CardDataScript.CardData>();

        if (Gamepad.all.Count > 0) padA = Gamepad.all[0];
        if (Gamepad.all.Count > 1) padB = Gamepad.all[1];

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
                //odotta

            }
            else //voi mennä eteenpäin
            {
                // change scene !!!!
            }
        }
        else
        {
           //ei tule mitään, koska ei ole pelaajia ja tämä menee vain ohi
        }


        if (AllCards == null || AllCards.Count == 0) return;

        // --- Player A selection ---
        if (padA != null && !lockedA)
        {
            if (padA.dpad.left.wasPressedThisFrame) indexA = Mathf.Max(0, indexA - 1);
            if (padA.dpad.right.wasPressedThisFrame) indexA = Mathf.Min(AllCards.Count - 1, indexA + 1);

            if (padA.buttonSouth.wasPressedThisFrame) // A button
            {
                PlayerAChoice = AllCards[indexA];
                lockedA = true;
                Debug.Log($"[CardSelect] Player A chose {PlayerAChoice}+ valinta(puuttuu)");
            }
        }

        // --- Player B selection ---
        if (padB != null && !lockedB)
        {
            if (padB.dpad.left.wasPressedThisFrame) indexB = Mathf.Max(0, indexB - 1);
            if (padB.dpad.right.wasPressedThisFrame) indexB = Mathf.Min(AllCards.Count - 1, indexB + 1);

            if (padB.buttonSouth.wasPressedThisFrame)
            {
                PlayerBChoice = AllCards[indexB];
                lockedB = true;

                Debug.Log($"[CardSelect] Player B chose {PlayerBChoice}");
            }
        }

        // Kun molemmat ovat valinneet kortin, peli voi alkaa
        if (lockedA && lockedB)
        {
            Debug.Log("? Both players have chosen cards. Starting match...");
            enabled = false;
        }
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
            return null;
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
        script.AllCards.Add(card);
    }
}

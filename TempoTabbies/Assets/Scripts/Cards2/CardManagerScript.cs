using System.Collections.Generic;
using UnityEngine;
using static CardDataScript;
using UnityEngine.InputSystem;
using TMPro;

public class CardManagerScript : MonoBehaviour
{
    [Header("Card Database")]
    public List<CardDataScript.CardData> AllCards;

    [Header("Player Card Choices")]
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

    public void Button1Press()
    {
        Debug.Log("Button 1");
    }
    public void Button2Press()
    {
        Debug.Log("Button 2");
    }
    public void Button3Press()
    {
        Debug.Log("Button 3");
    }

    public void GiveCardToPlayer(CardData card)
    {
        GameObject player= GameObject.FindGameObjectWithTag("Player");
        PlayerScript script = player.GetComponent<PlayerScript>();
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KorttiValintaScript : MonoBehaviour
{
    List<Button> buttons;
    Image image;
    List<CardDataScript.CardData> ChosenCard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttons = new List<Button>();
        image = GetComponent<Image>();
        ChosenCard = new List<CardDataScript.CardData>();
        RandomizeCard();
    }

    public void RandomizeCard()
    {
        for (int i = 0; i < 3; i++)
        {
            string nimi = "";
            int cardNumber = Random.Range(0, 3); //riippuu korttien vaihtoehto määrästä
            switch (cardNumber)
            {
                case 0:
                    nimi = "kissa";
                    break;
                case 1:
                    nimi = "koira";
                    break;
                case 2:
                    nimi = "valashai";
                    break;
            }
            CardDataScript.CardData Arvottu = new CardDataScript.CardData();
            Arvottu.CardName = nimi;
            ChosenCard.Add(Arvottu);
        }
        GameObject.Find("Valinta1").GetComponentInChildren<TextMeshProUGUI>().text = ChosenCard[0].CardName;
        GameObject.Find("Valinta2").GetComponentInChildren<TextMeshProUGUI>().text = ChosenCard[1].CardName;
        GameObject.Find("Valinta3").GetComponentInChildren<TextMeshProUGUI>().text = ChosenCard[2].CardName;
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

}

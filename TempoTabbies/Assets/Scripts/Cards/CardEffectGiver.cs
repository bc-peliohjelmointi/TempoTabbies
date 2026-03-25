using System.Collections.Generic;
using UnityEngine;
using static CardDataScript;

public class CardEffectGiver : MonoBehaviour
{
    public CardData[] AllCards; // Kaikki olemassa olvat kortit
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public CardData GetEffectDataforCard(EffectType effectType)//hakee effektit
    {

        for (int i = 0; i < AllCards.Length; i++)//luuppaa
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

using UnityEngine;
using UnityEngine.UI;

public class WhatCardIsActive : MonoBehaviour
{
    public Image korttiKuva;
    public Sprite CrazyModeEverything;
    private _GameManager gameManager;
    private PlayerScript playerScript;
    public int PlayerIndex;
    private CardDataScript PelaajanKortti;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindFirstObjectByType<_GameManager>();  //mikä mode    bool joka kattoo modet
        playerScript = FindFirstObjectByType<PlayerScript>();  // mikä kortti      EI Tarvii
        
    }
    void KukaPelaajaPartyModessa()
    {
        if (PlayerIndex == 0)
        {
            // etsi pelaaja 1 kortti
            PelaajanKortti = gameManager.p1.AllCards[0];
        }
        else
        {
            //etsi pelaaja 2
            PelaajanKortti = gameManager.p2.AllCards[0];
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(gameManager.party == true)
        {
            KukaPelaajaPartyModessa();
            korttiKuva.sprite = PelaajanKortti.icon;
        }
        else if (gameManager.crazy == true) 
        {
            korttiKuva.sprite = CrazyModeEverything;
        }
        else
        {
            gameObject.SetActive(false);
        }
        // jos perus mode on pohja kortti
        // jos crazy mode on erikois kortti
        // jos party mode on pelaajalta katsottu kortti

    }
}

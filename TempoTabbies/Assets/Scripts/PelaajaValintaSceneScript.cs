using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PelaajaValintaSceneScript : MonoBehaviour
{
    public TMP_Text text;
    private _GameManager gameManager;
    public PlayerInput playerInput;
    public int _playerIndex;
    public List<PlayerScript> players;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TMP_Text>();
        gameManager = FindFirstObjectByType<_GameManager>();
        playerInput = GetComponent<PlayerInput>();
        _playerIndex = playerInput.playerIndex;
    }

    // Update is called once per frame
    void Update()
    {
      /*  if (players.Count == 0)
        {
            text.text = "Player 1 turn";
            CompareTag("Player");
            //pelaajan 1 vuoro
            //pelaaja valitsee ja painaa nappia
        }
        if (players.Count == 1)
        {
            text.text = "Player 2 turn";
            //pelaajan 2 vuoro
        }
        else
        {
            text.text = "pelaajaa ei löydy :(";
        }*/

        //gameManager.whoGetsToPlay = _playerIndex;
        gameManager.whoGetsToPlay = _playerIndex;
    }
}

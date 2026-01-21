using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PelaajaValintaSceneScript : MonoBehaviour
{
    public TMP_Text text;
    private _GameManager gameManager;
    public PlayerInput playerInput;
    public int _playerIndex;

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
        if (_playerIndex == 0)
        {
            //pelaajan 1 vuoro
            //pelaaja valitsee ja painaa nappia
        }
        if (_playerIndex == 1)
        {
            //pelaajan 2 vuoro
        }

        gameManager.whoGetsToPlay = _playerIndex;
        text.text = "hei";
    }
}

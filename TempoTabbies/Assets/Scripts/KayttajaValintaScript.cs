using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class KayttajaValintaScript : MonoBehaviour
{
    public TMP_Dropdown myDropdown; // Make sure to assign this
    public string[] UserFolder; //Pelaajan tekem‰t JSON-tiedostot tallennetaan t‰h‰n kansioon
    private List<string> fullPaths = new List<string>();

    //pelaaja asioita
    public List<PlayerScript> players;
    public PlayerInput playerInput;
    public int _playerIndex;
    private int currentPlayer = 0;

    enum MikaKontrolleri
    {
        Keyboard,
        Xbox,
        PlayStation,
        Nothing
    }
    private Image MikaKontrolleriKuva;

    _GameManager gm;
    void Start()
    {
        MikaKontrolleriKuva = gameObject.GetComponent<Image>();
        myDropdown.ClearOptions();//tyhjent‰‰ listan

        LoadAllJSONs();

        // Use _GameManager singleton when available
        gm = _GameManager.instance ?? FindAnyObjectByType<_GameManager>();
        gm.state = _GameManager.GameState.PlayerSelect;

        EventSystem.current.SetSelectedGameObject(myDropdown.gameObject);
    }
    void LoadAllJSONs()
    {
        fullPaths.Clear();

        string[] files = Directory.GetFiles("JSON");
        List<string> options = new List<string>();

        foreach (string file in files)
        {
            options.Add(Path.GetFileNameWithoutExtension(file));
            fullPaths.Add(file); // talletetaan koko polku
            Debug.Log("K‰ytt‰j‰n tekem‰ JSON " + file);
        }

        myDropdown.ClearOptions();
        myDropdown.AddOptions(options);
    }

    public void PelaajanKontrolleri()
    {
        if (currentPlayer == 0)
        {
            // Map the string control scheme to the MikaKontrolleri enum
            MikaKontrolleri kontrolleri;
            switch (playerInput.currentControlScheme)
            {
                case "Keyboard&Mouse":
                case "Keyboard":
                    kontrolleri = MikaKontrolleri.Keyboard;
                    //MikaKontrolleriKuva.sprite= 
                    break;
                case "Gamepad":
                    if (playerInput.currentControlScheme.Contains("Xbox"))
                        kontrolleri = MikaKontrolleri.Xbox;
                    if (playerInput.currentControlScheme.Contains("PlayStation"))
                        kontrolleri = MikaKontrolleri.PlayStation;
                    else
                        kontrolleri = MikaKontrolleri.Nothing;
                    break;
            }
        }
    }
}
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
    public int _playerIndex;

    enum MikaKontrolleri
    {
        Keyboard,
        Xbox,
        PlayStation,
        Nothing,
        none
    }
    MikaKontrolleri kontrolleri;

    public Image xbox;
    public Image playstation;
    public Image Keyboard;
    public Image playerBG;
    public TextMeshProUGUI noControllerText;
    public TextMeshProUGUI playerText;

    _GameManager gm;
    void Start()
    {
        myDropdown.ClearOptions();//tyhjent‰‰ listan
        LoadAllJSONs();


        // Use _GameManager singleton when available
        gm = _GameManager.instance ?? FindAnyObjectByType<_GameManager>();
        gm.state = _GameManager.GameState.PlayerSelect;

        EventSystem.current.SetSelectedGameObject(myDropdown.gameObject);
    }

    private void Update()
    {
        PelaajanKontrolleri();
        switch (kontrolleri)
        {
            case MikaKontrolleri.Keyboard:
                xbox.gameObject.SetActive(false);
                playstation.gameObject.SetActive(false);
                Keyboard.gameObject.SetActive(true);
                noControllerText.gameObject.SetActive(false);
                break;
            case MikaKontrolleri.Xbox:
                xbox.gameObject.SetActive(true);
                playstation.gameObject.SetActive(false);
                Keyboard.gameObject.SetActive(false);
                noControllerText.gameObject.SetActive(false);
                break;
            case MikaKontrolleri.PlayStation:
                xbox.gameObject.SetActive(false);
                playstation.gameObject.SetActive(true);
                Keyboard.gameObject.SetActive(false);
                noControllerText.gameObject.SetActive(false);
                break;
            case MikaKontrolleri.Nothing:
                xbox.gameObject.SetActive(false);
                playstation.gameObject.SetActive(false);
                Keyboard.gameObject.SetActive(false);
                noControllerText.gameObject.SetActive(false);
                break;
            case MikaKontrolleri.none:
                xbox.gameObject.SetActive(false);
                playstation.gameObject.SetActive(false);
                Keyboard.gameObject.SetActive(false);
                noControllerText.gameObject.SetActive(true);
                break;
        }
    }

    void LoadAllJSONs()
    {
        fullPaths.Clear();

        List<string> options = new List<string>();

        string[] files = Directory.GetFiles("JSON/DefaultProfiles");

        foreach (string file in files)
        {
            options.Add(Path.GetFileNameWithoutExtension(file));
            fullPaths.Add(file); // talletetaan koko polku
            Debug.Log("Meidn‰n tekem‰ JSON " + file);
        }

        files = Directory.GetFiles("JSON");

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
        playerBG.color = new Color(1, 1, 1, 1);
        playerText.color = new Color(1, 1, 1, 1);
        if (_playerIndex == 0)
        {
            if (gm.players.Count > 0 && gm.p1 != null)
            {
                Debug.Log("player 1 input laite " + gm.p1.inputDevice.name);
                if (gm.p1.inputDevice is Gamepad)
                {
                    if (gm.p1.inputDevice.displayName.Contains("Xbox"))
                    {
                        kontrolleri = MikaKontrolleri.Xbox;
                    }
                    else if (gm.p1.inputDevice.displayName.Contains("DualSense"))
                    {
                        kontrolleri = MikaKontrolleri.PlayStation;
                    }
                    else
                    {
                        kontrolleri = MikaKontrolleri.Nothing;
                    }
                }
                else if (gm.p1.inputDevice == null || gm.p1.inputDevice == null)
                {
                    kontrolleri = MikaKontrolleri.Nothing;
                }
                else
                {
                    kontrolleri = MikaKontrolleri.Keyboard;
                }
            }
            else
            {
                playerBG.color = new Color(1, 1, 1, 0.2f);
                playerText.color = new Color(1, 1, 1, 0.2f);
                kontrolleri = MikaKontrolleri.none;
            }
        }
        else if (_playerIndex == 1 && gm.p2 != null)
        {
            if (gm.p2.inputDevice is Gamepad)
            {
                if (gm.p2.inputDevice.displayName.Contains("Xbox"))
                {
                    kontrolleri = MikaKontrolleri.Xbox;
                }
                else if (gm.p2.inputDevice.displayName.Contains("DualSense"))
                {
                    kontrolleri = MikaKontrolleri.PlayStation;
                }
                else
                {
                    kontrolleri = MikaKontrolleri.PlayStation;
                }
            }
            else
            {
                kontrolleri = MikaKontrolleri.Keyboard;
            }
        }
        else
        {
            playerBG.color = new Color(1, 1, 1, 0.2f);
            playerText.color = new Color(1, 1, 1, 0.2f);
            kontrolleri = MikaKontrolleri.none;
        }
    }
}
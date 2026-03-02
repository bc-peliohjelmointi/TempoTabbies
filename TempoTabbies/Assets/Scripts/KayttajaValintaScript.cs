using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
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
        Nothing
    }
    MikaKontrolleri kontrolleri;

    public Image xbox;
    public Image playstation;
    public Image Keyboard;

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
                break;
            case MikaKontrolleri.Xbox:
                xbox.gameObject.SetActive(true);
                playstation.gameObject.SetActive(false);
                Keyboard.gameObject.SetActive(false);
                break;
            case MikaKontrolleri.PlayStation:
                xbox.gameObject.SetActive(false);
                playstation.gameObject.SetActive(true);
                Keyboard.gameObject.SetActive(false);
                break;
            case MikaKontrolleri.Nothing:
                xbox.gameObject.SetActive(false);
                playstation.gameObject.SetActive(false);
                Keyboard.gameObject.SetActive(false);
                break;
        }
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
        Debug.Log(gm.p1.inputDevice.displayName);
        Debug.Log(gm.p2.inputDevice.displayName);
        if (_playerIndex == 0)
        {
            if (gm.p1.inputDevice is Gamepad)
            {
                if (gm.p1.inputDevice.displayName.Contains("Xbox"))
                {
                    kontrolleri = MikaKontrolleri.Xbox;
                }
                else if (gm.p1.inputDevice.displayName.Contains("Dualsense"))
                {
                    kontrolleri = MikaKontrolleri.PlayStation;
                }
                else
                {
                    kontrolleri = MikaKontrolleri.Nothing;
                }
            }
            else
            {
                kontrolleri = MikaKontrolleri.Keyboard;
            }
        }
        else if (_playerIndex == 1)
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
    }
}
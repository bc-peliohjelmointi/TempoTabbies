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
    public string UserFolder; //Pelaajan tekem‰t JSON-tiedostot tallennetaan t‰h‰n kansioon
    private List<string> fullPaths = new List<string>();
    private Object[] OURCreatedJSONs; // Lataa kaikki JSON-tiedostot Resources/JSON-kansiosta
    private List<TextAsset> OURJsonAssets = new List<TextAsset>();

    //pelaaja asioita
    public List<PlayerScript> players;
    public PlayerInput playerInput;
    public int _playerIndex;
    private int currentPlayer = 0;
    private int maxPlayers = 2;
    public int PlayerWhoGetsDropdown; //0 player 1, 1 player 2
    private bool[] playerLocked;
    private int[] playerSelections;

    private int currentPlayerTurn = 0;

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
        playerLocked = new bool[maxPlayers];
        playerSelections = new int[maxPlayers];
        myDropdown.ClearOptions();//tyhjent‰‰ listan

        UserFolder = Path.Combine(Application.persistentDataPath, "JSON");
        OURCreatedJSONs = Resources.LoadAll("JSON", typeof(TextAsset)); // Lataa kaikki JSON-tiedostot Resources/JSON-kansiosta

        if (!Directory.Exists(UserFolder))
        {
            Directory.CreateDirectory(UserFolder);
            Debug.Log("JSON-kansio luotu: " + UserFolder);
        }
        LoadAllJSONs(UserFolder);


        // Use _GameManager singleton when available
        gm = _GameManager.instance ?? FindAnyObjectByType<_GameManager>();
        if (gm != null)
        {
            // Make sure gm has found player objects
            gm.FindPlayers();
            // Decide maxPlayers based on gm state (multiplayer) and known players
            if (gm.multiplayer)
            {
                maxPlayers = Mathf.Max(2, gm.players != null ? gm.players.Count : 2);
            }
            else
            {
                maxPlayers = 1;
            }
        }
        else
        {
            // fallback to public players list length if set in Inspector
            maxPlayers = players != null ? players.Count : maxPlayers;
        }


        UpdateTurn();
        EventSystem.current.SetSelectedGameObject(myDropdown.gameObject);
    }
    void LoadAllJSONs(string UserFolder)
    {
        fullPaths.Clear();

        string[] files = Directory.GetFiles(UserFolder, "*.json");
        List<string> options = new List<string>();

        foreach (string file in files)
        {
            options.Add(Path.GetFileNameWithoutExtension(file));
            fullPaths.Add(file); // talletetaan koko polku
            Debug.Log("K‰ytt‰j‰n tekem‰ JSON " + file);
        }
        foreach (object asset in OURCreatedJSONs)
        {
            options.Add(Path.GetFileNameWithoutExtension(((TextAsset)asset).name));
            OURJsonAssets.Add((TextAsset)asset);
        }

        myDropdown.ClearOptions();
        myDropdown.AddOptions(options);
    }

    public void OnJsonSelected(int index)
    {
        string json = "";
        if (index >= 0 && index <= fullPaths.Count)
        {
            json = File.ReadAllText(fullPaths[index]);

        }

        else if (index >= 0 && index < fullPaths.Count + OURJsonAssets.Count)
        {
            json = OURJsonAssets[index - fullPaths.Count].text;
        }

        else
        {
            Debug.LogError("JSONina ei L÷YDYYYYY!!!:(");
            return;
        }

        PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);

        /* VAnha player valinta
         // ANNA JSON TIEDOT OIKEALLE PELAAJALLE
        if (currentPlayerTurn < players.Count)
        {
            players[currentPlayerTurn].ApplyProfile(data);
            Debug.Log($"JSON {fullPaths[index]} annettu Player {players[currentPlayerTurn]._playerIndex + 1}");

            // Siirry seuraavaan pelaajaan
            currentPlayerTurn++;
        }

        // Kaikki pelaajat valinneet
        if (currentPlayerTurn >= players.Count)
        {
            Debug.Log("Kaikki pelaajat valinneet profiilin");
            myDropdown.interactable = false;  // est‰ lis‰valinta
        }
        /*if (playerLocked[currentPlayer])
            return;

        playerSelections[currentPlayer] = index;

        string json = File.ReadAllText(fullPaths[index]);
        Debug.Log($"Player {currentPlayer + 1} valitsi:\n" + json);*/
        // Determine the correct PlayerScript target using _GameManager when available
        int selectedPlayerIndex = PlayerWhoGetsDropdown >= 0 ? PlayerWhoGetsDropdown : currentPlayerTurn;



        PlayerScript target = null;

        if (gm != null)
        {
            // Ensure gm has up-to-date player list
            gm.FindPlayers();

            // Singleplayer: always apply to p1
            if (!gm.multiplayer)
            {
                target = gm.p1;
            }
            else
            {
                // Try direct p1/p2 references if they exist
                if (selectedPlayerIndex == 0)
                {
                    target = gm.p1 ?? (gm.players != null && gm.players.Count > 0 ? gm.players[0] : null);
                }
                else if (selectedPlayerIndex == 1)
                {
                    target = gm.p2 ?? (gm.players != null && gm.players.Count > 1 ? gm.players[1] : null);
                }
                else if (gm.players != null && selectedPlayerIndex >= 0 && selectedPlayerIndex < gm.players.Count)
                {
                    target = gm.players[selectedPlayerIndex];
                }
            }
        }

        // Fallback to local players list if _GameManager not available or target still null
        if (target == null && players != null && selectedPlayerIndex >= 0 && selectedPlayerIndex < players.Count)
        {
            target = players[selectedPlayerIndex];
        }

        if (target != null)
        {
            target.ApplyProfile(data);
            Debug.Log($"JSON {(index < fullPaths.Count ? fullPaths[index] : "[builtin asset]")} applied to Player {(target._playerIndex + 1)}");
        }
        else
        {
            Debug.LogError("No target player found to apply profile for index " + selectedPlayerIndex);
            return;
        }

        // If using default turn-based selection (PlayerWhoGetsDropdown < 0) advance the turn and disable when finished.
        if (PlayerWhoGetsDropdown < 0)
        {
            // Siirry seuraavaan pelaajaan
            currentPlayerTurn++;

            int totalPlayers = (gm != null) ? (gm.multiplayer ? gm.players.Count : 1) : (players != null ? players.Count : maxPlayers);
            if (currentPlayerTurn >= totalPlayers)
            {
                Debug.Log("Kaikki pelaajat valinneet profiilin");
                myDropdown.interactable = false;  // est‰ lis‰valinta
            }
        }
    }

    void UpdateTurn()
    {
        Debug.Log($"Player {currentPlayer + 1} turn");

        //myDropdown.interactable = !playerLocked[currentPlayer];
    }
    public void LockSelection()
    {
        playerLocked[currentPlayer] = true;
        Debug.Log($"Player {currentPlayer + 1} locked selection");

        currentPlayer++;

        if (currentPlayer >= maxPlayers)
        {
            Debug.Log("Kaikki pelaajat valittu!");
            myDropdown.interactable = false;
            return;
        }

        //UpdateTurn();
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
                    if(playerInput.currentControlScheme.Contains("Xbox"))
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
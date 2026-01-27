using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class KayttajaValintaScript : MonoBehaviour
{
    public TMP_Dropdown myDropdown; // Make sure to assign this
    public string folderPath;
    private List<string> fullPaths = new List<string>();

    //pelaaja asioita
    public List<PlayerScript> players;
    public PlayerInput playerInput;
    public int _playerIndex;
    private int currentPlayer = 0;
    private int maxPlayers = 2;

    private bool[] playerLocked;
    private int[] playerSelections;

    private int currentPlayerTurn = 0;
    void Start()
    {
        playerLocked = new bool[maxPlayers];
        playerSelections = new int[maxPlayers];
        myDropdown.ClearOptions();//tyhjent‰‰ listan

        folderPath = Path.Combine(Application.persistentDataPath, "JSON");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("JSON-kansio luotu: " + folderPath);
        }
        LoadJsonFiles();
        // UpdateTurn();
        EventSystem.current.SetSelectedGameObject(myDropdown.gameObject);
    }
    void LoadJsonFiles()
    {
        fullPaths.Clear();

        string[] files = Directory.GetFiles("JSON", "*.json");
        List<string> options = new List<string>();

        foreach (string file in files)
        {
            options.Add(Path.GetFileNameWithoutExtension(file));
            fullPaths.Add(file); // talletetaan koko polku
        }
        myDropdown.ClearOptions();
        myDropdown.AddOptions(options);
    }

    public void OnJsonSelected(int index)
    {
        if (index < 0 || index >= fullPaths.Count)
            return;

        string json = File.ReadAllText(fullPaths[index]);
        PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);

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
    }

    void UpdateTurn()
    {
        Debug.Log($"Player {currentPlayer + 1} turn");

        myDropdown.interactable = !playerLocked[currentPlayer];
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
    /*
        public void Kayttaja()
        {

            Debug.Log("Player 1 turn");
            CompareTag("Player");
            //pelaajan 1 vuoro
            //pelaaja valitsee ja painaa nappia




            Debug.Log("Player 2 turn");
            //pelaajan 2 vuoro

        }*/
}
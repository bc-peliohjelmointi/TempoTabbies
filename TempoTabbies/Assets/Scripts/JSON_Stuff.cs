using System;
using System.IO;
using UnityEngine;

// Placehodler for the PlayerScript things
[Serializable]
public class Player
{
    public float scrollSpeed;
    public bool assistTick;
}

// Placeholder for the _GameManager things
[Serializable]
public class GM
{
    public float volume;
    public float scrollSpeed;
    public float audioOffset;
    public bool assistTick;
    public float assistTickVolume;
    public bool hitSound;
    public float hitSoundVolume;
    public bool showButtons;
}

public class JSON_Stuff : MonoBehaviour
{
    public static JSON_Stuff instance;

    public _GameManager gameManager;
    public Create_LoadPlayer maker;

    // String used to save and find JSON files
    string json;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        gameManager = FindAnyObjectByType<_GameManager>();
        maker = FindAnyObjectByType<Create_LoadPlayer>();
    }

    // Saves the data from _GameManager to a JSON file
    public void SaveGameManager()
    {
        // Makes the placeholder class
        GM gm = new();
        // Saves the values from _GameManager to the placeholder class
        gm.volume = gameManager.volume;
        gm.scrollSpeed = gameManager.scrollSpeed;
        gm.audioOffset = gameManager.audioOffset;
        gm.assistTick = gameManager.assistTick;
        gm.assistTickVolume = gameManager.assistTickVolume;
        gm.hitSound = gameManager.hitSound;
        gm.hitSoundVolume = gameManager.hitSoundVolume;
        gm.showButtons = gameManager.showButtons;
        // Turns the placeholder class into a JSON string
        json = JsonUtility.ToJson(gm);
        // Turns the newly made JSON string into a JSON file
        File.WriteAllText("JSON/GameManager/_GameManager", json);
    }

    // Load the data for _GameManager from a JSON file
    public void LoadGameManager()
    {
        // makes the placeholder class
        GM gm = new();
        // Finds the currently existing JSON file needed
        json = File.ReadAllText("JSON/GameManager/_GameManager");
        // Transform the JSON file into the placeholder class
        gm = JsonUtility.FromJson<GM>(json);
        // Tranfers the values from the place holder class to the _GameManager
        gameManager.volume = gm.volume;
        gameManager.scrollSpeed = gm.scrollSpeed;
        gameManager.audioOffset = gm.audioOffset;
        gameManager.assistTick = gm.assistTick;
        gameManager.assistTickVolume = gm.assistTickVolume;
        gameManager.hitSound = gm.hitSound;
        gameManager.hitSoundVolume = gm.hitSoundVolume;
        gameManager.showButtons = gm.showButtons;
    }

    public void SavePlayer(string name, float scrollSpeed, bool assistTick)
    {
        Player player = new();
        player.scrollSpeed = scrollSpeed;
        player.assistTick = assistTick;
        json = JsonUtility.ToJson(player);
        File.WriteAllText($"JSON/{name}", json);
    }

    public void LoadPlayer(string name)
    {
        Player player = new();
        json = File.ReadAllText($"JSON/{name}");
        player = JsonUtility.FromJson<Player>(json);
        if (maker == null)
        {
            maker = FindFirstObjectByType<Create_LoadPlayer>();
        }
        maker.scrollSpeed = player.scrollSpeed;
        maker.assistTick = player.assistTick;
    }

    // Saves the data from Player1 to a JSON file
    public void SavePlayer1()
    {
        Player player = new();
        player.scrollSpeed = gameManager.p1.scrollSpeed;
        player.assistTick = gameManager.p1.assistTick;
        json = JsonUtility.ToJson(player);
        File.WriteAllText("JSON/Player1", json);
    }

    // Loads the data for Player1 from a JSON file
    public void LoadPlayer1()
    {
        Player player = new();
        json = File.ReadAllText("JSON/Player1");
        player = JsonUtility.FromJson<Player>(json);
        gameManager.p1.scrollSpeed = player.scrollSpeed;
        gameManager.p1.assistTick = player.assistTick;
    }

    // Saves the data from Player2 to a JSON file
    public void SavePlayer2()
    {
        Player player = new();
        player.scrollSpeed = gameManager.p2.scrollSpeed;
        player.assistTick = gameManager.p2.assistTick;
        json = JsonUtility.ToJson(player);
        File.WriteAllText("JSON/Player2", json);
    }

    // Loads the data for Player2 from a JSON file
    public void LoadPlayer2()
    {
        Player player = new();
        json = File.ReadAllText("JSON/Player2");
        player = JsonUtility.FromJson<Player>(json);
        gameManager.p2.scrollSpeed = player.scrollSpeed;
        gameManager.p2.assistTick = player.assistTick;
    }
}

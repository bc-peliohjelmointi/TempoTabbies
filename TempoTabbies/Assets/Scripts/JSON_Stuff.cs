using System;
using System.IO;
using UnityEngine;

// Placehodler for the PlayerScript things
[Serializable]
public class Player
{
    public float scrollSpeed;
    public bool assistTick;
    public bool showButtons;
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

    public void SavePlayer(string name, float scrollSpeed, bool assistTick, bool showButtons)
    {
        Player player = new();
        player.scrollSpeed = scrollSpeed;
        player.assistTick = assistTick;
        player.showButtons = showButtons;
        json = JsonUtility.ToJson(player);
        File.WriteAllText($"JSON/{name}", json);
    }
    public void LoadPlayerToPlayer(string name, int playerIndex)
    {
        Player player = new();
        json = File.ReadAllText($"JSON/{name}");
        player = JsonUtility.FromJson<Player>(json);
        Debug.Log(player.scrollSpeed);
        if (playerIndex == 0)
        {
            gameManager.p1.scrollSpeed = player.scrollSpeed;
        }
        else if (playerIndex == 1)
        {
            gameManager.p2.scrollSpeed = player.scrollSpeed;
        }
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
        maker.showButtons = player.showButtons;
    }
}

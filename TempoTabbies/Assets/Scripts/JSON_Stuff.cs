using System;
using System.IO;
using UnityEngine;

// Placehodler for the PlayerScript things
[Serializable]
public class Player
{
    public float scrollSpeed;
    public float stickSensitivity;
    public bool assistTick;
}

// Placeholder for the _GameManager things
[Serializable]
public class GM
{
    public float volume;
    public float scrollSpeed;
    public float stickSensitivity;
    public float audioOffset;
    public bool assistTick;
    public float assistTickVolume;
    public bool hitSound;
    public float hitSoundVolume;
}

public class JSON_Stuff : MonoBehaviour
{
    public static JSON_Stuff instance;

    public _GameManager gameManager;

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
    }

    // Saves the data from _GameManager to a JSON file
    public void SaveGameManager()
    {
        // Makes the placeholder class
        GM gm = new GM();
        // Saves the values from _GameManager to the placeholder class
        gm.volume = gameManager.volume;
        gm.scrollSpeed = gameManager.scrollSpeed;
        gm.stickSensitivity = gameManager.stickSensitivity;
        gm.audioOffset = gameManager.audioOffset;
        gm.assistTick = gameManager.assistTick;
        gm.assistTickVolume = gameManager.assistTickVolume;
        gm.hitSound = gameManager.hitSound;
        gm.hitSoundVolume = gameManager.hitSoundVolume;
        // Turns the placeholder class into a JSON string
        json = JsonUtility.ToJson(gm);
        // Turns the newly made JSON string into a JSON file
        File.WriteAllText("JSON/_GameManager", json);
    }

    // Load the data for _GameManager from a JSON file
    public void LoadGameManager()
    {
        // makes the placeholder class
        GM gm = new GM();
        // Finds the currently existing JSON file needed
        json = File.ReadAllText("JSON/_GameManager");
        // Transform the JSON file into the placeholder class
        gm = JsonUtility.FromJson<GM>(json);
        // Tranfers the values from the place holder class to the _GameManager
        gameManager.volume = gm.volume;
        gameManager.scrollSpeed = gm.scrollSpeed;
        gameManager.stickSensitivity = gm.stickSensitivity;
        gameManager.audioOffset = gm.audioOffset;
        gameManager.assistTick = gm.assistTick;
        gameManager.assistTickVolume = gm.assistTickVolume;
        gameManager.hitSound = gm.hitSound;
        gameManager.hitSoundVolume = gm.hitSoundVolume;
    }

    // Saves the data from Player1 to a JSON file
    public void SavePlayer1()
    {
        Player player = new Player();
        player.scrollSpeed = gameManager.p1.scrollSpeed;
        player.stickSensitivity = gameManager.p1.stickSensitivity;
        player.assistTick = gameManager.p1.assistTick;
        json = JsonUtility.ToJson(player);
        File.WriteAllText("JSON/Player1", json);
    }

    // Loads the data for Player1 from a JSON file
    public void LoadPlayer1()
    {
        Player player = new Player();
        json = File.ReadAllText("JSON/Player1");
        player = JsonUtility.FromJson<Player>(json);
        gameManager.p1.scrollSpeed = player.scrollSpeed;
        gameManager.p1.stickSensitivity = player.stickSensitivity;
        gameManager.p1.assistTick = player.assistTick;
    }

    // Saves the data from Player2 to a JSON file
    public void SavePlayer2()
    {
        Player player = new Player();
        player.scrollSpeed = gameManager.p2.scrollSpeed;
        player.stickSensitivity = gameManager.p2.stickSensitivity;
        player.assistTick = gameManager.p2.assistTick;
        json = JsonUtility.ToJson(player);
        File.WriteAllText("JSON/Player2", json);
    }

    // Loads the data for Player2 from a JSON file
    public void LoadPlayer2()
    {
        Player player = new Player();
        json = File.ReadAllText("JSON/Player2");
        player = JsonUtility.FromJson<Player>(json);
        gameManager.p2.scrollSpeed = player.scrollSpeed;
        gameManager.p2.stickSensitivity = player.stickSensitivity;
        gameManager.p2.assistTick = player.assistTick;
    }
}

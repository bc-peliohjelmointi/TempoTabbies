using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

// Placehodler for the PlayerScript things
[Serializable]
public class Player
{
    public float scrollSpeed;
    public bool assistTick;
    public bool showButtons;
    public string button1;
    public string button2;
    public string button3;
    public string button4;
    public string key1;
    public string key2;
    public string key3;
    public string key4;
    public string swipeLeft;
    public string swipeRight;
}

// Placeholder for the _GameManager things
[Serializable]
public class GM
{
    public float volume;
    public float audioOffset;
    public bool assistTick;
    public float assistTickVolume;
    public bool hitSound;
    public bool missRumble;
    public float hitSoundVolume;
    public bool showButtons;
    public int lineOpacity;
    public bool epilepsy;
    public bool movingBG;
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
        gm.audioOffset = gameManager.audioOffset;
        gm.assistTick = gameManager.assistTick;
        gm.assistTickVolume = gameManager.assistTickVolume;
        gm.hitSound = gameManager.hitSound;
        gm.missRumble = gameManager.missRumble;
        gm.hitSoundVolume = gameManager.hitSoundVolume;
        gm.showButtons = gameManager.showButtons;
        gm.lineOpacity = gameManager.lineOpacity;
        gm.epilepsy = gameManager.epilepsy;
        gm.movingBG = gameManager.movingBG;
        // Turns the placeholder class into a JSON string
        json = JsonUtility.ToJson(gm);
        // Turns the newly made JSON string into a JSON file
        File.WriteAllText("JSON/GameManager/_GameManager.json", json);
    }

    // Load the data for _GameManager from a JSON file
    public void LoadGameManager()
    {
        if (!File.Exists(("JSON/GameManager/_GameManager.json")))
        {
            Debug.LogError("No JSON file found for GameManager, Creating a default");
            SaveGameManager();
        }
        // makes the placeholder class
        GM gm = new();
        // Finds the currently existing JSON file needed
        json = File.ReadAllText("JSON/GameManager/_GameManager.json");
        // Transform the JSON file into the placeholder class
        gm = JsonUtility.FromJson<GM>(json);
        // Tranfers the values from the place holder class to the _GameManager
        gameManager.volume = gm.volume;
        gameManager.audioOffset = gm.audioOffset;
        gameManager.assistTick = gm.assistTick;
        gameManager.assistTickVolume = gm.assistTickVolume;
        gameManager.hitSound = gm.hitSound;
        gameManager.hitSoundVolume = gm.hitSoundVolume;
        gameManager.missRumble = gm.missRumble;
        gameManager.showButtons = gm.showButtons;
        gameManager.lineOpacity = gm.lineOpacity;
        gameManager.epilepsy = gm.epilepsy;
        gameManager.movingBG = gm.movingBG;
    }

    public void SavePlayer(string name, float scrollSpeed, bool assistTick, bool showButtons, string button1, string button2, string button3, string button4, string key1, string key2, string key3, string key4, string swipeLeft, string swipeRight)
    {
        Player player = new();
        player.scrollSpeed = scrollSpeed;
        player.assistTick = assistTick;
        player.showButtons = showButtons;
        player.button1 = button1;
        player.button2 = button2;
        player.button3 = button3;
        player.button4 = button4;
        player.key1 = key1;
        player.key2 = key2;
        player.key3 = key3;
        player.key4 = key4;
        player.swipeLeft = swipeLeft;
        player.swipeRight = swipeRight;
        json = JsonUtility.ToJson(player);
        File.WriteAllText($"JSON/{name}.json".Replace("\u200B", ""), json);
    }
    public void LoadPlayerToPlayer(string name, int playerIndex)
    {
        Debug.Log("Started loading player");
        Player player = new();
        json = File.ReadAllText($"JSON/{name}.json");
        player = JsonUtility.FromJson<Player>(json);
        Debug.Log(player.scrollSpeed);
        if (playerIndex == 0)
        {
            gameManager.p1.scrollSpeed = player.scrollSpeed;
            gameManager.p1.name = name;
            gameManager.p1.SetNewButtons(player.button1, player.button2, player.button3, player.button4);
            gameManager.p1.SetNewKeys(player.key1, player.key2, player.key3, player.key4, player.swipeLeft, player.swipeRight);
            gameManager.p1.showButtons = player.showButtons;
            gameManager.p1.assistTick = player.assistTick;
        }
        else if (playerIndex == 1)
        {
            gameManager.p2.scrollSpeed = player.scrollSpeed;
            gameManager.p2.name = name;
            gameManager.p2.SetNewButtons(player.button1, player.button2, player.button3, player.button4);
            gameManager.p2.SetNewKeys(player.key1, player.key2, player.key3, player.key4, player.swipeLeft, player.swipeRight);
            gameManager.p2.showButtons = player.showButtons;
            gameManager.p2.assistTick = player.assistTick;
        }
    }

    public void LoadPlayer(string name)
    {
        Player player = new();
        json = File.ReadAllText($"JSON/{name}.json");
        player = JsonUtility.FromJson<Player>(json);
        if (maker == null)
        {
            maker = FindFirstObjectByType<Create_LoadPlayer>();
        }
        maker.scrollSpeed = player.scrollSpeed;
        maker.assistTick = player.assistTick;
        maker.showButtons = player.showButtons;
        maker.button1name = player.button1;
        maker.button2name = player.button2;
        maker.button3name = player.button3;
        maker.button4name = player.button4;
        maker.key1name = player.key1;
        maker.key2name = player.key2;
        maker.key3name = player.key3;
        maker.key4name = player.key4;
        maker.swipeLeftname = player.swipeLeft;
        maker.swipeRightname = player.swipeRight;
    }
}

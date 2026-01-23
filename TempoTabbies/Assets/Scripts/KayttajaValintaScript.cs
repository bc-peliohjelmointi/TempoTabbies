using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;


public class KayttajaValintaScript : MonoBehaviour
{
    public TMP_Dropdown myDropdown; // Make sure to assign this
    public string folderPath;
    private List<string> fullPaths = new List<string>();

    void Start()
    {
        myDropdown.ClearOptions();//tyhjent‰‰ listan

        folderPath = Path.Combine(Application.persistentDataPath,"JSON");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("JSON-kansio luotu: " + folderPath);
        }
        LoadJsonFiles();
    }
    void LoadJsonFiles()
    {
        fullPaths.Clear();

        string[] files = Directory.GetFiles("JSON");
        List<string> options = new List<string>();

        foreach (string file in files)
        {
            options.Add(Path.GetFileNameWithoutExtension(file));
            fullPaths.Add(file); // talletetaan koko polku
        }

        myDropdown.AddOptions(options);
    }

    public void OnJsonSelected(int index)
    {
        if (index < 0 || index >= fullPaths.Count)
            return;

        string json = File.ReadAllText(fullPaths[index]);
        Debug.Log("Valittu JSON:\n" + json);

        // Esim:
        // PlayerData data = JsonUtility.FromJson<PlayerData>(json);
    }
}

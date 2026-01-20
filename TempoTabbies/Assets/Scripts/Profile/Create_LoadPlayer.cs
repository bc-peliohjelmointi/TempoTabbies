using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Create_LoadPlayer : MonoBehaviour
{
    public JSON_Stuff json;
    public string player;
    
    public float scrollSpeed;
    public Slider scrollSlider;
    public TextMeshProUGUI scrollValue;

    public bool assistTick;
    public Image assistImage;

    public int playerNumber;
    public GameObject playerPrefab;
    public GameObject playerParent;

    public List<GameObject> playerList;

    private void Awake()
    {
        MakeButtons();
        AssistTick(); AssistTick();
    }

    private void Update()
    {
        scrollValue.text = scrollSlider.value.ToString();
    }

    public void MakeButtons()
    {
        foreach(GameObject player in playerList)
        {
            Destroy(player);
        }
        playerList.Clear();
        int placement = 0;
        foreach (string file in Directory.GetFiles("JSON"))
        {
            GameObject playerObject = playerPrefab;
            playerObject.name = file.Replace("JSON\\", "");
            GameObject button = Instantiate(playerObject, playerParent.transform);
            button.transform.position += new Vector3(0, placement, 0);
            placement -= 50;
            playerList.Add(button);
        }
    }

    public void ChangeName(TextMeshProUGUI text)
    {
        player = text.text;
    }

    public void SaveName()
    {
        json.SavePlayer(player, scrollSpeed, assistTick);
    }

    public void LoadName()
    {
        json.LoadPlayer(player, playerNumber);
    }

    public void ScrollSpeed()
    {
        scrollSpeed = scrollSlider.value;
    }

    public void AssistTick()
    {
        if (assistTick == false)
        {
            assistTick = true;
            assistImage.color = Color.green;
        }
        else
        {
            assistTick = false;
            assistImage.color = Color.red;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Create_LoadPlayer : MonoBehaviour
{
    public JSON_Stuff json;
    public string player;

    public GameObject section1;
    public GameObject s1Start;

    public GameObject section2;
    public GameObject s2Start;

    public float scrollSpeed;
    public Slider scrollSlider;
    public TextMeshProUGUI scrollValue;

    public TMP_InputField chosenName;

    public bool assistTick;
    public Image assistImage;

    public GameObject playerPrefab;
    public GameObject playerParent;
    public List<GameObject> playerList;

    private void Awake()
    {
        MakeButtons();
        AssistTick(); AssistTick();
        SwapToSection1();
        if (json == null)
        {
            json = FindFirstObjectByType<JSON_Stuff>();
        }
    }

    private void Update()
    {
        scrollValue.text = scrollSlider.value.ToString();
    }

    public void MakeButtons()
    {
        foreach (GameObject player in playerList)
        {
            Destroy(player);
        }
        playerList.Clear();
        int placement = -80;
        foreach (string file in Directory.GetFiles("JSON"))
        {
            GameObject playerObject = playerPrefab;
            playerObject.name = file.Replace("JSON\\", "");
            GameObject button = Instantiate(playerObject, playerParent.transform);
            button.transform.position += new Vector3(0, placement, 0);
            placement -= 40;
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
        json.LoadPlayer(player);
    }

    public void DeleteName()
    {
        File.Delete($"JSON/{chosenName.text}");
    }

    public void ScrollSpeed()
    {
        if ((int)scrollSlider.value != scrollSlider.value)
        {
            scrollSlider.value = (int)scrollSlider.value;
        }
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

    public void SwapToSection1()
    {
        section1.SetActive(true);
        section2.SetActive(false);
        EventSystem.current.SetSelectedGameObject(s1Start);
    }

    public void SwapToSection2()
    {
        section1.SetActive(false);
        section2.SetActive(true);
        EventSystem.current.SetSelectedGameObject(s2Start);
    }

    public void BackToOptions()
    {
        SceneManager.LoadScene("Options");
    }
}

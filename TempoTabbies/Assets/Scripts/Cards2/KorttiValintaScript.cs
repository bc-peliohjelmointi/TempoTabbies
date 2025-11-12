using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KorttiValintaScript : MonoBehaviour
{
    List<Button> buttons;
    Image image;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttons = new List<Button>();
        image = GetComponent<Image>();
     
    }
}

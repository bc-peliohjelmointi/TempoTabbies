using UnityEngine;
using UnityEngine.UI;
using static CardDataScript;

public class VoidCat : MonoBehaviour
{
    public int playerIndex;
    CardDataScript data;
    CardEffectGiver giver;
    public Image image;
    private void Start()
    {
        giver = FindFirstObjectByType<CardEffectGiver>();
        data = giver.GetEffectDataforCard(EffectType.VoidCat);
        data.activeP1 = false;
        data.activeP2 = false;
    }
    // Update is called once per frame
    void Update()
    {
        if ((playerIndex == 0 && data.activeP1) || (playerIndex == 1 && data.activeP2) && !image.gameObject.activeSelf)
        {
            if (image.gameObject.activeSelf == false)
            {
                image.gameObject.SetActive(true);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using static CardDataScript;

public class DiscoCatScript : MonoBehaviour
{
    _GameManager gm;
    PlayerScript holder;
    public int playerIndex;
    CardData data;
    CardEffectGiver giver;
    public Image image;

    // Update is called once per frame
    void Update()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
            data = giver.GetEffectDataforCard(EffectType.DiscoCat);
        }
        if (gm == null)
        {
            gm = FindFirstObjectByType<_GameManager>();
            if (playerIndex == 0)
            {
                holder = gm.p1;
            }
            else
            {
                holder = gm.p2;
            }
        }
        if ((playerIndex == 0 && data.activeP1) || (playerIndex == 1 && data.activeP2))
        {
            if (image.gameObject.activeSelf == false)
            {
                image.gameObject.SetActive(true);
            }
        }
        else
        {
            if (image.gameObject.activeSelf == true)
            {
                image.gameObject.SetActive(false);
            }
        }
    }
}
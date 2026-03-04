using UnityEngine;
using UnityEngine.UI;
using static CardDataScript;

public class DiscoCatScript : MonoBehaviour
{
    public int playerIndex;
    CardData data;
    CardEffectGiver giver;
    public Image image;

    private void Start()
    {
        giver = FindFirstObjectByType<CardEffectGiver>();
        data = giver.GetEffectDataforCard(EffectType.DiscoCat);
        data.activeP1 = false;
        data.activeP2 = false;
    }
    // Update is called once per frame
    void Update()
    {
        if ((playerIndex == 0 && data.activeP1) || (playerIndex == 1 && data.activeP2))
        {
            Debug.LogError(data.activeP1);
            if (image.gameObject.activeSelf == false)
            {
                image.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("DiscoCat not active for player " + playerIndex + name);
            image.gameObject.SetActive(false);
        }
    }
}
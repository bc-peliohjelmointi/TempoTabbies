using UnityEngine;
using static CardDataScript;

public class Zoomies : MonoBehaviour
{
    CardData data;
    CardEffectGiver giver;

    public int playerIndex;
    public NoteSpawner spawner1;
    public NoteSpawner spawner2;
    private void Start()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.Zoomies);
        }

        data.activeP1 = false;
        data.activeP2 = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (data.activeP1 && playerIndex == 0)
        {
            if (spawner2.ScrollSpeed < 8)
            {
                spawner2.ScrollSpeed += 1;
            }
            else
            {
                spawner2.ScrollSpeed += 0.5f;
            }
            playerIndex = 999;
        }
        else if (data.activeP2 && playerIndex == 1)
        {
            if (spawner1.ScrollSpeed < 8)
            {
                spawner1.ScrollSpeed += 1;
            }
            else
            {
                spawner1.ScrollSpeed += 0.5f;
            }
            playerIndex = 999;
        }
    }
}

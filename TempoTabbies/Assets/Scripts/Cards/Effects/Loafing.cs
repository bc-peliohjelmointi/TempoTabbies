using UnityEngine;
using static CardDataScript;

public class Loafing : MonoBehaviour
{
    CardDataScript data;
    CardEffectGiver giver;

    public int playerIndex;
    public NoteSpawner spawner1;
    public NoteSpawner spawner2;
    public _GameManager gm;
    private void Start()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.Loafing);
        }
        if (gm == null)
        {
            gm = FindFirstObjectByType<_GameManager>();
        }

        data.activeP1 = false;
        data.activeP2 = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gm.multiplayer)
        {
            if ((data.activeP1 || data.activeP2) && playerIndex == 0)
            {
                spawner1.ScrollSpeed -= 1;
                if (spawner1.ScrollSpeed <= 0)
                {
                    spawner1.ScrollSpeed = 1;
                }
                playerIndex = 999;
            }
        }
        else
        {
            if (data.activeP1 && playerIndex == 0)
            {
                spawner2.ScrollSpeed -= 1;
                if (spawner2.ScrollSpeed <= 0)
                {
                    spawner2.ScrollSpeed = 1;
                }
                playerIndex = 999;
            }
            else if (data.activeP2 && playerIndex == 1)
            {
                spawner1.ScrollSpeed -= 1;
                if (spawner1.ScrollSpeed <= 0)
                {
                    spawner1.ScrollSpeed = 1;
                }
                playerIndex = 999;
            }
        }
    }
}

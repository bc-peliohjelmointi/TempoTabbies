using UnityEngine;
using static CardDataScript;

public class CatCompression : MonoBehaviour
{
    CardDataScript data;
    CardEffectGiver giver;

    public int playerIndex;
    public ScoreManager scoreManager1;
    public ScoreManager scoreManager2;

    _GameManager gm;

    private void Start()
    {
        if (giver == null)
        {
            giver = FindFirstObjectByType<CardEffectGiver>();
        }
        if (data == null)
        {
            data = giver.GetEffectDataforCard(EffectType.CatCompressions);
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
            if (data.activeP1 || data.activeP2)
            {
                scoreManager1.catCompressions = true;
            }
            else
            {
                scoreManager1.catCompressions = false;
            }
        }
        else
        {
            if ((data.activeP1 || data.activeP2) && (!scoreManager1.catCompressions || !scoreManager2.catCompressions))
            {
                scoreManager1.catCompressions = true;
                scoreManager2.catCompressions = true;
            }
            else
            {
                scoreManager1.catCompressions = false;
                scoreManager2.catCompressions = false;
            }
        }
    }
}

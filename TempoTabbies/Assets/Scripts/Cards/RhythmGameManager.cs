using UnityEngine;
using static PlayerStatsScript;

public class RhythmGameManager : MonoBehaviour
{
  /*  public static RhythmGameManager Instance;

    public PlayerStatsScript PlayerA;
    public PlayerStatsScript PlayerB;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // ? lis‰‰ t‰m‰ jos haluat s‰ilytt‰‰ managerin scenejen v‰lill‰
        }
        else
        {
            Destroy(gameObject);
        }
    }

 /*   public PlayerStats GetPlayer(string name)
    {
        if (name == "PlayerA") return PlayerA;
        if (name == "PlayerB") return PlayerB;
        return null;
    }
    public PlayerStatsScript GetPlayer(string playerId)
    {
        if (playerId == "PlayerA") return PlayerA;
        if (playerId == "PlayerB") return PlayerB;
        return null;
    }

    public void ShowComboStatus()
    {
        Debug.Log($"[Combo] A:{PlayerA.Combo} | B:{PlayerB.Combo}");
    }


    public enum PlayerId
    {
        PlayerA,
        PlayerB
    }*/
}

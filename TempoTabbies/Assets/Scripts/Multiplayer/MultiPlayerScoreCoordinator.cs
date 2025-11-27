using System.Collections.Generic;
using UnityEngine;

public class MultiplayerScoreCoordinator : MonoBehaviour
{
    public static MultiplayerScoreCoordinator Instance { get; private set; }

    private Dictionary<int, PlayerScoreManager> playerManagers = new Dictionary<int, PlayerScoreManager>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterScoreManager(int playerIndex, PlayerScoreManager manager)
    {
        playerManagers[playerIndex] = manager;
    }

    public PlayerScoreManager GetScoreManager(int playerIndex)
    {
        playerManagers.TryGetValue(playerIndex, out PlayerScoreManager manager);
        return manager;
    }
}
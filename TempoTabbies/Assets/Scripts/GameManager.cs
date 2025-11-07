using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// General game manager
/// </summary>
public class _GameManager : MonoBehaviour
{
    public static _GameManager instance;

    // A spot to remember what stage is currently selected
    public int stageID;

    public int whoGetsToPlay; // When 0, only player 1 gets to do stuff in menus, when 1, only player 2 gets to do stuff in menus

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnableControllers()
    {
        var allControllers = InputSystem.devices;
        for (int i = 0; allControllers.Count > i; i++)
        {
            InputSystem.EnableDevice(allControllers[i]);
        }
    }
}

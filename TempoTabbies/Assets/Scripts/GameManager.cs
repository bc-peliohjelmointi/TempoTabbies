using UnityEngine;

/// <summary>
/// General game manager
/// </summary>
public class _GameManager : MonoBehaviour
{
    public static _GameManager instance;

    // A spot to remember what stage is currently selected
    public int stageID;

    public bool whoGetsToPlay; // When true, only player 1 gets to do stuff in menus, when false, only player 2 gets to do stuff in menus

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
}

using Unity.VisualScripting;
using UnityEngine;

public class _GameManager : MonoBehaviour
{
    public static _GameManager instance;

    // A spot to remember what stage is currently selected
    public int stageID;

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

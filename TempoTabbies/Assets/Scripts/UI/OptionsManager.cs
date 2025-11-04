using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] float volume;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}

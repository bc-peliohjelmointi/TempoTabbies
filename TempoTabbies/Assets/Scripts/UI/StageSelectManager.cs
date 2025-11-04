using UnityEngine;
using UnityEngine.EventSystems;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button stage1;
    [SerializeField] private UnityEngine.UI.Button stage2;

    private void Awake()
    {
        EventSystem.current.SetSelectedGameObject(stage1.gameObject);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class CatManagerGame : MonoBehaviour
{
    private PlayerScript player;
    private _GameManager _gm;

    public Image catObject;
    public Sprite tabby;
    public Sprite orange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gm = FindFirstObjectByType<_GameManager>();
        player = FindFirstObjectByType<PlayerScript>();
        if (_gm.p1.cat == 1)
        {
            catObject.sprite = tabby;
        }
        if (_gm.p1.cat == 2)
        {
            catObject.sprite = orange;
        }
        if (_gm.p1.cat == 3)
        {
            catObject.gameObject.SetActive(false);
        }
    }



    // Update is called once per frame
    void Update()
    {

    }
    
}

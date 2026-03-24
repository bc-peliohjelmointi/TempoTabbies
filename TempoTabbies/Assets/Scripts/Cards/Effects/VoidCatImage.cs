using UnityEngine;

public class VoidCatImage : MonoBehaviour
{
    public bool player;
    public _GameManager gm;
    private void Start()
    {
        gm = FindFirstObjectByType<_GameManager>();
        if (player)
        {
            if (gm.p1.scrollSpeed > 0 && gm.p1.scrollSpeed < 5)
            {
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, -60, gameObject.transform.localPosition.z);
            }
            else if (gm.p1.scrollSpeed >= 5 && gm.p1.scrollSpeed < 8)
            {
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0, gameObject.transform.localPosition.z);
            }
            else
            {
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 80, gameObject.transform.localPosition.z);
            }
        }
        else
        {
            if (gm.p2.scrollSpeed > 0 && gm.p2.scrollSpeed < 5)
            {
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, -60, gameObject.transform.localPosition.z);
            }
            else if (gm.p2.scrollSpeed >= 5 && gm.p2.scrollSpeed < 8)
            {
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0, gameObject.transform.localPosition.z);
            }
            else
            {
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 80, gameObject.transform.localPosition.z);
            }
        }
    }
}

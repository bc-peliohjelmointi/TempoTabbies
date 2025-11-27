using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CatSelectButtons : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] Button thisButton;
    [SerializeField] GameObject bigImage;
    [SerializeField] TextMeshProUGUI catNameText;
    [SerializeField] TextMeshProUGUI catDescriptionText;
    [SerializeField] GameObject p1;
    [SerializeField] GameObject p2;

    _GameManager gm;

    [SerializeField] string catName;
    [SerializeField] string catDescription;

    private void Awake()
    {
        gm = FindFirstObjectByType<_GameManager>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        gameObject.transform.localScale = new Vector3(1.125f, 7.125f, 1.125f);
        bigImage.SetActive(true);
        catNameText.text = catName;
        catDescriptionText.text = catDescription;
        thisButton.image.color = new Color(1, 1, 1);
        if (gm.whoGetsToPlay == 0)
        {
            p1.SetActive(true);
        }
        else if (gm.whoGetsToPlay == 1)
        {
            p2.SetActive(true);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        gameObject.transform.localScale = new Vector3(0.75f, 4.75f, 0.75f);
        bigImage.SetActive(false);
        thisButton.image.color = new Color(0.6f, 0.6f, 0.6f);
        if (gm.whoGetsToPlay == 0)
        {
            p1.SetActive(false);
        }
        else if (gm.whoGetsToPlay == 1)
        {
            p2.SetActive(false);
        }
    }
}

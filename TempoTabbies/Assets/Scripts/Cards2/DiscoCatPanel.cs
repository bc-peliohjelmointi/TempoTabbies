using UnityEngine;
using UnityEngine.UI;

public class DiscoCatPanel : MonoBehaviour
{
    //public Transform canvas;
    //public static Canvas instance;
    public Canvas canvas;
    public Image panelImage;
    public static DiscoCatPanel instance;
    float timer = 0f;
    bool timerRunning = false;

    Color[] Väri;
    int valittuVäri;
    float Väriajastin;

    void Awake()
    {
        // GameObject panel = new GameObject("Panel");
        //DontDestroyOnLoad(canvas);
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(canvas.gameObject);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Väri = new Color[]
        {
            Color.blue, Color.darkViolet, Color.hotPink, Color.yellow,Color.limeGreen, Color.violetRed
        };
        Väriajastin = 0.5f;
    }
    private void Start()
    {
        canvas.gameObject.SetActive(false);
    }
    public void StartTimer(float duration)
    {
        Väriajastin = (duration > 0f) ? duration : 0.5f;
        timerRunning = true;
        canvas.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!timerRunning) return;

        Väriajastin -= Time.deltaTime;

        if (Väriajastin <= 0)
        {
            timerRunning = false;
            //canvas.gameObject.SetActive(false);
            valittuVäri = (valittuVäri + 1) % Väri.Length;
            panelImage.color = Väri[valittuVäri];
            StartTimer(Väriajastin > 0f ? Väriajastin: 0.5f); 
        }
    }
}

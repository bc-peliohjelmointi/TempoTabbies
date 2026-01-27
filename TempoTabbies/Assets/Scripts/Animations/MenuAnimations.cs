using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAnimations : MonoBehaviour
{
    public bool BigToSmall;
    public string scene;
    public Animator animator;

    public void Awake()
    {
        if (BigToSmall)
        {
            animator.Play("PawBigFade");
        }
    }

    public void PawStB()
    {
        animator.Play("PawStB");
    }

    public void SceneSwitch()
    {
        SceneManager.LoadScene(scene);
    }

    public void StartText()
    {
        animator.Play("StartText");
    }

    public void MenuScreen()
    {
        animator.Play("MainMenu");
    }

    public void TurnOff()
    {
        gameObject.SetActive(false);
    }

    public void TurnOn()
    {
        gameObject.SetActive(true);
    }
}

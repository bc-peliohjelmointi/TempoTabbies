using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CardAnimations : MonoBehaviour
{
    [SerializeField] GameObject cardToDraw;
    [SerializeField] AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void DrawThis()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("Active");
    }

    public void Draw()
    {
        Animator animator = cardToDraw.GetComponent<Animator>();
        animator.SetTrigger("Active");
    }

    public void CardNoise()
    {
        audioSource.PlayOneShot(audioSource.clip);
    }

    public void ResetCard()
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.ResetTrigger("Active");
        animator.Play("IdleCard");
    }
}

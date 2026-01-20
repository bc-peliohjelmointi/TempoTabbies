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

    public void Draw()
    {
        Animator animator = cardToDraw.GetComponent<Animator>();
        animator.SetTrigger("Active");
    }

    public void CardNoise()
    {
        audioSource.PlayOneShot(audioSource.clip);
    }
}

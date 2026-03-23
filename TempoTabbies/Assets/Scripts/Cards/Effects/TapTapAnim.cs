using UnityEngine;

public class TapTapAnim : MonoBehaviour
{
    public Animator animator;
    private void Start()
    {
        if (animator == null)
        {
            animator = gameObject.GetComponent<Animator>();
        }
    }

    public void TapTap()
    {
        animator.SetTrigger("Active");
    }

    public void ResetTapTap()
    {
        animator.ResetTrigger("Active");
    }
}

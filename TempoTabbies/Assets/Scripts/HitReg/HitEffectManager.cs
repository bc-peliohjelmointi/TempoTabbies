using UnityEngine;

public class SimpleHitSprite : MonoBehaviour
{
    public static SimpleHitSprite Instance { get; private set; }

    [Header("Lane Positions")]
    public Transform[] lanePositions; // Assign lane transform positions in Inspector

    [Header("Effect Prefab")]
    public GameObject hitEffectPrefab; // Should have Animator with looping animation

    [Header("Effect Settings")]
    public float effectDuration = 0.5f; // How long effects last

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate setup
        if (lanePositions == null || lanePositions.Length == 0)
        {
            Debug.LogWarning("SimpleHitSprite: No lane positions assigned!");
        }

        if (hitEffectPrefab == null)
        {
            Debug.LogError("SimpleHitSprite: No hit effect prefab assigned!");
        }
    }

    /// <summary>
    /// Play hit effect at specific lane
    /// </summary>
    public void PlayHitEffect(int lane, string animationName = "DEFAULT")
    {
        if (lane < 0 || lane >= lanePositions.Length)
        {
            Debug.LogWarning($"SimpleHitSprite: Invalid lane index {lane}");
            return;
        }

        if (hitEffectPrefab == null)
        {
            Debug.LogError("SimpleHitSprite: No prefab assigned!");
            return;
        }

        if (lanePositions[lane] == null)
        {
            Debug.LogWarning($"SimpleHitSprite: No position for lane {lane}");
            return;
        }

        // Instantiate effect at lane position
        GameObject effect = Instantiate(hitEffectPrefab, lanePositions[lane].position, Quaternion.identity);

        // Try to play animation
        Animator animator = effect.GetComponent<Animator>();
        if (animator != null)
        {
            // Try to play the named animation
            animator.Play(animationName, 0, 0f);

            // Auto-destroy after animation or duration
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float destroyTime = stateInfo.length > 0 ? stateInfo.length : effectDuration;
            Destroy(effect, destroyTime);
        }
        else
        {
            // No animator, just destroy after time
            Destroy(effect, effectDuration);
        }
    }

    /// <summary>
    /// Play hit effect at exact position
    /// </summary>
    public void PlayHitEffectAtPosition(Vector3 position, string animationName = "DEFAULT")
    {
        if (hitEffectPrefab == null)
        {
            Debug.LogError("SimpleHitSprite: No prefab assigned!");
            return;
        }

        GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

        Animator animator = effect.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animationName, 0, 0f);
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float destroyTime = stateInfo.length > 0 ? stateInfo.length : effectDuration;
            Destroy(effect, destroyTime);
        }
        else
        {
            Destroy(effect, effectDuration);
        }
    }
}
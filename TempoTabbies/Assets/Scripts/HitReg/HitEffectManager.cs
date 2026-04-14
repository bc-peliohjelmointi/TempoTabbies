using UnityEngine;

public class SimpleHitSprite : MonoBehaviour
{
    // Allow multiple instances in multiplayer so each player can have their own
    // hit effect manager and lane positions. Removed singleton enforcement.

    [Header("Multiplayer")]
    [Tooltip("Optional: set which player this SimpleHitSprite belongs to (used for auto-assignment)")]
    public int playerNumber = 0;

    // Simple registry so HitManagers can claim distinct SimpleHitSprite instances
    private static readonly System.Collections.Generic.List<SimpleHitSprite> registry = new();

    public bool IsClaimed { get; private set; } = false;

    void OnEnable()
    {
        if (!registry.Contains(this))
            registry.Add(this);
    }

    void OnDisable()
    {
        registry.Remove(this);
    }

    /// <summary>
    /// Try to claim this instance for use. Returns true if successful.
    /// </summary>
    public bool TryClaim()
    {
        if (IsClaimed) return false;
        IsClaimed = true;
        return true;
    }

    /// <summary>
    /// Find an unclaimed SimpleHitSprite matching playerNumber (if >0), otherwise first unclaimed.
    /// Marks found instance as claimed and returns it. Returns null if none available.
    /// </summary>
    public static SimpleHitSprite FindAndClaim(int wantedPlayerNumber)
    {
        // Prefer exact playerNumber matches first
        if (wantedPlayerNumber > 0)
        {
            foreach (var s in registry)
            {
                if (s != null && !s.IsClaimed && s.playerNumber == wantedPlayerNumber)
                {
                    s.IsClaimed = true;
                    return s;
                }
            }
        }

        // Fallback: any unclaimed
        foreach (var s in registry)
        {
            if (s != null && !s.IsClaimed)
            {
                s.IsClaimed = true;
                return s;
            }
        }

        return null;
    }

    [Header("Lane Positions")]
    public Transform[] lanePositions; // Assign lane transform positions in Inspector

    [Header("Effect Prefab")]
    public GameObject hitEffectPrefab; // Should have Animator with looping animation

    [Header("Effect Settings")]
    public float effectDuration = 0.5f; // How long effects last

    void Awake()
    {
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

        Debug.Log($"SimpleHitSprite.PlayHitEffect: '{name}' player={playerNumber} lane={lane} pos={lanePositions[lane].position}");

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

        Debug.Log($"SimpleHitSprite.PlayHitEffectAtPosition: '{name}' player={playerNumber} pos={position}");

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













































































































































































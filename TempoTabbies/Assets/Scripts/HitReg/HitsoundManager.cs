using UnityEngine;

public class HitSoundManager : MonoBehaviour
{
    public AudioSource hitSoundSource;
    public AudioClip hitSoundClip;

    [Header("Hit Sound Variations")]
    public AudioClip[] hitSoundVariations; // Optional: different sounds for different judgments

    public static HitSoundManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void PlayHitSound(string judgment = "")
    {
        if (hitSoundSource == null) return;

        AudioClip clipToPlay = hitSoundClip;

        // Optional: Use different sounds based on judgment
        if (hitSoundVariations != null && hitSoundVariations.Length > 0)
        {
            switch (judgment)
            {
                case "MARVELOUS":
                case "PERFECT":
                    if (hitSoundVariations.Length > 0)
                        clipToPlay = hitSoundVariations[0];
                    break;
                case "GREAT":
                    if (hitSoundVariations.Length > 1)
                        clipToPlay = hitSoundVariations[1];
                    break;
                case "GOOD":
                case "BAD":
                    if (hitSoundVariations.Length > 2)
                        clipToPlay = hitSoundVariations[2];
                    break;
            }
        }

        if (clipToPlay != null)
        {
            hitSoundSource.PlayOneShot(clipToPlay);
        }
    }
}
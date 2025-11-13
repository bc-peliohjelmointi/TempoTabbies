using UnityEngine;
using System.Collections.Generic;

public class AssistTickManager : MonoBehaviour
{
    public AudioSource tickSoundSource;
    public AudioClip tickSoundClip;

    [Header("Timing Settings")]
    public float tickLeadTime = 0.1f; // How early to schedule the tick sound

    private Queue<float> scheduledTicks = new Queue<float>();
    private float lastProcessedTime = 0f;

    public static AssistTickManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (tickSoundSource == null || tickSoundClip == null) return;
        if (!GameManager.Instance.Music.isPlaying) return;

        float currentTime = GameManager.SongTime;

        // Process scheduled ticks that are due
        while (scheduledTicks.Count > 0 && scheduledTicks.Peek() <= currentTime)
        {
            float tickTime = scheduledTicks.Dequeue();
            tickSoundSource.PlayOneShot(tickSoundClip);
            lastProcessedTime = tickTime;
        }
    }

    public void ScheduleTick(float targetTime)
    {
        // Schedule the tick to play slightly before the actual hit time
        float tickPlayTime = targetTime - tickLeadTime;

        // Only schedule if it's in the future and we haven't already processed it
        if (tickPlayTime > lastProcessedTime && tickPlayTime > GameManager.SongTime)
        {
            scheduledTicks.Enqueue(tickPlayTime);
        }
    }

    public void ClearScheduledTicks()
    {
        scheduledTicks.Clear();
    }
}
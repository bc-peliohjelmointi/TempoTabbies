using UnityEngine;
using System.Collections;

public class MusicDelayer : MonoBehaviour
{
    public AudioSource Music;
    public float delay = 0f;

    private float actualStartTime;

    void Start()
    {
        if (Music != null)
        {
            StartCoroutine(PlayDelayed());
        }
    }

    private IEnumerator PlayDelayed()
    {
        if (delay > 0f)
        {
            // Positive offset - wait before playing
            yield return new WaitForSeconds(delay);
            Music.Play();
            actualStartTime = Time.time;
        }
        else if (delay < 0f)
        {
            // Negative offset - start music immediately but skip ahead
            Music.time = Mathf.Abs(delay);
            Music.Play();
            actualStartTime = Time.time;
        }
        else
        {
            // No offset - play normally
            Music.Play();
            actualStartTime = Time.time;
        }

        Debug.Log($"[MusicDelayer] Music started at time {actualStartTime} with delay {delay}");
    }

    // Add this method to get the actual music time
    public float GetMusicTime()
    {
        if (Music != null && Music.isPlaying)
        {
            return Music.time;
        }
        return 0f;
    }
}
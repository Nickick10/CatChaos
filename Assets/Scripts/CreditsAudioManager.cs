using System.Collections;
using UnityEngine;

public class CreditsAudioManager : MonoBehaviour
{
    public static CreditsAudioManager Instance;
    [Header("Clips")]
    public AudioClip bgmClip;

    [Header("Volume")]
    public float volume = .2f;

    [Header("Audio")]
    private AudioSource audioSource;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.clip = bgmClip;
        audioSource.Play();
    }
    public IEnumerator FadeOutAndDestroy(float duration = 1f)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        audioSource.Stop();
        Destroy(gameObject);
    }
}

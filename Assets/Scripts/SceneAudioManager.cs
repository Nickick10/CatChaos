using System;
using System.Collections;
using UnityEngine;

public class SceneAudioManager : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip bgmClip;
    public AudioClip eventMusicClip;

    [Header("Timing")]
    public float bgmDelay = 2f;
    public float fadeDuration = 1f;

    [Header("Volume")]
    public float volume = 1f;

    private AudioSource audioSource;
    private Coroutine bgmLoopCoroutine;
    private Coroutine fadeCoroutine;

    public static object Instance { get; internal set; }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;

        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (bgmLoopCoroutine != null)
            StopCoroutine(bgmLoopCoroutine);

        bgmLoopCoroutine = StartCoroutine(LoopBGMWithDelay());
    }

    private IEnumerator LoopBGMWithDelay()
    {
        yield return new WaitForSeconds(bgmDelay);
        while (true)
        {
            audioSource.clip = bgmClip;
            audioSource.Play();
            yield return new WaitForSeconds(bgmClip.length + bgmDelay);
        }
    }

    public void PlayChaseMusic()
    {
        if (bgmLoopCoroutine != null)
            StopCoroutine(bgmLoopCoroutine);

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(eventMusicClip));
    }

    public void StopChaseMusicAndResumeBGM()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(bgmClip, resumeLoop: true, delayAfterClip: bgmDelay));
    }

    private IEnumerator FadeToClip(AudioClip newClip, bool resumeLoop = false, float delayAfterClip = 0f)
    {
        // Fade out
        float t = 0f;
        float startVol = audioSource.volume;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        yield return new WaitForSeconds(delayAfterClip);
        audioSource.Play();

        // Fade in
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVol, t / fadeDuration);
            yield return null;
        }

        if (resumeLoop)
        {
            yield return new WaitForSeconds(newClip.length + delayAfterClip);
            PlayBackgroundMusic();
        }
    }

    public static implicit operator SceneAudioManager(MenuAudioManager v)
    {
        throw new NotImplementedException();
    }
}

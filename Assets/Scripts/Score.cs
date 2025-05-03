using UnityEngine;
using TMPro;
using System.Collections;

public class Score : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text scoreGainText;

    private int score;
    private int scoreGain;
    public int scorePerBrokenObject = 10;
    public float scoreCommitDelay = 5f;

    private Coroutine commitCoroutine;
    private Coroutine fadeCoroutine;

    private Vector3 originalScale;

    void Start()
    {
        score = 0;
        scoreText.text = "SCORE: " + score;
        scoreGainText.alpha = 0;
        originalScale = scoreGainText.transform.localScale;
    }

    public void AddScoreGain()
    {
        scoreGain += scorePerBrokenObject;
        scoreGainText.text = $"+{scoreGain}";
        scoreGainText.alpha = 1;

        // Pop effect!
        StartCoroutine(AnimatePop(scoreGainText.transform));

        // Reset commit coroutine
        if (commitCoroutine != null)
            StopCoroutine(commitCoroutine);

        commitCoroutine = StartCoroutine(CommitScoreAfterDelay());
    }

    private IEnumerator CommitScoreAfterDelay()
    {
        yield return new WaitForSeconds(scoreCommitDelay);

        score += scoreGain;
        scoreGain = 0;
        scoreText.text = "SCORE: " + score;

        // Fade out text
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutText(scoreGainText, 0.5f));

        commitCoroutine = null;
    }

    private IEnumerator FadeOutText(TMP_Text text, float duration)
    {
        float startAlpha = text.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            text.alpha = newAlpha;
            yield return null;
        }

        text.alpha = 0f;
    }

    private IEnumerator AnimatePop(Transform target)
    {
        float popTime = 0.15f;
        float t = 0f;
        Vector3 overshoot = originalScale * 1.10f;

        // Scale up to overshoot
        while (t < popTime)
        {
            t += Time.deltaTime;
            float progress = t / popTime;
            target.localScale = Vector3.Lerp(originalScale, overshoot, progress);
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float progress = t / popTime;
            target.localScale = Vector3.Lerp(overshoot, originalScale, progress);
            yield return null;
        }

        target.localScale = originalScale;
    }
}

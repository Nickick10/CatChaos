using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    public void LoadStartMenuScene()
    {
        CreditsAudioManager.Instance.StartCoroutine(CreditsAudioManager.Instance.FadeOutAndDestroy());
        SceneManager.LoadScene("StartMenu");
    }
}
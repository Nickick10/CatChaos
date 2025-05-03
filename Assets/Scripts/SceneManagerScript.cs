using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (sceneName == "SampleScene" && MenuAudioManager.Instance != null)
        {
            MenuAudioManager.Instance.StartCoroutine(MenuAudioManager.Instance.FadeOutAndDestroy());
        }

        SceneManager.LoadScene(sceneName);
    }
}

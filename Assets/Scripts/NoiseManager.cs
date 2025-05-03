using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance;

    [Header("Noise Settings")]
    public float noiseLevel = 0f;
    public float maxNoise = 100f;
    public float decayRate = 5f;
    public float alertThreshold = 10f;

    public delegate void AlertOwner();
    public static event AlertOwner OnNoiseAlert;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (noiseLevel > 0)
        {
            noiseLevel -= decayRate * Time.deltaTime;
            noiseLevel = Mathf.Clamp(noiseLevel, 0f, maxNoise);
        }

        if (noiseLevel >= alertThreshold)
        {
            OnNoiseAlert?.Invoke();
        }
    }

    public void AddNoise(float amount)
    {
        noiseLevel += amount;
        noiseLevel = Mathf.Clamp(noiseLevel, 0f, maxNoise);
    }

    public float GetNoiseLevel() => noiseLevel;
    public float GetMaxNoise() => maxNoise;
}

using UnityEngine;
using UnityEngine.UI;

public class ThrowMeter : MonoBehaviour
{
    [Header("Slider Settings")]
    public Slider slider;
    public float speed = 1.5f;

    private bool goingUp = true;
    private bool isRunning = false;

    private float currentValue = 0f;
    private float currentScore = 1f;
    private int currentTier = 5;

    [Header("Debug")]
    [Range(0f, 1f)] public float currentError = 0f;

    void Update()
    {
        if (!isRunning) return;

        currentValue += (goingUp ? 1 : -1) * speed * Time.deltaTime;

        if (currentValue >= 1f)
        {
            currentValue = 1f;
            goingUp = false;
        }
        else if (currentValue <= 0f)
        {
            currentValue = 0f;
            goingUp = true;
        }

        slider.value = currentValue;

        currentScore = 1f - Mathf.Abs(currentValue - 0.5f) * 2f;
        currentTier = GetScoreTier(currentScore);
    }

    public void StartMeter()
    {
        isRunning = true;
        goingUp = true;
        currentValue = 0f;
        slider.value = 0f;
    }

    public float StopMeterAndGetError()
    {
        isRunning = false;

        currentScore = 1f - Mathf.Abs(currentValue - 0.5f) * 2f;
        currentTier = GetScoreTier(currentScore);

        currentError = GetErrorByTier(currentTier);
        Debug.Log($" Score: {currentScore:F2}, Tier: {currentTier}, Error: {currentError:F2}");

        return currentError;
    }

    public int GetScoreTier(float score)
    {
        if (score >= 0.9f) return 5; // Perfect
        if (score >= 0.7f) return 4; // Great
        if (score >= 0.4f) return 3; // Okay
        if (score >= 0.2f) return 2; // Bad
        return 1;                    // Miss
    }

    public float GetErrorByTier(int tier)
    {
        switch (tier)
        {
            case 5: return 0f;    // Perfect
            case 4: return 0.1f;  // Great
            case 3: return 0.25f; // Okay
            case 2: return 0.4f;  // Bad
            case 1: return 0.6f;  // Miss
            default: return 0.5f;
        }
    }

    public float GetRawScore() => currentScore;
    public int GetTier() => currentTier;
    public float GetError() => currentError;
}

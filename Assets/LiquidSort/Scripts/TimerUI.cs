using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("Timer Settings")]
    public TMP_Text timerText;      // Assign your TextMeshPro - Text (UI) here
    public float startTime = 0f;    // Starting time in seconds
    public bool countDown = false;  // Count down if true, else count up
    public bool autoStart = true;   // Start automatically on Start()

    private float currentTime;
    private bool isRunning = false;

    private void Start()
    {
        currentTime = startTime;

        if (autoStart)
            StartTimer();
    }

    private void Update()
    {
        if (!isRunning) return;

        // Update the timer
        currentTime += (countDown ? -Time.deltaTime : Time.deltaTime);

        // Clamp at zero if counting down
        if (countDown && currentTime < 0f)
        {
            currentTime = 0f;
            StopTimer();
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Public control methods
    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer(float newTime = 0f)
    {
        currentTime = newTime;
        UpdateTimerDisplay();
    }

    public float GetTime()
    {
        return currentTime;
    }
}
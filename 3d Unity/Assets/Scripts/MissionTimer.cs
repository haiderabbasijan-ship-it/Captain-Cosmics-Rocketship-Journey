using UnityEngine;
using TMPro;

public class MissionTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timeRemaining = 180f; // 3 minutes default

    [Header("Warning Settings")]
    [SerializeField] private float warningTime = 30f; // When to start showing warning (30 seconds left)
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private AudioClip warningSound;

    [Header("References")]
    [SerializeField] private PlayerDeathHandler deathHandler;
    [SerializeField] private MissionEndHandler missionEndHandler;

    private bool isRunning = true;
    private bool warningPlayed = false;
    private bool timerExpired = false;

    private void Start()
    {
        // Find references if not assigned
        if (deathHandler == null)
        {
            deathHandler = FindObjectOfType<PlayerDeathHandler>();
        }

        if (missionEndHandler == null && deathHandler == null)
        {
            missionEndHandler = FindObjectOfType<MissionEndHandler>();
        }

        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (isRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                // Check for warning threshold
                if (timeRemaining <= warningTime && !warningPlayed)
                {
                    PlayWarningSound();
                }

                UpdateTimerDisplay();
            }
            else
            {
                timeRemaining = 0;
                isRunning = false;
                Debug.Log("Timer has ended!");

                if (!timerExpired)
                {
                    timerExpired = true;
                    OnTimerExpired();
                }
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Update color based on remaining time
        if (timeRemaining <= warningTime)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    // Handle timer expiration
    private void OnTimerExpired()
    {
        // Use the PlayerDeathHandler if available
        if (deathHandler != null)
        {
            deathHandler.OnTimeExpired();
        }
        // Fall back to direct MissionEndHandler if necessary
        else if (missionEndHandler != null)
        {
            missionEndHandler.FailMission();
        }
        else
        {
            Debug.LogWarning("No handler found for timer expiration!");
        }
    }

    // Play the warning sound
    private void PlayWarningSound()
    {
        if (warningSound != null)
        {
            AudioSource.PlayClipAtPoint(warningSound, Camera.main.transform.position, 1f);
            warningPlayed = true;
        }
    }

    // Basic controls
    public void StartTimer()
    {
        isRunning = true;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResetTimer(float newTime = -1)
    {
        if (newTime > 0)
            timeRemaining = newTime;
        else
            timeRemaining = 180f; // Reset to default

        isRunning = true;
        timerExpired = false;
        warningPlayed = false;
        UpdateTimerDisplay();
    }

    // Add time to the timer (for power-ups etc.)
    public void AddTime(float additionalTime)
    {
        timeRemaining += additionalTime;
        UpdateTimerDisplay();
    }
}
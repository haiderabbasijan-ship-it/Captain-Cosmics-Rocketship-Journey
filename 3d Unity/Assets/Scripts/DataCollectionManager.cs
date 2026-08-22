using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class DataCollectionManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI counterText;

    [Header("Collection Settings")]
    [SerializeField] private string displayFormat = "{0}/{1}";
    [SerializeField] private int targetAmount = 5;
    [SerializeField] private string resultsSceneName = "MissionResults";

    [Header("Mission End Settings")]
    [SerializeField] private float endDialogueDelay = 9.0f; // Delay before showing results screen
    [SerializeField] private MissionEndHandler missionEndHandler;

    [Header("Optional References")]
    [SerializeField] private AudioClip allCollectedSound;
    [SerializeField] private float allCollectedSoundVolume = 1f;

    private int fragmentsCollected = 0;
    private bool missionComplete = false;

    // Singleton pattern for easy access
    public static DataCollectionManager Instance { get; private set; }

    private void Awake()
    {
        // Simple singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize display
        UpdateCounterDisplay();

        // Find mission end handler if not assigned
        if (missionEndHandler == null)
        {
            missionEndHandler = FindObjectOfType<MissionEndHandler>();
            if (missionEndHandler == null)
            {
                Debug.LogWarning("MissionEndHandler not found! Mission completion won't work properly.");
            }
        }
    }

    // Call this method whenever a data fragment is collected
    public void OnFragmentCollected()
    {
        fragmentsCollected++;
        UpdateCounterDisplay();

        // Check if we've reached the target and mission isn't already complete
        if (fragmentsCollected >= targetAmount && !missionComplete)
        {
            OnAllFragmentsCollected();
        }
    }

    // Handle completion of all fragment collection
    private void OnAllFragmentsCollected()
    {
        Debug.Log("All data fragments collected! Starting end dialogue delay...");
        missionComplete = true;

        // Play completion sound if available
        if (allCollectedSound != null)
        {
            AudioSource.PlayClipAtPoint(allCollectedSound, Camera.main.transform.position, allCollectedSoundVolume);
        }

        // Start the delay for end dialogue
        StartCoroutine(DelayedMissionCompletion());
    }

    // Coroutine to delay mission completion for dialogue
    private IEnumerator DelayedMissionCompletion()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(endDialogueDelay);

        // Complete the mission after the delay
        if (missionEndHandler != null)
        {
            Debug.Log("Dialogue delay complete. Ending mission...");
            missionEndHandler.CompleteMission();
        }
        else
        {
            // Fallback to direct scene loading if MissionEndHandler is missing
            Debug.LogWarning("MissionEndHandler not found! Falling back to direct scene loading.");
            SceneManager.LoadScene(resultsSceneName);
        }
    }

    // Update the UI display with current/total format
    private void UpdateCounterDisplay()
    {
        if (counterText != null)
        {
            counterText.text = string.Format(displayFormat, fragmentsCollected, targetAmount);
        }
        else
        {
            Debug.LogWarning("Counter Text is not assigned in the DataCollectionManager!");
        }
    }

    // Public accessor methods
    public int GetFragmentCount()
    {
        return fragmentsCollected;
    }

    public int GetTargetAmount()
    {
        return targetAmount;
    }

    public bool AllFragmentsCollected()
    {
        return fragmentsCollected >= targetAmount;
    }
}
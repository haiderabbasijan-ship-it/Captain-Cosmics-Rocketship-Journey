using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MissionEndHandler : MonoBehaviour
{
    // Reference to your ScoreTracker
    [SerializeField] private ScoreTracker scoreTracker;

    // Name of the results scenes
    [SerializeField] private string missionSuccessSceneName = "MissionSuccessScene";
    [SerializeField] private string missionFailedSceneName = "MissionFailedScene";

    // Optional: Reference to any UI elements that show end-of-mission information
    [SerializeField] private TextMeshProUGUI missionEndText;

    // Call this when the mission is completed (success)
    public void CompleteMission()
    {
        // Optional: Display any mission complete text before scene transition
        if (missionEndText != null)
        {
            missionEndText.text = "Mission Complete!";
            missionEndText.gameObject.SetActive(true);
        }

        // Get final score from your ScoreTracker
        if (scoreTracker != null)
        {
            MissionResultsDisplay.finalScore = scoreTracker.GetScore();
        }

        // Set mission success flag to true
        MissionResultsDisplay.isMissionSuccess = true;

        // Clean up audio before loading new scene
        CleanupAudio();

        // Load the success scene
        LoadSuccessScene();
    }

    // Call this when the mission fails (player dies)
    public void FailMission()
    {
        // Optional: Display any mission failed text before scene transition
        if (missionEndText != null)
        {
            missionEndText.text = "Mission Failed!";
            missionEndText.gameObject.SetActive(true);
        }

        // Get final score from your ScoreTracker (if you want to show the score on failure too)
        if (scoreTracker != null)
        {
            MissionResultsDisplay.finalScore = scoreTracker.GetScore();
        }

        // Set mission success flag to false
        MissionResultsDisplay.isMissionSuccess = false;

        // Clean up audio before loading new scene
        CleanupAudio();

        // Load the mission failed scene directly
        LoadFailedScene();
    }

    // Clean up audio by destroying the AudioManager
    private void CleanupAudio()
    {
        // Find and destroy the AudioManager
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            Destroy(audioManager.gameObject);
            Debug.Log("AudioManager destroyed before scene transition");
        }
    }

    // Load the success scene
    private void LoadSuccessScene()
    {
        SceneManager.LoadScene(missionSuccessSceneName);
    }

    // Load the failed scene
    private void LoadFailedScene()
    {
        SceneManager.LoadScene(missionFailedSceneName);
    }

    // Optional: Coroutine for delayed scene loading with audio cleanup
    private IEnumerator DelayedSceneLoad(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        // Clean up audio before loading scene
        CleanupAudio();
        SceneManager.LoadScene(sceneName);
    }

    // For testing - you can attach this to a key press or event
    private void Update()
    {
        // Example: Press 'End' key to complete mission (for testing)
        if (Input.GetKeyDown(KeyCode.End))
        {
            CompleteMission();
        }

        // Example: Press 'Delete' key to fail mission (for testing)
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            FailMission();
        }
    }
}
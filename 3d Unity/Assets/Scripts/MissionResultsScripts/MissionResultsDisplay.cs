using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MissionResultsDisplay : MonoBehaviour
{
    // Reference to the TextMeshPro text that will display the final score
    [SerializeField] private TextMeshProUGUI finalScoreText;
    // Reference to mission success text
    [SerializeField] private TextMeshProUGUI missionStatusText;

    // Scene names for navigation
    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "Menu";
    [SerializeField] private string missionSceneName = "MainScene";

    // Static variables to pass data between scenes
    public static int finalScore = 0;
    public static bool isMissionSuccess = true;

    void Start()
    {
        // Display the final score
        DisplayFinalScore();
    }

    // Display the score passed from the gameplay scene
    private void DisplayFinalScore()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore.ToString();
        }

        // Set text based on mission success status
        if (missionStatusText != null)
        {
            missionStatusText.text = isMissionSuccess ? "Mission Success" : "Mission Failed";
        }
    }

    // Call this method to return to the main menu
    public void ReturnToMenu()
    {
        Debug.Log("ReturnToMenu called - Loading scene: " + mainMenuSceneName);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Call this to restart the mission
    public void RestartMission()
    {
        Debug.Log("RestartMission called - Loading scene: " + missionSceneName);
        SceneManager.LoadScene(missionSceneName);
    }

    // Call this to quit the game (for standalone builds)
    public void QuitGame()
    {
        Debug.Log("QuitGame called - Application will quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
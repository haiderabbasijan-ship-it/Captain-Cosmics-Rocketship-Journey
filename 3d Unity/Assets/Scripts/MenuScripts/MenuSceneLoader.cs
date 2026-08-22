using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string cinematicSceneName = "CinematicScene";
    [SerializeField] private string controlsSceneName = "Controls";
    [SerializeField] private string creditsSceneName = "Credits";

    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button creditsButton;

    private void Start()
    {
        // If you prefer to set this up in code rather than the Inspector
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(LoadControlsScene);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(LoadCreditsScene);
        }
    }

    // This can be called directly from the Button's OnClick in the Inspector
    public void StartGame()
    {
        SceneManager.LoadScene(cinematicSceneName);
    }

    // Load the controls scene
    public void LoadControlsScene()
    {
        SceneManager.LoadScene(controlsSceneName);
    }

    // Load the credits scene
    public void LoadCreditsScene()
    {
        SceneManager.LoadScene(creditsSceneName);
    }

    // Optional: Load by build index if you prefer
    public void StartGameByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Similarly, you can add index-based loading for controls and credits
    public void LoadControlsByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadCreditsByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    private void OnDestroy()
    {
        // Clean up the listeners if we set them programmatically
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }

        if (controlsButton != null)
        {
            controlsButton.onClick.RemoveListener(LoadControlsScene);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.RemoveListener(LoadCreditsScene);
        }
    }
}
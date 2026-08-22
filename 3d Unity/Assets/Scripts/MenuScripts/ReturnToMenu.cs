using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private string menuSceneName = "MenuScene";

    private void Start()
    {
        // Set up the listener if the button is assigned
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(GoBackToMenu);
        }
    }

    // This can be called directly from the Button's OnClick in the Inspector
    public void GoBackToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    // Optional: Go back to menu using build index
    public void GoBackToMenuByIndex(int menuSceneIndex)
    {
        SceneManager.LoadScene(menuSceneIndex);
    }

    private void OnDestroy()
    {
        // Clean up the listener if we set it programmatically
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(GoBackToMenu);
        }
    }
}
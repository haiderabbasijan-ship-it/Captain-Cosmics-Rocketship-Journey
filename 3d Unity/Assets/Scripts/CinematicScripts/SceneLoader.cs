using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // This function can be called directly from a Button's OnClick event in the Inspector
    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Alternative method that loads the next scene by build index
    public void LoadNextSceneByIndex()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}
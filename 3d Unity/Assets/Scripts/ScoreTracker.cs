using UnityEngine;
// using UnityEngine.UI; // Use this for regular UI Text
using TMPro; // For TextMeshPro
using System.Collections;

public class ScoreTracker : MonoBehaviour
{
    // Point values
    public int pointsPerAsteroid = 100;
    public int pointsPerDataFragment = 1000;

    // Score text UI reference
    public TextMeshProUGUI scoreText; // TextMeshPro UI reference

    // Current score
    private int currentScore = 0;

    // Animation system
    private ScoreAnimationManager animationManager;

    void Start()
    {
        // Initialize the score display
        UpdateScoreDisplay();

        // Get or add the animation manager
        animationManager = GetComponent<ScoreAnimationManager>();
        if (animationManager == null)
        {
            animationManager = gameObject.AddComponent<ScoreAnimationManager>();
        }
    }

    // Call this when player shoots an asteroid
    public void AsteroidDestroyed(Vector3 position)
    {
        AddScore(pointsPerAsteroid, position);
    }

    // Overload for when position is not provided
    public void AsteroidDestroyed()
    {
        // Use a default position in front of the camera if not provided
        if (Camera.main != null)
        {
            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 5f;
            AddScore(pointsPerAsteroid, position);
        }
        else
        {
            AddScore(pointsPerAsteroid);
        }
    }

    // Call this when player collects a data fragment
    public void DataFragmentCollected(Vector3 position)
    {
        AddScore(pointsPerDataFragment, position);
    }

    // Overload for when position is not provided
    public void DataFragmentCollected()
    {
        // Use a default position in front of the camera if not provided
        if (Camera.main != null)
        {
            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 5f;
            AddScore(pointsPerDataFragment, position);
        }
        else
        {
            AddScore(pointsPerDataFragment);
        }
    }

    // Generic method to add any amount to the score with position
    public void AddScore(int points, Vector3 position)
    {
        currentScore += points;
        UpdateScoreDisplay();

        // Play animations
        if (animationManager != null)
        {
            animationManager.AnimateScoreChange(points, position);
        }
    }

    // Overload for when position is not provided
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    // Updates the UI text
    private void UpdateScoreDisplay()
    {
        scoreText.text = "Score: " + currentScore.ToString();
    }

    // Get the current score (useful for other scripts)
    public int GetScore()
    {
        return currentScore;
    }
}
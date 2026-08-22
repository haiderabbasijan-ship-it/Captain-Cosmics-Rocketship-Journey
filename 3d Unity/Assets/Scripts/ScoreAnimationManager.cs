using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreAnimationManager : MonoBehaviour
{
    [Header("Score Text Animation")]
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private Color pulseColor = new Color(1f, 0.8f, 0.2f); // Gold color for emphasis

    [Header("Floating Score Text")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private float floatSpeed = 1.0f;

    // Reference to the main score text
    private TextMeshProUGUI mainScoreText;
    // Original color of the score text
    private Color originalColor;
    // Original scale of the score text
    private Vector3 originalScale;
    // Is animation currently running?
    private bool isAnimating = false;

    private void Awake()
    {
        // Find the score text component
        ScoreTracker scoreTracker = GetComponent<ScoreTracker>();
        if (scoreTracker != null)
        {
            mainScoreText = scoreTracker.scoreText;

            if (mainScoreText != null)
            {
                originalColor = mainScoreText.color;
                originalScale = mainScoreText.transform.localScale;
            }
        }

        // Create a floating text prefab if not assigned
        if (floatingTextPrefab == null)
        {
            CreateFloatingTextPrefab();
        }
    }

    public void AnimateScoreChange(int points, Vector3 worldPosition)
    {
        // Animate the main score text
        if (mainScoreText != null && !isAnimating)
        {
            StartCoroutine(PulseScoreText());
        }

        // Create floating text at the specified position
        if (points != 0)
        {
            CreateFloatingText(points, worldPosition);
        }
    }

    private IEnumerator PulseScoreText()
    {
        isAnimating = true;

        // Scale up and change color
        float timer = 0;
        while (timer < pulseDuration / 2)
        {
            timer += Time.deltaTime;
            float progress = timer / (pulseDuration / 2);

            // Scale up
            mainScoreText.transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, progress);

            // Change color
            mainScoreText.color = Color.Lerp(originalColor, pulseColor, progress);

            yield return null;
        }

        // Scale down and return to original color
        timer = 0;
        while (timer < pulseDuration / 2)
        {
            timer += Time.deltaTime;
            float progress = timer / (pulseDuration / 2);

            // Scale down
            mainScoreText.transform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, progress);

            // Return to original color
            mainScoreText.color = Color.Lerp(pulseColor, originalColor, progress);

            yield return null;
        }

        // Ensure we end at exactly the original values
        mainScoreText.transform.localScale = originalScale;
        mainScoreText.color = originalColor;

        isAnimating = false;
    }

    private void CreateFloatingText(int points, Vector3 worldPosition)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("No main camera found for floating text!");
            return;
        }

        // Create the floating text object as a child of the canvas
        GameObject floatingTextObj = Instantiate(floatingTextPrefab);

        // Position it in world space but facing camera
        floatingTextObj.transform.position = worldPosition;

        // IMPORTANT: Make it face the camera directly, ignoring any rotation of the asteroid
        floatingTextObj.transform.rotation = Camera.main.transform.rotation;

        // Get the TextMeshPro component
        TextMeshPro textMesh = floatingTextObj.GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            // Set the text
            textMesh.text = (points > 0 ? "+" : "") + points.ToString();

            // Set the color based on points value
            // For data fragments (1000 points): blue color
            // For asteroids (100 points): orange color
            textMesh.color = points >= 1000 ?
                new Color(0f, 1f, 1f) :     // Cyan/bright blue for data fragments
                new Color(1f, 0.5f, 0f);    // Brighter orange for asteroids

            // Make sure text is set to face camera
            textMesh.alignment = TextAlignmentOptions.Center;

            // Start the floating animation
            StartCoroutine(AnimateFloatingText(floatingTextObj, textMesh));
        }
    }

    private IEnumerator AnimateFloatingText(GameObject textObj, TextMeshPro textMesh)
    {
        float time = 0;
        Vector3 startPos = textObj.transform.position;
        Vector3 targetPos = startPos + Vector3.up * 1.5f; // Float upward
        Vector3 startScale = textObj.transform.localScale;
        Color startColor = textMesh.color;

        while (time < 1.0f)
        {
            time += Time.deltaTime * floatSpeed;

            // Move upward
            textObj.transform.position = Vector3.Lerp(startPos, targetPos, time);

            // Scale up slightly then back down
            float scaleCurve = 1 + Mathf.Sin(time * Mathf.PI) * 0.3f;
            textObj.transform.localScale = startScale * scaleCurve;

            // Fade out gradually
            if (time > 0.5f)
            {
                float alpha = 1 - ((time - 0.5f) * 2);
                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
            }

            yield return null;
        }

        // Destroy the object when animation is complete
        Destroy(textObj);
    }

    private void CreateFloatingTextPrefab()
    {
        // Create a new GameObject for the prefab
        GameObject prefab = new GameObject("FloatingText");

        // Add TextMeshPro component
        TextMeshPro textMesh = prefab.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5;
        textMesh.fontStyle = FontStyles.Bold;

        // Make it face the camera
        prefab.AddComponent<Billboard>();

        // Set the prefab
        floatingTextPrefab = prefab;
    }
}

// Simple billboard script to make the text face the camera
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Always face the camera directly
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
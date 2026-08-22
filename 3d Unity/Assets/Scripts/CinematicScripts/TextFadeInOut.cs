using System.Collections;
using UnityEngine;
using TMPro;

public class TextFadeInOut : MonoBehaviour
{
    [Header("Component Reference")]
    [SerializeField] private TextMeshProUGUI textDisplay;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float initialDelay = 0f;

    private void Start()
    {
        // Make sure we have a TextMeshPro component
        if (textDisplay == null)
        {
            textDisplay = GetComponent<TextMeshProUGUI>();

            if (textDisplay == null)
            {
                Debug.LogError("No TextMeshProUGUI component found!");
                return;
            }
        }

        // Start with invisible text
        SetTextAlpha(0);

        // Start the fade animation
        StartCoroutine(FadeTextAnimation());
    }

    private IEnumerator FadeTextAnimation()
    {
        // Initial delay before starting animation
        yield return new WaitForSeconds(initialDelay);

        // Fade in
        float elapsedTime = 0;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            SetTextAlpha(alpha);
            yield return null;
        }

        // Ensure text is fully visible
        SetTextAlpha(1);

        // Display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsedTime = 0;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(elapsedTime / fadeOutDuration);
            SetTextAlpha(alpha);
            yield return null;
        }

        // Ensure text is fully invisible
        SetTextAlpha(0);

        // Optionally deactivate the GameObject after completion
        // gameObject.SetActive(false);
    }

    private void SetTextAlpha(float alpha)
    {
        Color currentColor = textDisplay.color;
        currentColor.a = alpha;
        textDisplay.color = currentColor;
    }
}
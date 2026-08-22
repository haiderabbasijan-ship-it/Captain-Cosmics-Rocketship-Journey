using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptainCosmic : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;

    [Header("Animation")]
    [SerializeField] private Animator captainAnimator;
    [SerializeField] private string talkAnimationTrigger = "Talk";
    [SerializeField] private string idleAnimationTrigger = "Idle";

    [Header("Audio")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioClip[] voiceClips;

    [Header("Options")]
    [SerializeField] private bool useOnlyLocalFacts = false;
    [SerializeField] private float entryAnimationTime = 0.5f;
    [SerializeField] private float exitAnimationTime = 0.5f;

    private bool isDisplayingDialogue = false;
    private bool isProcessingDataFragment = false; // Guard flag to prevent multiple simultaneous invocations
    private Vector3 originalPosition;
    private Vector3 offscreenPosition;

    private void Awake()
    {
        // Store original position for animation
        originalPosition = transform.position;

        // Calculate offscreen position
        offscreenPosition = originalPosition + new Vector3(-1000f, 0f, 0f);

        // Hide Captain at start
        transform.position = offscreenPosition;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Start()
    {
        // Hide the captain game object at start
        gameObject.SetActive(false);

        // Add listener to continue button
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseDialogue);
    }

    public void OnDataFragmentCollected(int factIndex = 0)
    {
        Debug.Log("CaptainCosmic: Data fragment collected with fact index: " + factIndex);

        // Don't trigger if we're already showing a dialogue or processing another fragment
        if (isDisplayingDialogue || isProcessingDataFragment)
        {
            Debug.Log("CaptainCosmic: Already busy with dialogue or fragment, ignoring new request");
            return;
        }

        // Set processing flag to prevent multiple simultaneous calls
        isProcessingDataFragment = true;

        // Show the captain
        gameObject.SetActive(true);

        // Start in offscreen position
        transform.position = offscreenPosition;

        // Animate the entrance
        StartCoroutine(AnimateEntry());

        // Request a Jupiter fact from OpenAI with the specific index
        if (useOnlyLocalFacts)
        {
            UseLocalFact(factIndex);
            return;
        }

        if (OpenAIManager.Instance != null)
        {
            OpenAIManager.Instance.GetJupiterFact((fact) => {
                DisplayFact(fact);
                // Clear the processing flag when done with API request
                isProcessingDataFragment = false;
            }, factIndex);
        }
        else
        {
            UseLocalFact(factIndex);
        }
    }

    private IEnumerator AnimateEntry()
    {
        float startTime = Time.time;
        Vector3 startPos = offscreenPosition;

        // Animate movement from offscreen to original position
        while (Time.time < startTime + entryAnimationTime)
        {
            float t = (Time.time - startTime) / entryAnimationTime;
            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            yield return null;
        }

        transform.position = originalPosition;
    }

    private IEnumerator AnimateExit()
    {
        float startTime = Time.time;
        Vector3 startPos = originalPosition;

        // Animate movement from original position to offscreen
        while (Time.time < startTime + exitAnimationTime)
        {
            float t = (Time.time - startTime) / exitAnimationTime;
            transform.position = Vector3.Lerp(startPos, offscreenPosition, t);
            yield return null;
        }

        transform.position = offscreenPosition;
        gameObject.SetActive(false);
    }

    private void DisplayFact(string fact)
    {
        Debug.Log("CaptainCosmic: DisplayFact called with: " + fact);

        // If response contains error indicators, use local fact instead
        if (string.IsNullOrEmpty(fact) ||
            fact.Length < 10 ||
            fact.Contains("offline") ||
            fact.Contains("database") ||
            fact.Contains("error") ||
            fact.Contains("unexpected") ||
            fact.Contains("try again"))
        {
            Debug.LogWarning("CaptainCosmic: Received fact appears to be an error message, using local fact instead");
            UseLocalFact(0);
            return;
        }

        StartCoroutine(ShowDialogue(fact));
    }

    private void UseLocalFact(int factIndex = 0)
    {
        Debug.Log("CaptainCosmic: Using local fact with index: " + factIndex);

        if (OpenAIManager.Instance != null)
        {
            OpenAIManager.Instance.UseLocalFacts(DisplayLocalFact, factIndex);
        }
        else
        {
            // Hardcoded fallback if OpenAIManager isn't available
            string[] jupiterFacts = new string[]
            {
                "Analysing data: Jupiter is so big that all the other planets in our solar system could fit inside it!",
                "Analysing data: Jupiter has a massive storm called the Great Red Spot that's been raging for hundreds of years!",
                "Analysing data: Jupiter has at least 79 moons orbiting around it!",
                "Analysing data: A day on Jupiter is only about 10 hours long, even though it's the biggest planet!",
                "Analysing data: Jupiter's powerful magnetic field is the strongest of all planets in our solar system!"
            };

            int index = factIndex % jupiterFacts.Length;
            string fact = jupiterFacts[index];
            DisplayLocalFact(fact);
        }

        // Clear the processing flag when using local facts
        isProcessingDataFragment = false;
    }

    private void DisplayLocalFact(string fact)
    {
        Debug.Log("CaptainCosmic: Displaying local fact: " + fact);
        StartCoroutine(ShowDialogue(fact));
    }

    private IEnumerator ShowDialogue(string text)
    {
        Debug.Log("CaptainCosmic: ShowDialogue coroutine started");
        isDisplayingDialogue = true;

        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Play talk animation
        if (captainAnimator != null)
            captainAnimator.SetTrigger(talkAnimationTrigger);

        // Play random voice clip
        if (voiceAudioSource != null && voiceClips != null && voiceClips.Length > 0)
        {
            voiceAudioSource.clip = voiceClips[UnityEngine.Random.Range(0, voiceClips.Length)];
            voiceAudioSource.Play();
        }

        // Type the text
        yield return StartCoroutine(TypeText(text));

        // Switch to idle animation
        if (captainAnimator != null)
            captainAnimator.SetTrigger(idleAnimationTrigger);

        // If no continue button, auto-close after delay
        if (continueButton == null)
        {
            yield return new WaitForSeconds(5f);
            CloseDialogue();
        }
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogueText != null)
        {
            // Sanitize the text to prevent display issues
            string sanitizedText = SanitizeText(text);

            dialogueText.text = "";

            // Process the text character by character
            foreach (char c in sanitizedText)
            {
                // Add the character to the text
                dialogueText.text += c;

                // Force the TextMeshPro to update immediately
                dialogueText.ForceMeshUpdate();

                // Check for potential rendering issues
                if (dialogueText.textInfo.characterCount != dialogueText.text.Length)
                {
                    Debug.LogWarning("CaptainCosmic: Potential text rendering issue detected. Fixing...");

                    // Reset and use the sanitized text directly
                    dialogueText.text = sanitizedText;
                    break;
                }

                // Skip delay for spaces to make typing feel more natural
                if (c != ' ')
                {
                    yield return new WaitForSeconds(0.04f); // Typing speed
                }
                else
                {
                    yield return new WaitForSeconds(0.01f); // Faster for spaces
                }
            }

            // Ensure final text is correct
            dialogueText.text = sanitizedText;
        }
    }

    // Helper method to sanitize text and remove potentially problematic characters
    private string SanitizeText(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "Analysing data: Jupiter is the largest planet in our solar system!";

        // Remove any non-displayable or control characters
        string sanitized = "";
        foreach (char c in input)
        {
            // Only allow standard printable characters
            if ((c >= 32 && c <= 126) || c == '\n' || c == ' ')
            {
                sanitized += c;
            }
        }

        // Make sure the prefix is correct
        if (!sanitized.StartsWith("Analysing data:"))
        {
            if (sanitized.Contains("Analysing data:"))
            {
                // Extract the part after "Analysing data:"
                int index = sanitized.IndexOf("Analysing data:");
                sanitized = "Analysing data:" + sanitized.Substring(index + "Analysing data:".Length);
            }
            else
            {
                // If completely missing, add it
                sanitized = "Analysing data: " + sanitized;
            }
        }

        // Ensure the text isn't too long for the UI
        if (sanitized.Length > 150)
        {
            sanitized = sanitized.Substring(0, 147) + "...";
        }

        return sanitized;
    }

    private void CloseDialogue()
    {
        // Hide dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Animate exit and hide the character
        StartCoroutine(AnimateExit());

        isDisplayingDialogue = false;
    }
}
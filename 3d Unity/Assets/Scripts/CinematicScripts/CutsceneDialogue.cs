using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CutsceneDialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button startMissionButton;
    [SerializeField] private Animator captainAnimator;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject captainObject; // Reference to the entire captain object
    [SerializeField] private string talkAnimationTrigger = "Talk";
    [SerializeField] private string idleAnimationTrigger = "Idle";
    [SerializeField] private float entryAnimationTime = 0.5f;

    private Vector3 originalPosition;
    private Vector3 offscreenPosition;

    private void Awake()
    {
        // Store original position for animation
        if (captainObject != null)
            originalPosition = captainObject.transform.position;

        // Calculate offscreen position
        offscreenPosition = originalPosition + new Vector3(-1000f, 0f, 0f);
    }

    private void Start()
    {
        // DISABLE THE ENTIRE CAPTAIN OBJECT
        if (captainObject != null)
            captainObject.SetActive(false);

        // Hide dialogue panel and button
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (startMissionButton != null)
            startMissionButton.gameObject.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        // Start cutscene after delay
        Invoke("StartCutscene", 3.15f);

        // Show mission button
        Invoke("ShowMissionButton", 8.0f);
    }

    private void StartCutscene()
    {
        // First activate the captain object but at offscreen position
        if (captainObject != null)
        {
            captainObject.transform.position = offscreenPosition;
            captainObject.SetActive(true);
        }

        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Start animation
        StartCoroutine(AnimateEntry());
    }

    private IEnumerator AnimateEntry()
    {
        if (captainObject == null)
            yield break;

        float startTime = Time.time;
        Vector3 startPos = offscreenPosition;

        // Animate movement from offscreen to original position
        while (Time.time < startTime + entryAnimationTime)
        {
            float t = (Time.time - startTime) / entryAnimationTime;
            captainObject.transform.position = Vector3.Lerp(startPos, originalPosition, t);
            yield return null;
        }

        captainObject.transform.position = originalPosition;

        // Play talk animation
        if (captainAnimator != null)
            captainAnimator.SetTrigger(talkAnimationTrigger);

        // Start typing the dialogue
        StartCoroutine(TypeDialogue("Our satellite near Jupiter has broken down, you must recover the lost data!"));
    }

    private IEnumerator TypeDialogue(string text)
    {
        // Clear previous text
        dialogueText.text = "";

        // Type out each character
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.04f); // Typing speed
        }
    }

    private void ShowMissionButton()
    {
        // Show mission start button
        if (startMissionButton != null)
            startMissionButton.gameObject.SetActive(true);

        // Switch to idle animation
        if (captainAnimator != null)
            captainAnimator.SetTrigger(idleAnimationTrigger);
    }
}
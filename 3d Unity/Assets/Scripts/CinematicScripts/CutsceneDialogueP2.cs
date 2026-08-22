using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class CutsceneDialogueP2 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueTextP2;
    [SerializeField] private Button nextButtonP2;
    [SerializeField] private Animator captainAnimatorP2;
    [SerializeField] private GameObject dialoguePanelP2;
    [SerializeField] private GameObject captainObjectP2; // Reference to the entire captain object
    [SerializeField] private string talkAnimationTriggerP2 = "Talk";
    [SerializeField] private string idleAnimationTriggerP2 = "Idle";
    [SerializeField] private float entryAnimationTimeP2 = 0.5f;
    private Vector3 originalPositionP2;
    private Vector3 offscreenPositionP2;
    private void Awake()
    {
        // Store original position for animation
        if (captainObjectP2 != null)
            originalPositionP2 = captainObjectP2.transform.position;
        // Calculate offscreen position
        offscreenPositionP2 = originalPositionP2 + new Vector3(-1000f, 0f, 0f);
    }
    private void Start()
    {
        // DISABLE THE ENTIRE CAPTAIN OBJECT
        if (captainObjectP2 != null)
            captainObjectP2.SetActive(false);
        // Hide dialogue panel and button
        if (dialoguePanelP2 != null)
            dialoguePanelP2.SetActive(false);
        if (nextButtonP2 != null)
            nextButtonP2.gameObject.SetActive(false);
        if (dialogueTextP2 != null)
            dialogueTextP2.text = "";
        // Start cutscene after delay
        Invoke("StartCutsceneP2", 0.1f);
        // Show mission button
        Invoke("ShowMissionButtonP2", 5.0f);
    }
    private void StartCutsceneP2()
    {
        // First activate the captain object but at offscreen position
        if (captainObjectP2 != null)
        {
            captainObjectP2.transform.position = offscreenPositionP2;
            captainObjectP2.SetActive(true);
        }
        // Show dialogue panel
        if (dialoguePanelP2 != null)
            dialoguePanelP2.SetActive(true);
        // Start animation
        StartCoroutine(AnimateEntryP2());
    }
    private IEnumerator AnimateEntryP2()
    {
        if (captainObjectP2 == null)
            yield break;
        float startTime = Time.time;
        Vector3 startPos = offscreenPositionP2;
        // Animate movement from offscreen to original position
        while (Time.time < startTime + entryAnimationTimeP2)
        {
            float t = (Time.time - startTime) / entryAnimationTimeP2;
            captainObjectP2.transform.position = Vector3.Lerp(startPos, originalPositionP2, t);
            yield return null;
        }
        captainObjectP2.transform.position = originalPositionP2;
        // Play talk animation
        if (captainAnimatorP2 != null)
            captainAnimatorP2.SetTrigger(talkAnimationTriggerP2);
        // Start typing the dialogue
        StartCoroutine(TypeDialogueP2("Woah! An asteroid storm is building up in the area. Shoot or dodge them, Good luck rookie!"));
    }
    private IEnumerator TypeDialogueP2(string text)
    {
        // Clear previous text
        dialogueTextP2.text = "";
        // Type out each character
        foreach (char c in text)
        {
            dialogueTextP2.text += c;
            yield return new WaitForSeconds(0.04f); // Typing speed
        }
    }
    private void ShowMissionButtonP2()
    {
        // Show mission start button
        if (nextButtonP2 != null)
            nextButtonP2.gameObject.SetActive(true);
        // Switch to idle animation
        if (captainAnimatorP2 != null)
            captainAnimatorP2.SetTrigger(idleAnimationTriggerP2);
    }
}
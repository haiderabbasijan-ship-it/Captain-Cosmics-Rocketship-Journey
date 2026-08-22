using UnityEngine;
using System.Collections;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Mission Failed Settings")]
    [SerializeField] private float deathDelay = 1.5f; // Delay before showing mission failed screen
    [SerializeField] private MissionEndHandler missionEndHandler;

    [Header("Death Effects")]
    [SerializeField] private GameObject explosionPrefab; // Optional explosion effect
    [SerializeField] private AudioClip deathSound; // Optional death sound
    [SerializeField] private float soundVolume = 1f;

    // Flag to prevent multiple death triggers
    private bool isPlayerDead = false;

    private void Start()
    {
        // Find the mission end handler if not assigned
        if (missionEndHandler == null)
        {
            missionEndHandler = FindObjectOfType<MissionEndHandler>();
            if (missionEndHandler == null)
            {
                Debug.LogWarning("MissionEndHandler not found! Mission failure won't work properly.");
            }
        }

        Debug.Log("PlayerDeathHandler initialized on " + gameObject.name);
    }

    // Call this when the player is hit by an asteroid or collides with satellite
    public void OnPlayerKilled()
    {
        Debug.Log("OnPlayerKilled() called on PlayerDeathHandler");

        if (!isPlayerDead)
        {
            Debug.Log("Player wasn't already dead, starting death sequence");
            isPlayerDead = true;
            StartCoroutine(HandlePlayerDeath());
        }
        else
        {
            Debug.Log("Player was already dead, ignoring additional death trigger");
        }
    }

    // Call this when time runs out
    public void OnTimeExpired()
    {
        Debug.Log("OnTimeExpired() called on PlayerDeathHandler");

        if (!isPlayerDead)
        {
            isPlayerDead = true;
            StartCoroutine(HandleTimeOut());
        }
    }

    // Handle the player's ship being destroyed
    private IEnumerator HandlePlayerDeath()
    {
        Debug.Log("HandlePlayerDeath() started - Playing death effects");

        // Play death effects
        PlayDeathEffects();

        Debug.Log("Waiting for " + deathDelay + " seconds before showing mission failed");

        // Wait for the specified delay (to show explosion, etc.)
        yield return new WaitForSeconds(deathDelay);

        Debug.Log("Death delay complete, calling MissionEndHandler.FailMission()");

        // Trigger mission failure
        if (missionEndHandler != null)
        {
            missionEndHandler.FailMission();
        }
        else
        {
            Debug.LogError("Cannot fail mission - MissionEndHandler not found!");
        }
    }

    // Handle time running out
    private IEnumerator HandleTimeOut()
    {
        Debug.Log("Time has expired. Triggering mission failure...");

        // Wait briefly
        yield return new WaitForSeconds(deathDelay);

        // Trigger mission failure
        if (missionEndHandler != null)
        {
            missionEndHandler.FailMission();
        }
        else
        {
            Debug.LogError("Cannot fail mission - MissionEndHandler not found!");
        }
    }

    // Play visual and audio effects for death
    private void PlayDeathEffects()
    {
        Debug.Log("Playing death effects");

        // Spawn explosion effect if assigned
        if (explosionPrefab != null)
        {
            Debug.Log("Instantiating explosion prefab");
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("No explosion prefab assigned");
        }

        // Play death sound if assigned
        if (deathSound != null)
        {
            Debug.Log("Playing death sound");
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
        }
        else
        {
            Debug.Log("No death sound assigned");
        }

        // Hide the player model
        Debug.Log("Hiding player renderers");
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Disable player movement/control scripts
        Debug.Log("Disabling player control scripts");
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Don't disable this script
            if (script != this)
            {
                script.enabled = false;
            }
        }
    }
}
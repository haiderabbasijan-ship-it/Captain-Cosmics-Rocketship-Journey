using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataFragment : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 1f;
    [SerializeField] private ParticleSystem collectEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float collectSoundVolume = 1f;

    [Header("Game References")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private CaptainCosmic captainCosmicReference;

    [Header("Fact Settings")]
    [SerializeField] private int factIndex = 0; // Each fragment gets a unique index

    private Vector3 startPosition;
    private float bobTimer = 0f;
    private AudioSource audioSource;
    private ScoreTracker scoreTracker; // Reference to the score tracker
    private bool isCollected = false; // Flag to prevent multiple collections

    private void Start()
    {
        startPosition = transform.position;

        // Try to find Captain Cosmic if not assigned
        if (captainCosmicReference == null)
        {
            captainCosmicReference = FindObjectOfType<CaptainCosmic>();
            if (captainCosmicReference == null)
                Debug.LogWarning("Captain Cosmic reference not found! Please assign it in the inspector.");
        }

        // Find the score tracker in the scene
        scoreTracker = FindObjectOfType<ScoreTracker>();
        if (scoreTracker == null)
            Debug.LogWarning("ScoreTracker not found in the scene! Score won't be tracked.");

        // If no fact index is assigned, auto-assign based on instance ID to ensure uniqueness
        if (factIndex == 0)
        {
            factIndex = Mathf.Abs(GetInstanceID() % 1000);
            Debug.Log("DataFragment: Auto-assigned fact index " + factIndex + " to " + gameObject.name);
        }

        // Make sure we have an audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    private void Update()
    {
        // Don't update if already collected
        if (isCollected)
            return;

        // Rotate the data fragment
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Make the data fragment bob up and down
        bobTimer += Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(bobTimer * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if already collected to prevent multiple collections
        if (isCollected)
            return;

        if (other.CompareTag(playerTag))
        {
            // Set the collected flag immediately to prevent multiple collections
            isCollected = true;

            Debug.Log("DataFragment: Player tag detected! Collecting fragment with fact index " + factIndex);

            // Add points to the score for collecting a data fragment (1000 points)
            if (scoreTracker != null)
            {
                scoreTracker.DataFragmentCollected(transform.position);
                Debug.Log("Added 1000 points to score for collecting data fragment");
            }

            // Notify the DataCollectionManager
            DataCollectionManager collectionManager = DataCollectionManager.Instance;
            if (collectionManager != null)
            {
                collectionManager.OnFragmentCollected();
                Debug.Log("DataCollectionManager updated with fragment collection");
            }
            else
            {
                Debug.LogWarning("DataCollectionManager not found in the scene!");
            }

            // Play collection effects
            PlayCollectionEffects();

            // Notify Captain Cosmic with fact index
            NotifyCaptainCosmic();

            // Disable the collider to prevent further trigger events
            Collider fragmentCollider = GetComponent<Collider>();
            if (fragmentCollider != null)
            {
                fragmentCollider.enabled = false;
            }

            // Destroy the data fragment
            Destroy(gameObject, 0.1f); // Small delay to ensure sound starts playing
        }
    }

    private void PlayCollectionEffects()
    {
        // Spawn particle effect if available
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Play sound effect if available
        if (collectSound != null)
        {
            Debug.Log("Attempting to play sound: " + collectSound.name + " at volume: " + collectSoundVolume);

            // Method 1: Create a persistent sound object that won't be destroyed with this object
            GameObject soundObject = new GameObject("CollectSound");
            soundObject.transform.position = transform.position;
            AudioSource tempAudio = soundObject.AddComponent<AudioSource>();
            tempAudio.clip = collectSound;
            tempAudio.volume = collectSoundVolume;
            tempAudio.Play();

            // Destroy the temporary object after the sound finishes playing
            Destroy(soundObject, collectSound.length + 0.1f);

            // Method 2: As a backup, also try the standard PlayClipAtPoint
            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume);
        }
        else
        {
            Debug.LogError("No collect sound assigned to data fragment!");
        }
    }

    private void NotifyCaptainCosmic()
    {
        if (captainCosmicReference != null)
        {
            captainCosmicReference.OnDataFragmentCollected(factIndex);
        }
        else
        {
            // Try finding Captain Cosmic one more time
            captainCosmicReference = FindObjectOfType<CaptainCosmic>();
            if (captainCosmicReference != null)
            {
                captainCosmicReference.OnDataFragmentCollected(factIndex);
            }
            else
            {
                Debug.LogError("Couldn't find Captain Cosmic to notify about data fragment collection!");
            }
        }
    }
}
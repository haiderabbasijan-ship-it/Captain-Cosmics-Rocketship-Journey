using UnityEngine;
using System.Collections;

public class PlayerRocketAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Audio source for engine idle sound")]
    private AudioSource idleAudioSource;
    [Tooltip("Audio source for engine thrust sound")]
    private AudioSource thrustAudioSource;

    [Header("Engine Sound Clips")]
    [Tooltip("Engine idle sound (always playing at low volume)")]
    public AudioClip idleSound;
    [Tooltip("Engine thrust sound (fades in/out with movement)")]
    public AudioClip thrustSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float idleBaseVolume = 0.2f;
    [Range(0f, 1f)]
    public float thrustMaxVolume = 0.8f;
    [Range(0f, 1f)]
    public float thrustMinVolume = 0.05f;

    [Header("Pitch Settings")]
    [Range(0.5f, 1.5f)]
    public float idleBasePitch = 0.9f;
    [Range(0.5f, 1.5f)]
    public float thrustBasePitch = 1.0f;
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.2f;

    [Header("Transition Settings")]
    [Range(0.1f, 10f)]
    public float volumeChangeSpeed = 2.0f;
    [Range(0.1f, 10f)]
    public float pitchChangeSpeed = 1.5f;

    // Movement tracking
    private PlayerController playerController;
    private Vector3 lastPosition;
    private float currentMovementMagnitude = 0f;
    private float randomPitchOffset = 0f;

    void Start()
    {
        // Get player controller reference
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component not found!");
        }

        // Create and configure idle audio source
        idleAudioSource = gameObject.AddComponent<AudioSource>();
        idleAudioSource.clip = idleSound;
        idleAudioSource.loop = true;
        idleAudioSource.spatialBlend = 1.0f;
        idleAudioSource.volume = idleBaseVolume;
        idleAudioSource.pitch = idleBasePitch;
        idleAudioSource.playOnAwake = true;
        idleAudioSource.Play();

        // Create and configure thrust audio source
        thrustAudioSource = gameObject.AddComponent<AudioSource>();
        thrustAudioSource.clip = thrustSound;
        thrustAudioSource.loop = true;
        thrustAudioSource.spatialBlend = 1.0f;
        thrustAudioSource.volume = 0f; // Start with no volume
        thrustAudioSource.pitch = thrustBasePitch;
        thrustAudioSource.Play();

        // Store initial position
        lastPosition = transform.position;

        // Start subtle sound variation coroutine
        StartCoroutine(SubtleSoundVariation());
    }

    void Update()
    {
        UpdateEngineSounds();
    }

    private void UpdateEngineSounds()
    {
        // Calculate movement magnitude
        Vector3 movement = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Get normalized magnitude of movement
        float movementMagnitude = movement.magnitude / playerController.speed;
        movementMagnitude = Mathf.Clamp01(movementMagnitude);

        // Smooth the movement value
        currentMovementMagnitude = Mathf.Lerp(currentMovementMagnitude, movementMagnitude, Time.deltaTime * volumeChangeSpeed);

        // Update thrust sound volume based on movement
        float targetThrustVolume = Mathf.Lerp(thrustMinVolume, thrustMaxVolume, currentMovementMagnitude);
        thrustAudioSource.volume = Mathf.Lerp(thrustAudioSource.volume, targetThrustVolume, Time.deltaTime * volumeChangeSpeed);

        // Slightly adjust idle volume inversely to thrust
        float targetIdleVolume = Mathf.Lerp(idleBaseVolume, idleBaseVolume * 0.4f, currentMovementMagnitude);
        idleAudioSource.volume = Mathf.Lerp(idleAudioSource.volume, targetIdleVolume, Time.deltaTime * volumeChangeSpeed);

        // Update pitches based on movement
        float thrustPitchTarget = thrustBasePitch + (currentMovementMagnitude * pitchVariation) + randomPitchOffset;
        thrustAudioSource.pitch = Mathf.Lerp(thrustAudioSource.pitch, thrustPitchTarget, Time.deltaTime * pitchChangeSpeed);

        float idlePitchTarget = idleBasePitch + (currentMovementMagnitude * pitchVariation * 0.5f) + (randomPitchOffset * 0.5f);
        idleAudioSource.pitch = Mathf.Lerp(idleAudioSource.pitch, idlePitchTarget, Time.deltaTime * pitchChangeSpeed);
    }

    // Coroutine to add subtle, random variations to engine sounds
    private IEnumerator SubtleSoundVariation()
    {
        while (true)
        {
            // Gradually change the random pitch offset for subtle variation
            float targetPitchOffset = Random.Range(-0.05f, 0.05f);
            float transitionTime = Random.Range(0.5f, 2.0f);
            float elapsedTime = 0f;
            float startPitchOffset = randomPitchOffset;

            while (elapsedTime < transitionTime)
            {
                randomPitchOffset = Mathf.Lerp(startPitchOffset, targetPitchOffset, elapsedTime / transitionTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            randomPitchOffset = targetPitchOffset;
            yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
        }
    }
}
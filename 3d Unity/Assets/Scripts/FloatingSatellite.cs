using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingSatellite : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobFrequency = 0.5f;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0);

    [Header("Collision Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionSoundVolume = 1f;
    [SerializeField] private bool debugMode = true;

    private Vector3 startPosition;
    private float bobTimer = 0f;
    private PlayerDeathHandler playerDeathHandler;

    private void Start()
    {
        startPosition = transform.position;

        // Make sure the satellite has a collider
        Collider satelliteCollider = GetComponent<Collider>();
        if (satelliteCollider == null)
        {
            Debug.LogWarning("No collider found on satellite! Adding a box collider.");
            gameObject.AddComponent<BoxCollider>();
        }

        // Find the player death handler
        playerDeathHandler = FindObjectOfType<PlayerDeathHandler>();
        if (playerDeathHandler == null && debugMode)
        {
            Debug.LogWarning("PlayerDeathHandler not found! Player death won't trigger mission failure.");
        }
    }

    private void Update()
    {
        // Rotate the satellite
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);

        // Make the satellite bob up and down
        bobTimer += Time.deltaTime;
        float bobOffset = Mathf.Sin(bobTimer * bobFrequency) * bobAmplitude;
        transform.position = startPosition + new Vector3(0, bobOffset, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            if (debugMode)
                Debug.Log("Player hit satellite via Collision!");

            // Trigger player death
            TriggerPlayerDeath(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (debugMode)
                Debug.Log("Player hit satellite via Trigger!");

            // Trigger player death
            TriggerPlayerDeath(other.gameObject);
        }
    }

    private void TriggerPlayerDeath(GameObject player)
    {
        if (debugMode)
            Debug.Log("TriggerPlayerDeath called");

        // Play explosion effect if assigned
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, player.transform.position, Quaternion.identity);
        }

        // Play explosion sound if assigned
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, player.transform.position, explosionSoundVolume);
        }

        // Notify the death handler
        if (playerDeathHandler != null)
        {
            playerDeathHandler.OnPlayerKilled();
        }
        else if (debugMode)
        {
            Debug.LogWarning("Cannot trigger mission failure - PlayerDeathHandler not found!");
        }
    }
}
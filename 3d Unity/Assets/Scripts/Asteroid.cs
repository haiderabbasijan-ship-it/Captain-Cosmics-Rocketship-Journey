using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    public float rotationSpeed = 10f;
    public float movementSpeed = 2f;

    [Header("Collision Settings")]
    public string playerTag = "Player";
    public float playerDetectionRadius = 1.5f; // Radius for additional player detection sphere

    [Header("Audio Settings")]
    public string explosionSoundName = "asteroid_explosion";

    private ScoreTracker scoreTracker;
    private PlayerDeathHandler playerDeathHandler;
    private SphereCollider triggerCollider;
    private bool playerDeathTriggered = false;

    // In Start() method of Asteroid.cs
    void Start()
    {
        // Find the score tracker in the scene
        scoreTracker = FindObjectOfType<ScoreTracker>();

        // Find player death handler directly regardless of tags
        playerDeathHandler = FindObjectOfType<PlayerDeathHandler>();

        if (playerDeathHandler != null)
        {
            Debug.Log("Asteroid found PlayerDeathHandler on: " + playerDeathHandler.gameObject.name);
        }
        else
        {
            Debug.LogWarning("PlayerDeathHandler not found in scene!");
        }

        // Add a separate trigger collider for player detection
        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.radius = playerDetectionRadius;
        triggerCollider.isTrigger = true;
    }

    void Update()
    {
        // Rotate the asteroid
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Move the asteroid forward
        transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
    }

    // Handle regular collisions (for bullets)
    void OnCollisionEnter(Collision collision)
    {
        // Only handle bullet collisions in OnCollisionEnter
        if (collision.gameObject.CompareTag("Bullet"))
        {
            HandleBulletCollision(collision);
        }
    }

    // Handle trigger collisions (for player)
    void OnTriggerEnter(Collider other)
    {
        // Only handle player collisions in OnTriggerEnter
        if (other.CompareTag(playerTag) && !playerDeathTriggered)
        {
            playerDeathTriggered = true;
            Debug.Log("Asteroid trigger detected player! Player GameObject: " + other.gameObject.name);
            HandlePlayerCollision(other.gameObject);
        }
    }

    // Handle bullet collision
    private void HandleBulletCollision(Collision collision)
    {
        // Destroy the bullet
        Destroy(collision.gameObject);

        // Add points to the score for destroying an asteroid
        if (scoreTracker != null)
        {
            scoreTracker.AsteroidDestroyed(transform.position);
        }

        // Play explosion sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundAtPosition(explosionSoundName, transform.position);
        }

        // Destroy the asteroid
        Destroy(gameObject);
    }

    // Handle player collision
    private void HandlePlayerCollision(GameObject player)
    {
        // Try to get player death handler directly if not found earlier
        if (playerDeathHandler == null)
        {
            Debug.Log("Trying to find PlayerDeathHandler directly from collision object");
            playerDeathHandler = player.GetComponent<PlayerDeathHandler>();
            if (playerDeathHandler == null)
            {
                playerDeathHandler = player.GetComponentInChildren<PlayerDeathHandler>();
            }

            if (playerDeathHandler != null)
            {
                Debug.Log("Found PlayerDeathHandler on collision object: " + playerDeathHandler.gameObject.name);
            }
            else
            {
                Debug.LogError("PlayerDeathHandler STILL not found during collision!");
            }
        }

        // Trigger player death
        if (playerDeathHandler != null)
        {
            Debug.Log("CALLING OnPlayerKilled() on PlayerDeathHandler");
            playerDeathHandler.OnPlayerKilled();
            Debug.Log("OnPlayerKilled() called successfully");
        }
        else
        {
            Debug.LogError("Cannot trigger mission failure - PlayerDeathHandler not found!");

            // Last resort: direct mission end
            MissionEndHandler missionEndHandler = FindObjectOfType<MissionEndHandler>();
            if (missionEndHandler != null)
            {
                Debug.Log("Calling MissionEndHandler.FailMission() directly as fallback");
                missionEndHandler.FailMission();
            }
        }

        // Destroy the asteroid
        Destroy(gameObject);
    }
}
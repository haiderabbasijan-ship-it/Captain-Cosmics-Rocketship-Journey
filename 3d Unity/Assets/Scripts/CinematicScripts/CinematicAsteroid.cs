using UnityEngine;

public class CinematicAsteroid : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeedX = 5f;
    public float rotationSpeedY = 10f;
    public float rotationSpeedZ = 3f;

    [Header("Movement Settings")]
    public float movementSpeed = 0.2f;
    public Vector3 movementDirection = Vector3.forward;

    [Header("Wobble Effect")]
    public bool enableWobble = true;
    public float wobbleAmount = 0.1f;
    public float wobbleSpeed = 1f;

    [Header("Slow Drift")]
    public bool enableDrift = true;
    public float driftAmount = 0.05f;
    public float driftSpeed = 0.3f;

    // Private variables for internal calculations
    private Vector3 startPosition;
    private float wobbleTime;
    private float driftTime;

    private void Start()
    {
        // Store the initial position
        startPosition = transform.position;

        // Initialize time variables with random offsets for variety
        wobbleTime = Random.Range(0f, 100f);
        driftTime = Random.Range(0f, 100f);

        // Randomize movement direction slightly for variety
        if (enableDrift)
        {
            movementDirection = Quaternion.Euler(
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f)
            ) * movementDirection;

            movementDirection.Normalize();
        }
    }

    private void Update()
    {
        // Update time variables
        wobbleTime += Time.deltaTime * wobbleSpeed;
        driftTime += Time.deltaTime * driftSpeed;

        // Apply complex rotation on all axes
        transform.Rotate(
            rotationSpeedX * Time.deltaTime,
            rotationSpeedY * Time.deltaTime,
            rotationSpeedZ * Time.deltaTime
        );

        // Calculate the wobble effect (perpendicular to movement)
        Vector3 wobbleOffset = Vector3.zero;
        if (enableWobble)
        {
            // Create a perpendicular vector for the wobble
            Vector3 wobbleDirection = Vector3.Cross(movementDirection, Vector3.up).normalized;
            if (wobbleDirection.magnitude < 0.1f)
            {
                wobbleDirection = Vector3.Cross(movementDirection, Vector3.right).normalized;
            }

            // Apply a sine wave to create the wobble
            wobbleOffset = wobbleDirection * Mathf.Sin(wobbleTime) * wobbleAmount;
        }

        // Calculate the drift effect (vertical slow movement)
        Vector3 driftOffset = Vector3.zero;
        if (enableDrift)
        {
            driftOffset = Vector3.up * Mathf.Sin(driftTime) * driftAmount;
        }

        // Move the asteroid forward with wobble and drift added
        transform.Translate(
            (movementDirection * movementSpeed + wobbleOffset + driftOffset) * Time.deltaTime,
            Space.World
        );
    }
}
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed around the Y axis in degrees per second")]
    public float rotationSpeed = 10f;

    [Tooltip("Custom rotation axis (if useCustomAxis is enabled)")]
    public Vector3 customRotationAxis = Vector3.up;

    [Tooltip("Enable to use a custom rotation axis instead of Y axis")]
    public bool useCustomAxis = false;

    [Header("Wobble Effect (Optional)")]
    [Tooltip("Enable slight wobble effect to simulate planetary physics")]
    public bool enableWobble = false;

    [Tooltip("Amount of wobble")]
    [Range(0f, 5f)]
    public float wobbleAmount = 1f;

    [Tooltip("Speed of the wobble effect")]
    public float wobbleSpeed = 0.5f;

    private float wobbleTime;

    private void Start()
    {
        // Normalize the custom axis if it's being used
        if (useCustomAxis)
        {
            customRotationAxis.Normalize();
        }
    }

    private void Update()
    {
        // Calculate rotation for this frame
        float rotationAmount = rotationSpeed * Time.deltaTime;

        // Apply rotation based on selected axis
        if (useCustomAxis)
        {
            transform.Rotate(customRotationAxis, rotationAmount, Space.World);
        }
        else
        {
            transform.Rotate(Vector3.up, rotationAmount, Space.World);
        }

        // Apply wobble effect if enabled
        if (enableWobble)
        {
            ApplyWobbleEffect();
        }
    }

    private void ApplyWobbleEffect()
    {
        wobbleTime += Time.deltaTime * wobbleSpeed;

        // Create a slight wobble using sine waves with different frequencies
        Vector3 wobble = new Vector3(
            Mathf.Sin(wobbleTime * 0.9f),
            0f,
            Mathf.Sin(wobbleTime * 1.1f)
        ) * wobbleAmount * Time.deltaTime;

        // Apply a very subtle rotation to simulate wobble
        transform.Rotate(wobble, Space.Self);
    }
}
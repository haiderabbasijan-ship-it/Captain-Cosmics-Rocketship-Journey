using UnityEngine;

public class JupiterRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 5.0f;  // Rotation speed in degrees per second
    public Vector3 rotationAxis = new Vector3(0, 1, 0);  // Default: rotate around Y axis

    void Update()
    {
        // Rotate Jupiter around the specified axis
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
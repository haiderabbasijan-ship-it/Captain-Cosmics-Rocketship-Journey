using UnityEngine;
public class DataFragmentRotation : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 90, 0); // Degrees per second
    [SerializeField] private bool randomizeStartRotation = true;
    [SerializeField] private bool randomizeRotationSpeed = false;
    [SerializeField] private float speedVariation = 0.2f; // 20% variation
    [SerializeField] private float startDelay = 3.0f; // Delay in seconds before rotation begins

    private Vector3 actualRotationSpeed;
    private float timer = 0f;
    private bool canRotate = false;

    private void Start()
    {
        // Optionally randomize starting rotation
        if (randomizeStartRotation)
        {
            transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );
        }

        // Optionally randomize rotation speed
        if (randomizeRotationSpeed)
        {
            float randomFactor = 1f + Random.Range(-speedVariation, speedVariation);
            actualRotationSpeed = rotationSpeed * randomFactor;
        }
        else
        {
            actualRotationSpeed = rotationSpeed;
        }
    }

    private void Update()
    {
        // Check if we can start rotating
        if (!canRotate)
        {
            timer += Time.deltaTime;
            if (timer >= startDelay)
            {
                canRotate = true;
            }
        }

        // Apply rotation only if we can rotate
        if (canRotate)
        {
            transform.Rotate(actualRotationSpeed * Time.deltaTime);
        }
    }
}
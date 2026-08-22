using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The rocket ship (or object to follow)
    public Vector3 offset = new Vector3(0, 2, -5); // Offset from target
    public float smoothSpeed = 0.75f; // Smooth speed for camera movement

    [Header("Target Loss Handling")]
    public bool freezeCameraOnTargetLoss = true; // Whether to freeze the camera when target is lost
    public float targetSearchInterval = 2.0f; // How often to search for target if lost

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastKnownPosition;
    private Quaternion lastKnownRotation;
    private float searchTimer = 0f;
    private bool targetWasLost = false;

    void Start()
    {
        // Store initial position and rotation in case target is never found
        lastKnownPosition = transform.position;
        lastKnownRotation = transform.rotation;

        // Try to find target if not assigned
        if (target == null)
        {
            FindTarget();
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Reset target lost flag if we have a target
            if (targetWasLost)
            {
                targetWasLost = false;
                Debug.Log("Target found again in CameraFollow script");
            }

            // Store last known good position and rotation
            lastKnownPosition = target.position;
            lastKnownRotation = target.rotation;

            // Calculate desired position based on the target's rotation and the offset
            Vector3 desiredPosition = target.position + target.TransformDirection(offset);

            // Smoothly move the camera towards the desired position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            // Make the camera always look at the target
            transform.LookAt(target);
        }
        else
        {
            // Target is missing
            if (!targetWasLost)
            {
                // First time we notice the target is gone
                targetWasLost = true;
                Debug.LogWarning("Target lost in CameraFollow script on " + gameObject.name);
            }

            // Try to find the target periodically
            searchTimer += Time.deltaTime;
            if (searchTimer >= targetSearchInterval)
            {
                FindTarget();
                searchTimer = 0f;
            }

            // If not freezing the camera, we can add behavior here
            if (!freezeCameraOnTargetLoss)
            {
                // Optional: Add any custom camera behavior when target is lost
                // For example, slowly return to a default position
            }
        }
    }

    // Try to find a suitable target if the current one is lost
    private void FindTarget()
    {
        // Try to find player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("CameraFollow found new target: " + target.name);
        }
    }
}
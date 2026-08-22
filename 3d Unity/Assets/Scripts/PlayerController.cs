using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f; // Speed at which the rocketship moves
    public float rotationSpeed = 100.0f; // Speed at which the rocketship rotates
    public float mouseSensitivity = 2.0f; // Sensitivity for mouse movement
    public GameObject explosionPrefab; // Prefab of the explosion effect

    private float pitch = 0.0f; // Rotation around the x-axis
    private float yaw = 0.0f; // Rotation around the y-axis

    void Update()
    {
        // Get input from the user
        float moveVertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow keys for forward/backward movement
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow keys for left/right movement
        float moveUp = Input.GetKey(KeyCode.U) ? 1 : (Input.GetKey(KeyCode.Z) ? -1 : 0); // U/Z keys for up/down movement

        // Move the rocketship forward/backward, left/right, and up/down
        Vector3 movement = new Vector3(moveHorizontal, moveUp, moveVertical) * speed * Time.deltaTime;
        transform.Translate(movement);

        // Get mouse input for looking around
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Calculate new rotation values
        yaw += mouseX;
        pitch -= mouseY;

        // Apply rotation to the rocketship
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the player collides with an asteroid
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            // Instantiate the explosion effect at the player's position and rotation
            Instantiate(explosionPrefab, transform.position, transform.rotation);
            Debug.Log("Player ship exploded at: " + Time.time);

            // Destroy the player ship
            Destroy(gameObject);
            Debug.Log("Player ship destroyed at: " + Time.time);
        }
    }
}





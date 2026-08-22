using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab of the bullet to be shot
    public Transform firePoint;     // Point from where the bullet will be fired
    public float bulletForce = 50f; // Speed of the bullet (adjust this value to change bullet speed)

    // Optional audio variables
    [Header("Audio Settings")]
    public bool usePositionalAudio = true; // If true, sound comes from gun position
    public string shootSoundName = "gun_shot"; // Name of the sound in AudioManager

    void Update()
    {
        // Check if the shoot button is pressed
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Instantiate the bullet at the fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Get the Rigidbody component of the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // Add force to the bullet to make it move
        rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);

        // Play gun shot sound
        if (usePositionalAudio)
        {
            // Play the sound from the gun's position in 3D space
            AudioManager.Instance.PlaySoundAtPosition(shootSoundName, transform.position);
        }
        else
        {
            // Play the sound normally (not position-based)
            AudioManager.Instance.PlaySound(shootSoundName);
        }
    }
}
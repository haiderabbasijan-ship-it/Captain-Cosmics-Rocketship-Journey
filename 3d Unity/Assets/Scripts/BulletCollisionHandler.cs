using UnityEngine;

public class BulletCollisionHandler : MonoBehaviour
{
    public GameObject explosionPrefab; // Prefab of the explosion effect

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name + " at: " + Time.time);

        // Check if the collided object is an asteroid
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Instantiate(explosionPrefab, collision.transform.position, collision.transform.rotation);
            Debug.Log("Explosion instantiated at: " + collision.transform.position + " at: " + Time.time);

            Destroy(collision.gameObject);
            Debug.Log("Asteroid destroyed at: " + Time.time);

            Destroy(gameObject);
            Debug.Log("Bullet destroyed at: " + Time.time);
        }
    }
}


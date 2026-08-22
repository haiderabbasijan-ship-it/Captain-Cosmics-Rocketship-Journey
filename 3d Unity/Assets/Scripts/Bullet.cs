using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2f; // Time after which the bullet will be destroyed

    void Start()
    {
        // Destroy the bullet after the specified lifetime
        Destroy(gameObject, lifeTime);
        Debug.Log("Bullet will be destroyed after: " + lifeTime + " seconds");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name);
        // Destroy the bullet on collision with anything
        Destroy(gameObject);
        Debug.Log("Bullet destroyed on collision");
    }
}

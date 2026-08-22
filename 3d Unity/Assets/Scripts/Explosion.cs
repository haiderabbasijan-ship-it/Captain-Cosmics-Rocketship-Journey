using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionDuration = 2.0f; // Duration of the explosion effect

    void Start()
    {
        Debug.Log("Explosion effect started at: " + Time.time);

        // Ensure the Particle System only plays once
        var particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var mainModule = particleSystem.main;
            mainModule.loop = false; // Disable looping

            particleSystem.Play();
            Debug.Log("Particle System started at: " + Time.time);
        }

        // Destroy the explosion effect after the specified duration
        Destroy(gameObject, explosionDuration);
        Debug.Log("Explosion GameObject will be destroyed after: " + explosionDuration + " seconds at: " + Time.time);
    }
}





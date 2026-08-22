using UnityEngine;

public class ThrusterEffect : MonoBehaviour
{
    public ParticleSystem thrusterParticles; // Reference to the particle system
    public float maxEmissionRate = 30f;     // Maximum emission rate when at full thrust
    public float minEmissionRate = 5f;      // Minimum emission when idle

    private ParticleSystem.EmissionModule emission;
    private PlayerController playerController; // Reference to the player controller

    void Start()
    {
        // Get player controller reference
        playerController = GetComponentInParent<PlayerController>();

        // If thruster particles not assigned in inspector
        if (thrusterParticles == null)
        {
            Debug.LogError("Thruster particles not assigned to ThrusterEffect script!");
            return;
        }

        // Get emission module from particle system
        emission = thrusterParticles.emission;
    }

    void Update()
    {
        // Get input from user (same input used for ship movement)
        float moveVertical = Input.GetAxis("Vertical");

        // Calculate emission rate based on forward thrust
        float currentEmissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, Mathf.Max(0, moveVertical));

        // Update emission rate
        var emissionRate = emission.rateOverTime;
        emissionRate.constant = currentEmissionRate;
        emission.rateOverTime = emissionRate;

        // If moving backward, reduce particles
        if (moveVertical < 0)
        {
            var emissionRateReduced = emission.rateOverTime;
            emissionRateReduced.constant = minEmissionRate / 2;
            emission.rateOverTime = emissionRateReduced;
        }
    }
}
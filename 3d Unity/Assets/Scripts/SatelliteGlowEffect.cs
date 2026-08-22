using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatelliteGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private GameObject glowEffect; // This should be a child with a renderer or particle system
    [SerializeField] private bool pulseEffect = true;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;

    [Header("Activation")]
    [SerializeField] private bool activeOnlyWhenAllFragmentsCollected = true;

    private DataCollectionManager dataManager;
    private float currentPulseTime = 0f;
    private Light glowLight;
    private bool isGlowing = false;

    private void Start()
    {
        // Make sure the glow effect is initially disabled
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }

        // Try to find a light component for pulsing
        glowLight = glowEffect?.GetComponentInChildren<Light>();

        // Find the data collection manager
        dataManager = FindObjectOfType<DataCollectionManager>();

        if (!activeOnlyWhenAllFragmentsCollected)
        {
            // If we don't need to wait for fragments, activate immediately
            ActivateGlowEffect();
        }
    }

    private void Update()
    {
        // If we're waiting for fragments to be collected
        if (activeOnlyWhenAllFragmentsCollected && !isGlowing)
        {
            // Check if all fragments are collected
            if (dataManager != null && dataManager.AllFragmentsCollected())
            {
                ActivateGlowEffect();
            }
        }

        // If the glow is active and we want it to pulse
        if (isGlowing && pulseEffect && glowLight != null)
        {
            // Update the pulse timer
            currentPulseTime += Time.deltaTime * pulseSpeed;

            // Calculate the new intensity using a sine wave
            float intensityFactor = Mathf.Sin(currentPulseTime) * 0.5f + 0.5f; // Convert -1,1 to 0,1
            float newIntensity = Mathf.Lerp(minIntensity, maxIntensity, intensityFactor);

            // Apply the new intensity
            glowLight.intensity = newIntensity;
        }
    }

    public void ActivateGlowEffect()
    {
        if (!isGlowing && glowEffect != null)
        {
            glowEffect.SetActive(true);
            isGlowing = true;

            // If it's a particle system, make sure it's playing
            ParticleSystem particles = glowEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
            }
        }
    }

    public void DeactivateGlowEffect()
    {
        if (isGlowing && glowEffect != null)
        {
            glowEffect.SetActive(false);
            isGlowing = false;
        }
    }
}
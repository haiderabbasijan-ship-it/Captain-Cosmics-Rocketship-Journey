using UnityEngine;

public class SatelliteExplosion : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private GameObject[] dataFragments;
    [SerializeField] private float explosionForce = 500f;  // Increased force
    [SerializeField] private float explosionRadius = 10f;  // Increased radius

    [Header("Audio")]
    [SerializeField] private bool useAudioManager = true;
    [SerializeField] private AudioClip explosionClip; // Fallback if not using AudioManager

    private void Start()
    {
        // Trigger explosion after 3 seconds
        Invoke("TriggerExplosion", 3f);
    }

    public void TriggerExplosion()
    {
        // Play explosion sound
        PlayExplosionSound();

        // Ensure explosion effect is playing
        if (explosionEffect != null)
        {
            Debug.Log("Attempting to play explosion effect");
            explosionEffect.Play();
        }
        else
        {
            Debug.LogError("No explosion effect assigned!");
        }

        // Scatter data fragments
        if (dataFragments != null)
        {
            foreach (GameObject fragment in dataFragments)
            {
                if (fragment != null)
                {
                    // Detach from parent
                    fragment.transform.SetParent(null);

                    // Ensure Rigidbody exists
                    Rigidbody rb = fragment.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = fragment.AddComponent<Rigidbody>();
                    }

                    // Ensure Rigidbody is not kinematic
                    rb.isKinematic = false;

                    // Add explosion force with more power
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                    Debug.Log($"Fragment {fragment.name} scattered");
                }
            }
        }
        else
        {
            Debug.LogError("No data fragments assigned!");
        }
    }

    private void PlayExplosionSound()
    {
        if (useAudioManager)
        {
            // Use the CutsceneAudioManager if available
            if (CutsceneAudioManager.Instance != null)
            {
                CutsceneAudioManager.Instance.PlayExplosionSound();
                Debug.Log("Playing explosion sound via CutsceneAudioManager");
            }
            else
            {
                Debug.LogWarning("CutsceneAudioManager not found! Using fallback sound.");
                PlayFallbackSound();
            }
        }
        else
        {
            // Use fallback sound attached to this object
            PlayFallbackSound();
        }
    }

    private void PlayFallbackSound()
    {
        if (explosionClip != null)
        {
            // Create a temporary audio source for the explosion sound
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = explosionClip;
            audioSource.volume = 0.8f;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.Play();

            // Destroy the audio source after the clip finishes playing
            Destroy(audioSource, explosionClip.length + 0.1f);
            Debug.Log("Playing fallback explosion sound");
        }
    }
}
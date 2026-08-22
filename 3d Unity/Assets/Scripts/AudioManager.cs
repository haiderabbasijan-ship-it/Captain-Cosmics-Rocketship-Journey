using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton pattern
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;

        [Range(0f, 3f)] // Increased max volume to 3.0
        public float volume = 1.5f; // Default to higher volume

        [Range(0.1f, 3f)]
        public float pitch = 1f;

        public bool loop = false;

        [Range(0f, 1f)]
        public float spatialBlend = 1f; // Default to 3D sound
    }

    // Sound collections
    public Sound[] sounds;
    public Sound backgroundMusic;
    public Sound spaceAmbience; // New field for space ambience

    // Sound dictionaries for quick lookup
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

    // Component references
    private AudioSource musicSource;
    private AudioSource ambienceSource; // New source for space ambience

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create music audio source
        musicSource = gameObject.AddComponent<AudioSource>();

        // Create ambience audio source
        ambienceSource = gameObject.AddComponent<AudioSource>();

        // Set up music audio source
        if (backgroundMusic.clip != null)
        {
            musicSource.clip = backgroundMusic.clip;
            musicSource.volume = backgroundMusic.volume;
            musicSource.pitch = backgroundMusic.pitch;
            musicSource.loop = true;
            musicSource.playOnAwake = true;
            musicSource.Play();
        }

        // Set up space ambience audio source
        if (spaceAmbience.clip != null)
        {
            ambienceSource.clip = spaceAmbience.clip;
            ambienceSource.volume = spaceAmbience.volume;
            ambienceSource.pitch = spaceAmbience.pitch;
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = true;
            ambienceSource.Play();
        }

        // Add all sounds to dictionary
        foreach (Sound sound in sounds)
        {
            soundDictionary.Add(sound.name, sound);
        }
    }

    // Play a sound effect - similar to DataFragment approach
    public void PlaySound(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            // Create a dedicated game object for this sound
            GameObject soundObject = new GameObject("Sound_" + name);
            soundObject.transform.position = Camera.main.transform.position; // Play near camera for 2D sounds

            // Add audio source component
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.pitch = sound.pitch;
            audioSource.spatialBlend = 0f; // Force 2D for non-positional sounds

            // Play the sound
            audioSource.Play();

            // Destroy the game object after the sound is done
            Destroy(soundObject, sound.clip.length + 0.1f);

            Debug.Log($"Playing sound: {name} at volume: {sound.volume}");
        }
        else
        {
            Debug.LogWarning("Sound " + name + " not found!");
        }
    }

    // Play a 3D sound at a position - similar to DataFragment approach
    public void PlaySoundAtPosition(string name, Vector3 position)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            // Method 1: Create a persistent sound object
            GameObject soundObject = new GameObject("Sound3D_" + name);
            soundObject.transform.position = position;

            // Add audio source component
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            audioSource.pitch = sound.pitch;
            audioSource.spatialBlend = sound.spatialBlend; // Use the specified spatial blend

            // Play the sound
            audioSource.Play();

            // Destroy the game object after the sound is done
            Destroy(soundObject, sound.clip.length + 0.1f);

            // Method 2: Also use PlayClipAtPoint as backup
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume);

            Debug.Log($"Playing positional sound: {name} at position: {position} with volume: {sound.volume}");
        }
        else
        {
            Debug.LogWarning("Sound " + name + " not found!");
        }
    }

    // Set music volume
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    // Space ambience controls
    public void SetAmbienceVolume(float volume)
    {
        if (ambienceSource != null)
        {
            ambienceSource.volume = volume;
        }
    }

    public void PauseAmbience()
    {
        if (ambienceSource != null && ambienceSource.isPlaying)
        {
            ambienceSource.Pause();
        }
    }

    public void ResumeAmbience()
    {
        if (ambienceSource != null && !ambienceSource.isPlaying)
        {
            ambienceSource.Play();
        }
    }

    // Change ambient sound with optional crossfade
    public void ChangeAmbience(AudioClip newAmbienceClip, float crossfadeDuration = 1.0f)
    {
        if (ambienceSource != null && newAmbienceClip != null)
        {
            StartCoroutine(CrossfadeAmbience(newAmbienceClip, crossfadeDuration));
        }
    }

    private System.Collections.IEnumerator CrossfadeAmbience(AudioClip newClip, float duration)
    {
        // Create temporary source for crossfade
        GameObject tempObj = new GameObject("TempAmbience");
        tempObj.transform.parent = this.transform;
        AudioSource tempSource = tempObj.AddComponent<AudioSource>();

        // Set up temp source with new clip
        tempSource.clip = newClip;
        tempSource.loop = true;
        tempSource.volume = 0;
        tempSource.Play();

        // Store original volume
        float originalVolume = ambienceSource.volume;

        // Fade out original and fade in new
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float ratio = timer / duration;

            ambienceSource.volume = originalVolume * (1 - ratio);
            tempSource.volume = originalVolume * ratio;

            yield return null;
        }

        // Switch to new ambience
        ambienceSource.Stop();
        ambienceSource.clip = newClip;
        ambienceSource.volume = originalVolume;
        ambienceSource.Play();

        // Clean up temp
        Destroy(tempObj);
    }
}
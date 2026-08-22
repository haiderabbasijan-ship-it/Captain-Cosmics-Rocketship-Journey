using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A dedicated audio manager for the mission results scene.
/// Handles background music and button click sounds.
/// Ensures button sounds complete playing even during scene transitions.
/// </summary>
public class ResultsAudioManager : MonoBehaviour
{
    [System.Serializable]
    public class ResultsSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 0.5f;
    }

    [Header("Button Sounds")]
    [SerializeField] private ResultsSound buttonClickSound;

    [Header("Background Music")]
    [SerializeField] private ResultsSound backgroundMusic;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupAllButtons = true;

    // Audio source references
    private AudioSource musicSource;
    private AudioSource sfxSource;

    // Sound pool for button clicks to prevent overlapping issues
    private List<AudioSource> soundPool = new List<AudioSource>();
    private int poolSize = 3;
    private int currentPoolIndex = 0;

    // Keep track of sound pool gameObjects separately for persistence
    private static List<GameObject> persistentSoundPool = new List<GameObject>();
    private static int persistentPoolSize = 5;
    private static int persistentPoolIndex = 0;

    private void Awake()
    {
        // Set up audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // Create sound pool for button clicks
        for (int i = 0; i < poolSize; i++)
        {
            GameObject poolObj = new GameObject($"SoundPool_{i}");
            poolObj.transform.parent = this.transform;
            AudioSource source = poolObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            soundPool.Add(source);
        }

        // Create persistent sound pool if it doesn't exist yet
        if (persistentSoundPool.Count == 0)
        {
            for (int i = 0; i < persistentPoolSize; i++)
            {
                GameObject persistentObj = new GameObject($"PersistentSoundPool_{i}");
                DontDestroyOnLoad(persistentObj);
                AudioSource persistentSource = persistentObj.AddComponent<AudioSource>();
                persistentSource.playOnAwake = false;
                persistentSource.loop = false;
                persistentSoundPool.Add(persistentObj);
            }
        }
    }

    private void Start()
    {
        // Start background music if configured
        if (playMusicOnStart && backgroundMusic.clip != null)
        {
            PlayBackgroundMusic();
        }

        // Automatically set up all buttons in the scene
        if (autoSetupAllButtons)
        {
            SetupAllButtons();
        }
    }

    /// <summary>
    /// Auto-configures all buttons in the scene to play the click sound
    /// </summary>
    public void SetupAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button button in allButtons)
        {
            // Add the click sound to each button's onClick event
            button.onClick.AddListener(PlayButtonClickSound);
        }
        Debug.Log($"Results Audio Manager: Set up {allButtons.Length} buttons with click sounds");
    }

    /// <summary>
    /// Sets up a specific button to play the click sound
    /// </summary>
    public void SetupButton(Button button)
    {
        if (button != null)
        {
            button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    /// <summary>
    /// Plays the button click sound using sound pooling to prevent issues
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (buttonClickSound.clip == null) return;

        // Use both local and persistent sound pooling
        // Local pool for normal playback (in-scene)
        AudioSource source = soundPool[currentPoolIndex];
        source.clip = buttonClickSound.clip;
        source.volume = buttonClickSound.volume;
        source.Play();

        // Move to next pool item
        currentPoolIndex = (currentPoolIndex + 1) % poolSize;

        // Also play in persistent pool to ensure it plays even through scene changes
        AudioSource persistentSource = persistentSoundPool[persistentPoolIndex].GetComponent<AudioSource>();
        persistentSource.clip = buttonClickSound.clip;
        persistentSource.volume = buttonClickSound.volume;
        persistentSource.Play();

        // Start cleanup coroutine for this persistent sound
        StartCoroutine(CleanupPersistentSound(persistentSource));

        // Move to next persistent pool item
        persistentPoolIndex = (persistentPoolIndex + 1) % persistentPoolSize;
    }

    /// <summary>
    /// Cleans up a persistent sound after it finishes playing
    /// </summary>
    private IEnumerator CleanupPersistentSound(AudioSource source)
    {
        // Wait until the clip has finished playing (plus a small buffer)
        float clipLength = source.clip.length;
        yield return new WaitForSeconds(clipLength + 0.1f);

        // Reset the source for future use
        source.clip = null;
    }

    /// <summary>
    /// Plays the button click sound that will persist through scene changes
    /// </summary>
    public static void PlayPersistentButtonSound(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null || persistentSoundPool.Count == 0) return;

        AudioSource persistentSource = persistentSoundPool[persistentPoolIndex].GetComponent<AudioSource>();
        persistentSource.clip = clip;
        persistentSource.volume = volume;
        persistentSource.Play();

        // Move to next persistent pool item
        persistentPoolIndex = (persistentPoolIndex + 1) % persistentPoolSize;
    }

    /// <summary>
    /// Plays the background music
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic.clip == null) return;

        musicSource.clip = backgroundMusic.clip;
        musicSource.volume = backgroundMusic.volume;
        musicSource.Play();
    }

    /// <summary>
    /// Stops the background music
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Sets the volume of the background music
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Sets the volume for button click sounds
    /// </summary>
    public void SetButtonVolume(float volume)
    {
        buttonClickSound.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Plays the button click sound for UI transitions
    /// Ensures the sound will persist through scene changes
    /// </summary>
    public void PlayTransitionSound()
    {
        // Play in both regular and persistent pools to ensure it completes
        PlayButtonClickSound();
    }
}
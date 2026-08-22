using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// A dedicated audio manager for menu sounds that's separate from your main game audio system.
/// This handles UI button sounds and menu music specifically.
/// Auto-sets up all buttons in the scene and maintains music continuity between menu, controls, and credits scenes.
/// Includes fade in/out functionality for smoother audio transitions.
/// </summary>
public class MenuAudioManager : MonoBehaviour
{
    // Singleton instance
    public static MenuAudioManager Instance { get; private set; }

    [System.Serializable]
    public class MenuSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 0.5f;
    }

    [Header("Button Sounds")]
    [SerializeField] private MenuSound buttonClickSound;

    [Header("Menu Music")]
    [SerializeField] private MenuSound menuMusic;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupAllButtons = true;

    [Header("Scene Management")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private List<string> continuityScenesNames = new List<string>(); // Controls, Credits scenes
    [SerializeField] private bool destroyWhenLeavingMenuScenes = false;

    // Sound pool for button clicks to prevent overlapping issues
    private List<AudioSource> soundPool = new List<AudioSource>();
    private int poolSize = 3;
    private int currentPoolIndex = 0;

    // Persistent sound pool for transition sounds
    private static List<GameObject> persistentSoundPool = new List<GameObject>();
    private static int persistentPoolSize = 5;
    private static int persistentPoolIndex = 0;

    // Audio source references
    private AudioSource musicSource;

    // Scene tracking
    private string currentSceneName;

    // Fade coroutines
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Set up singleton pattern
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

        // Set up audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f; // Start at zero for fade in

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

        // Store initial scene name
        currentSceneName = SceneManager.GetActiveScene().name;

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Initialize the continuity scenes list if empty
        if (continuityScenesNames.Count == 0)
        {
            // Add default continuity scenes - modify these to match your scene names
            continuityScenesNames.Add("Controls");
            continuityScenesNames.Add("Credits");
        }

        // Start menu music if configured and we're in a valid scene
        if (playMusicOnStart && menuMusic.clip != null && IsInContinuityScene())
        {
            PlayMenuMusicWithFade();
        }

        // Automatically set up all buttons in the scene
        if (autoSetupAllButtons)
        {
            SetupAllButtons();
        }
    }

    private void Update()
    {
        // Check if we're not in a continuity scene
        if (!IsInContinuityScene())
        {
            // Stop menu music if we've left all menu-related scenes
            if (musicSource.isPlaying && fadeCoroutine == null)
            {
                StopMusicWithFade();
            }

            // Optionally destroy this manager if configured to do so
            if (destroyWhenLeavingMenuScenes && !musicSource.isPlaying && fadeCoroutine == null)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Called when a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update current scene name
        currentSceneName = scene.name;

        // Check if we've entered a continuity scene
        if (IsInContinuityScene())
        {
            // Restart menu music if it's not playing
            if (playMusicOnStart && menuMusic.clip != null && !musicSource.isPlaying)
            {
                PlayMenuMusicWithFade();
            }

            // Set up buttons in the new scene
            if (autoSetupAllButtons)
            {
                SetupAllButtons();
            }
        }
        else
        {
            // Stop menu music if we've left all menu-related scenes
            if (musicSource.isPlaying)
            {
                StopMusicWithFade();
            }
        }
    }

    /// <summary>
    /// Checks if we're currently in the menu scene or any continuity scene
    /// </summary>
    private bool IsInContinuityScene()
    {
        if (currentSceneName == menuSceneName)
        {
            return true;
        }

        return continuityScenesNames.Contains(currentSceneName);
    }

    /// <summary>
    /// Add a scene name to the list of scenes where menu music should continue playing
    /// </summary>
    public void AddContinuityScene(string sceneName)
    {
        if (!continuityScenesNames.Contains(sceneName))
        {
            continuityScenesNames.Add(sceneName);
        }
    }

    /// <summary>
    /// Remove a scene name from the continuity list
    /// </summary>
    public void RemoveContinuityScene(string sceneName)
    {
        if (continuityScenesNames.Contains(sceneName))
        {
            continuityScenesNames.Remove(sceneName);
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
        Debug.Log($"Menu Audio Manager: Set up {allButtons.Length} buttons with click sounds");
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

        // Use sound pooling to prevent sound overlap issues
        AudioSource source = soundPool[currentPoolIndex];
        source.clip = buttonClickSound.clip;
        source.volume = buttonClickSound.volume;
        source.Play();

        // Move to next pool item
        currentPoolIndex = (currentPoolIndex + 1) % poolSize;
    }

    /// <summary>
    /// Plays the button click sound that persists through scene transitions
    /// </summary>
    public void PlayTransitionButtonSound()
    {
        if (buttonClickSound.clip == null || persistentSoundPool.Count == 0) return;

        // Play the regular in-scene sound
        PlayButtonClickSound();

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
    /// Plays the menu music immediately (no fade)
    /// </summary>
    public void PlayMenuMusic()
    {
        if (menuMusic.clip == null) return;

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        musicSource.clip = menuMusic.clip;
        musicSource.volume = menuMusic.volume;
        musicSource.Play();
    }

    /// <summary>
    /// Plays the menu music with fade in
    /// </summary>
    public void PlayMenuMusicWithFade()
    {
        if (menuMusic.clip == null) return;

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Set up music source
        musicSource.clip = menuMusic.clip;

        // If not already playing, start from volume 0
        if (!musicSource.isPlaying)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            // Start fade in coroutine
            fadeCoroutine = StartCoroutine(FadeAudioSource(musicSource, 0f, menuMusic.volume, fadeInDuration));
        }
        else
        {
            // If already playing, just ensure it's at the proper volume
            fadeCoroutine = StartCoroutine(FadeAudioSource(musicSource, musicSource.volume, menuMusic.volume, fadeInDuration));
        }
    }

    /// <summary>
    /// Stops the menu music immediately (no fade)
    /// </summary>
    public void StopMusic()
    {
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        musicSource.Stop();
    }

    /// <summary>
    /// Stops the menu music with fade out
    /// </summary>
    public void StopMusicWithFade()
    {
        if (!musicSource.isPlaying) return;

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Start fade out coroutine
        fadeCoroutine = StartCoroutine(FadeAudioSource(musicSource, musicSource.volume, 0f, fadeOutDuration, true));
    }

    /// <summary>
    /// Fades an audio source from one volume to another over time
    /// </summary>
    private IEnumerator FadeAudioSource(AudioSource source, float startVolume, float targetVolume, float duration, bool stopAfterFade = false)
    {
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            // Calculate current volume using the animation curve for smoother transitions
            float normalizedTime = timeElapsed / duration;
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            source.volume = Mathf.Lerp(startVolume, targetVolume, curveValue);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we end exactly at the target volume
        source.volume = targetVolume;

        // Stop the audio if requested (for fade out)
        if (stopAfterFade && targetVolume <= 0f)
        {
            source.Stop();
        }

        // Clear coroutine reference
        fadeCoroutine = null;
    }

    /// <summary>
    /// Sets the volume of the menu music with fade
    /// </summary>
    public void SetMusicVolumeWithFade(float volume, float fadeDuration)
    {
        if (!musicSource.isPlaying) return;

        // Clamp volume to valid range
        volume = Mathf.Clamp01(volume);

        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Start fade coroutine
        fadeCoroutine = StartCoroutine(FadeAudioSource(musicSource, musicSource.volume, volume, fadeDuration));
    }

    /// <summary>
    /// Sets the volume of the menu music immediately (no fade)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

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
    /// Adjusts the fade in duration
    /// </summary>
    public void SetFadeInDuration(float duration)
    {
        fadeInDuration = Mathf.Max(0.1f, duration);
    }

    /// <summary>
    /// Adjusts the fade out duration
    /// </summary>
    public void SetFadeOutDuration(float duration)
    {
        fadeOutDuration = Mathf.Max(0.1f, duration);
    }
}
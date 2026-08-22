using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// A dedicated audio manager for cutscenes that handles background music,
/// interface sounds, ambient sounds, and special effects like explosions during cinematic sequences.
/// Supports multiple cutscene scenes.
/// </summary>
public class CutsceneAudioManager : MonoBehaviour
{
    // Singleton instance
    public static CutsceneAudioManager Instance { get; private set; }

    [System.Serializable]
    public class CutsceneSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 0.5f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
    }

    [Header("Background Music")]
    [SerializeField] private CutsceneSound backgroundMusic;
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;

    [Header("Ambient Sound")]
    [SerializeField] private CutsceneSound spaceAmbience;
    [SerializeField] private bool playAmbienceOnStart = true;
    [SerializeField] private float ambienceFadeInDuration = 3f;
    [SerializeField] private float ambienceFadeOutDuration = 2f;

    [Header("Interface Sounds")]
    [SerializeField] private CutsceneSound buttonClickSound;
    [SerializeField] private CutsceneSound dialogueAdvanceSound;

    [Header("Special Effects")]
    [SerializeField] private CutsceneSound explosionSound;
    [SerializeField] private CutsceneSound[] specialEffectSounds;

    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupAllButtons = true;

    [Header("Scene Management")]
    [SerializeField] private string[] cutsceneSceneNames = { "Cutscene", "Cutscene2" };
    [SerializeField] private bool destroyWhenLeavingAllCutsceneScenes = false;

    // Dictionary to store special effect sounds by name
    private Dictionary<string, CutsceneSound> specialEffectsDict = new Dictionary<string, CutsceneSound>();

    // Audio source references
    private AudioSource musicSource;
    private AudioSource ambienceSource;
    private AudioSource sfxSource;

    // Sound pool for interface sounds
    private List<AudioSource> soundPool = new List<AudioSource>();
    private int poolSize = 5; // Increased pool size for more simultaneous sounds
    private int currentPoolIndex = 0;

    // Scene tracking
    private string currentSceneName;

    // Fade tracking
    private bool isMusicFading = false;
    private bool isAmbienceFading = false;

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
        musicSource.volume = 0f; // Start at 0 for fade in

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.loop = true;
        ambienceSource.playOnAwake = false;
        ambienceSource.volume = 0f; // Start at 0 for fade in

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // Create sound pool for interface sounds
        for (int i = 0; i < poolSize; i++)
        {
            GameObject poolObj = new GameObject($"SoundPool_{i}");
            poolObj.transform.parent = this.transform;
            AudioSource source = poolObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            soundPool.Add(source);
        }

        // Store initial scene name
        currentSceneName = SceneManager.GetActiveScene().name;

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Set up special effects dictionary
        InitializeSpecialEffects();
    }

    /// <summary>
    /// Initialize the special effects sounds dictionary
    /// </summary>
    private void InitializeSpecialEffects()
    {
        // Add explosion sound if set
        if (explosionSound.clip != null)
        {
            specialEffectsDict["Explosion"] = explosionSound;
        }

        // Add other special effects from array
        if (specialEffectSounds != null)
        {
            foreach (var sound in specialEffectSounds)
            {
                if (sound.clip != null && !string.IsNullOrEmpty(sound.name))
                {
                    specialEffectsDict[sound.name] = sound;
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (IsInAnyCutsceneScene())
        {
            // Start background music if configured
            if (playMusicOnStart && backgroundMusic.clip != null)
            {
                PlayBackgroundMusic(true); // Play with fade in
            }

            // Start ambient sound if configured
            if (playAmbienceOnStart && spaceAmbience.clip != null)
            {
                PlaySpaceAmbience(true); // Play with fade in
            }

            // Automatically set up all buttons in the scene
            if (autoSetupAllButtons)
            {
                SetupAllButtons();
            }
        }
    }

    private void Update()
    {
        // Check if we're in a cutscene scene
        if (!IsInAnyCutsceneScene())
        {
            // Stop cutscene music if we've left all cutscene scenes and it's still playing
            if (musicSource.isPlaying && !isMusicFading)
            {
                StopBackgroundMusic(true); // Stop with fade out
            }

            // Stop ambient sound if we've left all cutscene scenes and it's still playing
            if (ambienceSource.isPlaying && !isAmbienceFading)
            {
                StopSpaceAmbience(true); // Stop with fade out
            }

            // Optionally destroy this manager if configured to do so
            // Only if we're not in the middle of fades
            if (destroyWhenLeavingAllCutsceneScenes && !isMusicFading && !isAmbienceFading)
            {
                // Wait to destroy until fade out is complete
                if (!musicSource.isPlaying && !ambienceSource.isPlaying)
                {
                    Destroy(gameObject);
                }
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

        // Check if we've entered a cutscene scene
        if (IsInAnyCutsceneScene())
        {
            // Restart cutscene music if it's not playing
            if (playMusicOnStart && backgroundMusic.clip != null && !musicSource.isPlaying)
            {
                PlayBackgroundMusic(true); // Play with fade in
            }

            // Restart space ambience if it's not playing
            if (playAmbienceOnStart && spaceAmbience.clip != null && !ambienceSource.isPlaying)
            {
                PlaySpaceAmbience(true); // Play with fade in
            }

            // Set up buttons in the new scene
            if (autoSetupAllButtons)
            {
                SetupAllButtons();
            }
        }
        else
        {
            // Stop cutscene music if we've left all cutscene scenes
            if (musicSource.isPlaying && !isMusicFading)
            {
                StopBackgroundMusic(true); // Stop with fade out
            }

            // Stop ambient sound if we've left all cutscene scenes
            if (ambienceSource.isPlaying && !isAmbienceFading)
            {
                StopSpaceAmbience(true); // Stop with fade out
            }
        }
    }

    /// <summary>
    /// Checks if we're currently in any of the cutscene scenes
    /// </summary>
    private bool IsInAnyCutsceneScene()
    {
        if (cutsceneSceneNames == null || cutsceneSceneNames.Length == 0)
        {
            return false;
        }

        foreach (string sceneName in cutsceneSceneNames)
        {
            if (currentSceneName == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    #region Button Sounds

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
        Debug.Log($"Cutscene Audio Manager: Set up {allButtons.Length} buttons with click sounds");
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
    /// Plays the button click sound using sound pooling
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (buttonClickSound.clip == null) return;

        // Use sound pooling to prevent sound overlap issues
        AudioSource source = soundPool[currentPoolIndex];
        source.clip = buttonClickSound.clip;
        source.volume = buttonClickSound.volume;
        source.pitch = buttonClickSound.pitch;
        source.Play();

        // Move to next pool item
        currentPoolIndex = (currentPoolIndex + 1) % poolSize;
    }

    /// <summary>
    /// Plays a sound to indicate dialogue advancing
    /// </summary>
    public void PlayDialogueAdvanceSound()
    {
        if (dialogueAdvanceSound.clip == null) return;

        // Use sound pooling to prevent sound overlap issues
        AudioSource source = soundPool[currentPoolIndex];
        source.clip = dialogueAdvanceSound.clip;
        source.volume = dialogueAdvanceSound.volume;
        source.pitch = dialogueAdvanceSound.pitch;
        source.Play();

        // Move to next pool item
        currentPoolIndex = (currentPoolIndex + 1) % poolSize;
    }

    #endregion

    #region Special Effects

    /// <summary>
    /// Plays the explosion sound
    /// </summary>
    public void PlayExplosionSound()
    {
        if (explosionSound.clip == null) return;

        // Use sound pooling to prevent sound overlap issues
        AudioSource source = soundPool[currentPoolIndex];
        source.clip = explosionSound.clip;
        source.volume = explosionSound.volume;
        source.pitch = explosionSound.pitch;
        source.Play();

        // Temporarily reduce music volume for explosion impact
        if (musicSource.isPlaying)
        {
            StartCoroutine(TemporarilyReduceMusic(0.3f, 1.5f));
        }

        // Move to next pool item
        currentPoolIndex = (currentPoolIndex + 1) % poolSize;

        Debug.Log("Playing explosion sound");
    }

    /// <summary>
    /// Plays a special effect sound by name
    /// </summary>
    public void PlaySpecialEffectSound(string effectName)
    {
        if (string.IsNullOrEmpty(effectName) || !specialEffectsDict.ContainsKey(effectName))
        {
            Debug.LogWarning($"Special effect sound '{effectName}' not found");
            return;
        }

        CutsceneSound sound = specialEffectsDict[effectName];

        // Use sound pooling to prevent sound overlap issues
        AudioSource source = soundPool[currentPoolIndex];
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.Play();

        // Temporarily reduce music volume for major effects
        if (sound.volume > 0.7f && musicSource.isPlaying)
        {
            StartCoroutine(TemporarilyReduceMusic(0.5f, 0.8f));
        }

        // Move to next pool item
        currentPoolIndex = (currentPoolIndex + 1) % poolSize;

        Debug.Log($"Playing special effect sound: {effectName}");
    }

    /// <summary>
    /// Coroutine to temporarily reduce music volume during loud sound effects
    /// </summary>
    private System.Collections.IEnumerator TemporarilyReduceMusic(float reductionFactor, float duration)
    {
        float originalVolume = musicSource.volume;
        float reducedVolume = originalVolume * reductionFactor;

        // Quickly reduce the volume
        float fadeTime = 0.1f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeTime;
            musicSource.volume = Mathf.Lerp(originalVolume, reducedVolume, normalizedTime);
            yield return null;
        }

        // Hold the reduced volume
        yield return new WaitForSeconds(duration - fadeTime * 2);

        // Restore the volume
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeTime;
            musicSource.volume = Mathf.Lerp(reducedVolume, originalVolume, normalizedTime);
            yield return null;
        }

        musicSource.volume = originalVolume;
    }

    #endregion

    #region Background Music

    /// <summary>
    /// Changes the background music for a new cutscene
    /// </summary>
    public void ChangeBackgroundMusic(AudioClip newMusic, bool useCrossfade = true, float fadeDuration = 2f)
    {
        if (newMusic == null) return;

        // Update the background music reference
        CutsceneSound newSound = new CutsceneSound();
        newSound.clip = newMusic;
        newSound.volume = backgroundMusic.volume;
        newSound.pitch = backgroundMusic.pitch;
        backgroundMusic = newSound;

        // Handle the music change
        if (useCrossfade && musicSource.isPlaying)
        {
            // Crossfade to the new music
            GameObject tempObj = new GameObject("TempMusicSource");
            tempObj.transform.parent = transform;
            AudioSource tempSource = tempObj.AddComponent<AudioSource>();

            // Set up the temporary source
            tempSource.clip = newMusic;
            tempSource.loop = true;
            tempSource.volume = 0f;
            tempSource.Play();

            // Crossfade between the two sources
            StartCoroutine(CrossfadeMusic(musicSource, tempSource, fadeDuration));
        }
        else
        {
            // Just stop and start the new music
            musicSource.Stop();
            musicSource.clip = newMusic;
            musicSource.volume = backgroundMusic.volume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Plays the background music with optional fade in
    /// </summary>
    public void PlayBackgroundMusic(bool fadeIn = false)
    {
        if (backgroundMusic.clip == null) return;

        musicSource.clip = backgroundMusic.clip;
        musicSource.pitch = backgroundMusic.pitch;

        if (fadeIn)
        {
            musicSource.volume = 0f;
            musicSource.Play();
            StartCoroutine(FadeMusicVolume(0f, backgroundMusic.volume, fadeInDuration));
        }
        else
        {
            musicSource.volume = backgroundMusic.volume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stops the background music with optional fade out
    /// </summary>
    public void StopBackgroundMusic(bool fadeOut = false)
    {
        if (fadeOut && musicSource.isPlaying)
        {
            StartCoroutine(FadeMusicVolume(musicSource.volume, 0f, fadeOutDuration));
        }
        else
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Coroutine to fade music volume from start to end value
    /// </summary>
    private System.Collections.IEnumerator FadeMusicVolume(float startVolume, float endVolume, float duration)
    {
        isMusicFading = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            musicSource.volume = Mathf.Lerp(startVolume, endVolume, normalizedTime);
            yield return null;
        }

        musicSource.volume = endVolume;

        // If we faded out, stop the music
        if (endVolume <= 0f)
        {
            musicSource.Stop();
        }

        isMusicFading = false;
    }

    /// <summary>
    /// Coroutine to crossfade between two audio sources
    /// </summary>
    private System.Collections.IEnumerator CrossfadeMusic(AudioSource oldSource, AudioSource newSource, float duration)
    {
        isMusicFading = true;
        float elapsed = 0f;
        float startVolume = oldSource.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;

            oldSource.volume = Mathf.Lerp(startVolume, 0f, normalizedTime);
            newSource.volume = Mathf.Lerp(0f, backgroundMusic.volume, normalizedTime);

            yield return null;
        }

        // Ensure end values are set correctly
        oldSource.Stop();
        newSource.volume = backgroundMusic.volume;

        // Replace the old music source with the new one
        Destroy(oldSource.gameObject);
        musicSource = newSource;

        isMusicFading = false;
    }

    #endregion

    #region Space Ambience

    /// <summary>
    /// Plays the space ambience with optional fade in
    /// </summary>
    public void PlaySpaceAmbience(bool fadeIn = false)
    {
        if (spaceAmbience.clip == null) return;

        ambienceSource.clip = spaceAmbience.clip;
        ambienceSource.pitch = spaceAmbience.pitch;

        if (fadeIn)
        {
            ambienceSource.volume = 0f;
            ambienceSource.Play();
            StartCoroutine(FadeAmbienceVolume(0f, spaceAmbience.volume, ambienceFadeInDuration));
        }
        else
        {
            ambienceSource.volume = spaceAmbience.volume;
            ambienceSource.Play();
        }

        Debug.Log("Playing space ambience");
    }

    /// <summary>
    /// Stops the space ambience with optional fade out
    /// </summary>
    public void StopSpaceAmbience(bool fadeOut = false)
    {
        if (fadeOut && ambienceSource.isPlaying)
        {
            StartCoroutine(FadeAmbienceVolume(ambienceSource.volume, 0f, ambienceFadeOutDuration));
        }
        else
        {
            ambienceSource.Stop();
        }
    }

    /// <summary>
    /// Changes the space ambience to a new clip with optional crossfade
    /// </summary>
    public void ChangeSpaceAmbience(AudioClip newAmbience, bool useCrossfade = true, float fadeDuration = 3f)
    {
        if (newAmbience == null) return;

        // Update the space ambience reference
        CutsceneSound newSound = new CutsceneSound();
        newSound.clip = newAmbience;
        newSound.volume = spaceAmbience.volume;
        newSound.pitch = spaceAmbience.pitch;
        spaceAmbience = newSound;

        // Handle the ambience change
        if (useCrossfade && ambienceSource.isPlaying)
        {
            // Crossfade to the new ambience
            GameObject tempObj = new GameObject("TempAmbienceSource");
            tempObj.transform.parent = transform;
            AudioSource tempSource = tempObj.AddComponent<AudioSource>();

            // Set up the temporary source
            tempSource.clip = newAmbience;
            tempSource.loop = true;
            tempSource.volume = 0f;
            tempSource.Play();

            // Crossfade between the two sources
            StartCoroutine(CrossfadeAmbience(ambienceSource, tempSource, fadeDuration));
        }
        else
        {
            // Just stop and start the new ambience
            ambienceSource.Stop();
            ambienceSource.clip = newAmbience;
            ambienceSource.volume = spaceAmbience.volume;
            ambienceSource.Play();
        }

        Debug.Log("Changed space ambience");
    }

    /// <summary>
    /// Coroutine to fade ambience volume from start to end value
    /// </summary>
    private System.Collections.IEnumerator FadeAmbienceVolume(float startVolume, float endVolume, float duration)
    {
        isAmbienceFading = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            ambienceSource.volume = Mathf.Lerp(startVolume, endVolume, normalizedTime);
            yield return null;
        }

        ambienceSource.volume = endVolume;

        // If we faded out, stop the ambience
        if (endVolume <= 0f)
        {
            ambienceSource.Stop();
        }

        isAmbienceFading = false;
    }

    /// <summary>
    /// Coroutine to crossfade between two ambience audio sources
    /// </summary>
    private System.Collections.IEnumerator CrossfadeAmbience(AudioSource oldSource, AudioSource newSource, float duration)
    {
        isAmbienceFading = true;
        float elapsed = 0f;
        float startVolume = oldSource.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;

            oldSource.volume = Mathf.Lerp(startVolume, 0f, normalizedTime);
            newSource.volume = Mathf.Lerp(0f, spaceAmbience.volume, normalizedTime);

            yield return null;
        }

        // Ensure end values are set correctly
        oldSource.Stop();
        newSource.volume = spaceAmbience.volume;

        // Replace the old ambience source with the new one
        Destroy(oldSource.gameObject);
        ambienceSource = newSource;

        isAmbienceFading = false;
    }

    /// <summary>
    /// Temporarily mutes the space ambience during important dialogue or events
    /// </summary>
    public void MuteAmbienceTemporarily(float duration)
    {
        if (!ambienceSource.isPlaying) return;

        StartCoroutine(TemporarilyMuteAmbience(duration));
    }

    /// <summary>
    /// Coroutine to temporarily mute the ambience and then restore it
    /// </summary>
    private System.Collections.IEnumerator TemporarilyMuteAmbience(float duration)
    {
        float originalVolume = ambienceSource.volume;

        // Quickly mute the ambience
        float fadeTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeTime;
            ambienceSource.volume = Mathf.Lerp(originalVolume, 0f, normalizedTime);
            yield return null;
        }

        // Hold the mute
        yield return new WaitForSeconds(duration);

        // Restore the ambience
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeTime;
            ambienceSource.volume = Mathf.Lerp(0f, originalVolume, normalizedTime);
            yield return null;
        }

        ambienceSource.volume = originalVolume;
    }

    #endregion

    #region Volume Controls

    /// <summary>
    /// Sets the volume of the background music
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        backgroundMusic.volume = Mathf.Clamp01(volume);
        if (musicSource.isPlaying)
        {
            musicSource.volume = backgroundMusic.volume;
        }
    }

    /// <summary>
    /// Sets the volume of the space ambience
    /// </summary>
    public void SetAmbienceVolume(float volume)
    {
        spaceAmbience.volume = Mathf.Clamp01(volume);
        if (ambienceSource.isPlaying)
        {
            ambienceSource.volume = spaceAmbience.volume;
        }
    }

    /// <summary>
    /// Sets the volume for interface sounds
    /// </summary>
    public void SetInterfaceVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        buttonClickSound.volume = volume;
        dialogueAdvanceSound.volume = volume;
    }

    /// <summary>
    /// Sets the volume for special effects
    /// </summary>
    public void SetSpecialEffectsVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        explosionSound.volume = volume;

        foreach (var sound in specialEffectSounds)
        {
            sound.volume = volume;
        }
    }

    #endregion
}
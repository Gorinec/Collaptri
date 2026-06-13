using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    public AudioClip blockLandSound;     // Звук приземления блока
    public AudioClip lineClearSound;     // Звук удаления линии
    public AudioClip gameOverSound;      // Звук Game Over
    public AudioClip rotateSound;        // Звук поворота
    
    

    [Header("Music")]
    public AudioClip backgroundMusic;    // Фоновая музыка

    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private bool musicEnabled = true;
    private bool effectsEnabled = true;

    void Awake()
    {
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

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        // Загружаем настройки
        LoadSettings();

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
    }

    void Start()
    {
        // Автоматически запускаем музыку при старте
        if (musicEnabled)
        {
            PlayMusic();
        }
    }

    void LoadSettings()
    {
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        effectsEnabled = PlayerPrefs.GetInt("EffectsEnabled", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("EffectsEnabled", effectsEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void PlaySound(AudioClip clip)
    {
        if (!effectsEnabled) return; // Если эффекты выключены
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayMusic()
    {
        if (!musicEnabled) return; // Если музыка выключена
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void ToggleMusic(bool isOn)
    {
        musicEnabled = isOn;
        if (musicEnabled)
        {
            PlayMusic();
        }
        else
        {
            StopMusic();
        }
        SaveSettings();
    }

    public void ToggleEffects(bool isOn)
    {
        effectsEnabled = isOn;
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        SaveSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        SaveSettings();
    }

    public bool IsMusicEnabled()
    {
        return musicEnabled;
    }

    public bool IsEffectsEnabled()
    {
        return effectsEnabled;
    }
}
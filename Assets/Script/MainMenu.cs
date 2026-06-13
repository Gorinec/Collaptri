using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Toggle musicToggle;
    public Toggle effectsToggle;
    public Button closeSettingsButton;

    [Header("Volume Settings")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        // Загружаем сохраненные настройки
        LoadSettings();

        // Привязываем кнопки
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        // Привязываем тогглы
        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(OnMusicToggle);

        if (effectsToggle != null)
            effectsToggle.onValueChanged.AddListener(OnEffectsToggle);

        // Привязываем слайдеры
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Скрываем панель настроек
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void StartGame()
    {
        SceneManager.LoadScene("MainScene"); // Название твоей игровой сцены
    }

    void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        SaveSettings();
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnMusicToggle(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            if (!isOn)
                AudioManager.Instance.StopMusic();
            else
                AudioManager.Instance.PlayMusic();
        }
    }

    void OnEffectsToggle(bool isOn)
    {
        // Сохраняем настройку, применяем при воспроизведении звуков
        PlayerPrefs.SetInt("EffectsEnabled", isOn ? 1 : 0);
    }

    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    void LoadSettings()
    {
        // Загружаем сохраненные настройки
        bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool effectsOn = PlayerPrefs.GetInt("EffectsEnabled", 1) == 1;
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

        if (musicToggle != null) musicToggle.isOn = musicOn;
        if (effectsToggle != null) effectsToggle.isOn = effectsOn;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol;
    }

    void SaveSettings()
    {
        if (musicToggle != null)
            PlayerPrefs.SetInt("MusicEnabled", musicToggle.isOn ? 1 : 0);
        if (effectsToggle != null)
            PlayerPrefs.SetInt("EffectsEnabled", effectsToggle.isOn ? 1 : 0);
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

        PlayerPrefs.Save();
    }
}
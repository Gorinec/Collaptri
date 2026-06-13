using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem; // ← Добавляем для новой Input System

public class PauseController : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject pauseButton;
    public GameObject menuButton;

    private bool isPaused = false;

    void Start()
    {
        // Привязываем кнопки
        if (pauseButton != null)
        {
            Button btn = pauseButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(TogglePause);
        }

        if (menuButton != null)
        {
            Button btn = menuButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(GoToMenu);
        }
    }

    // Используем новую Input System
    void OnPause() // Этот метод будет вызываться из Input Actions
    {
        TogglePause();
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            Debug.Log("Game Paused");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("Game Resumed");
        }
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
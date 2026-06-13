using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;  // ← Изменено с linesText на bestScoreText
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button menuButton;

    [Header("UI Elements to Hide")]
    public GameObject[] uiElementsToHide;

    private int currentScore = 0;
    private int bestScore = 0;  // ← Вместо totalLines

    void Start()
    {
        ShowUIElements();

        // Загружаем лучший результат
        LoadBestScore();

        UpdateUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);
    }

    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    void SaveBestScore()
    {
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
            UpdateUI();
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateUI();
        SaveBestScore();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"{currentScore}";

        if (bestScoreText != null)
            bestScoreText.text = $" {bestScore}";
    }

    public void ShowGameOver()
    {
        HideUIElements();
        StartCoroutine(DestroyTetrominosWithDelay());

        if (gameOverPanel != null)
        {
            // Показываем финальный счет и лучший результат
            TextMeshProUGUI finalScoreText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Score: {currentScore}\nBest: {bestScore}";
            }

            gameOverPanel.SetActive(true);
            StartCoroutine(ShowGameOverAnimation());
        }

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowInterstitialIfReady();
        }
    }

    private void DestroyAllTetrominos()
    {
        GameObject[] tetrominos = GameObject.FindGameObjectsWithTag("Tetromino");
        Debug.Log($"Найдено Tetromino по тегу: {tetrominos.Length}");

        foreach (GameObject tetro in tetrominos)
        {
            if (tetro != null)
            {
                Destroy(tetro);
            }
        }
    }

    System.Collections.IEnumerator DestroyTetrominosWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        DestroyAllTetrominos();
    }

    private void HideUIElements()
    {
        if (uiElementsToHide != null)
        {
            foreach (GameObject obj in uiElementsToHide)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    public void RestartGame()
    {
        currentScore = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowUIElements()
    {
        if (uiElementsToHide != null)
        {
            foreach (GameObject obj in uiElementsToHide)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    System.Collections.IEnumerator ShowGameOverAnimation()
    {
        gameOverPanel.transform.localScale = Vector3.zero;
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            gameOverPanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        gameOverPanel.transform.localScale = Vector3.one;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;
    public Button skipButton;

    public string[] tutorialSteps;

    private int currentStep = 0;

    void Start()
    {
        // ВРЕМЕННО: всегда показываем обучение
        ShowTutorial();

        if (nextButton != null)
            nextButton.onClick.AddListener(NextStep);
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipTutorial);
    }

    void ShowTutorial()
    {
        Debug.Log("ShowTutorial called");

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            Debug.Log("TutorialPanel activated");
        }
        else
        {
            Debug.LogError("TutorialPanel is NULL!");
        }

        if (tutorialText != null && tutorialSteps.Length > 0)
        {
            tutorialText.text = tutorialSteps[0];
            Debug.Log("Text set to: " + tutorialSteps[0]);
        }

        // Останавливаем игру
        Time.timeScale = 0f;

        // Отключаем спавнер
        Spawner spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null)
            spawner.enabled = false;
    }

    void NextStep()
    {
        currentStep++;

        if (currentStep < tutorialSteps.Length)
        {
            tutorialText.text = tutorialSteps[currentStep];
        }
        else
        {
            CompleteTutorial();
        }
    }

    void SkipTutorial()
    {
        CompleteTutorial();
    }

    void CompleteTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Time.timeScale = 1f;

        Spawner spawner = FindFirstObjectByType<Spawner>();
        if (spawner != null)
            spawner.enabled = true;
    }
}
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance { get; private set; }

    [Header("Unity Ads Settings")]
    [SerializeField] private string androidGameId = "6078135";
    [SerializeField] private string interstitialPlacementId = "Interstitial_Android";
    [SerializeField] private bool testMode = false;

    private int gameCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAds();
        LoadGameCount();
    }

    private void InitializeAds()
    {
        if (Advertisement.isSupported)
        {
            Advertisement.Initialize(androidGameId, testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads Initialization Complete.");
        LoadInterstitialAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error} - {message}");
    }

    private void LoadInterstitialAd()
    {
        Advertisement.Load(interstitialPlacementId, this);
    }

    public void ShowInterstitialIfReady()
    {
        gameCount++;
        SaveGameCount();

        // Показываем рекламу каждую 2-ю игру (2, 4, 6, 8...)
        if (gameCount % 2 == 0)
        {
            Debug.Log($"Showing Ad (Game {gameCount})");
            Advertisement.Show(interstitialPlacementId, this);
        }
        else
        {
            Debug.Log($"Skipping Ad (Game {gameCount}). Next ad in 1 game.");
        }
    }

    private void LoadGameCount()
    {
        gameCount = PlayerPrefs.GetInt("AdGameCount", 0);
    }

    private void SaveGameCount()
    {
        PlayerPrefs.SetInt("AdGameCount", gameCount);
        PlayerPrefs.Save();
    }

    // --- Load Listener ---
    public void OnUnityAdsAdLoaded(string placementId) { Debug.Log($"Ad Loaded: {placementId}"); }
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Ad Failed to Load: {placementId} - {error} - {message}");
        // Пробуем загрузить снова через 5 секунд
        Invoke(nameof(LoadInterstitialAd), 5f);
    }

    // --- Show Listener ---
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Ad Show Failed: {message}");
        LoadInterstitialAd();
    }

    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Ad Show Complete");
        LoadInterstitialAd(); // Загружаем следующую рекламу
    }
}
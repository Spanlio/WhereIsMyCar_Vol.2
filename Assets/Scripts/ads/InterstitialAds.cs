using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{

    [SerializeField] private string _androidAdUnitId = "BP_Interstitial_Android";
    String _adUnitId;
    public event Action OnInterstitialAdLoaded;
    public bool isReady = false;
    [SerializeField] Button _interstitialAdButton;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    // Update is called once per frame
    void Update()
    {
        if (_interstitialAdButton != null && AdManager.Instance != null && AdManager.Instance.interstitialAd != null)
        {
            _interstitialAdButton.interactable = AdManager.Instance.interstitialAd.isReady;
        }
    }

    public void OnInterstitialButtonClicked()
    {
        Debug.Log("Interstitial Ad Button Clicked");
        ShowInterstitial();
    }

    public void ShowInterstitial()
    {
        if (AdManager.Instance.interstitialAd != null &&
         AdManager.Instance.interstitialAd.isReady)
        {
            Debug.Log("Showing Interstitial Ad manually");
            ShowAd();
        }
        else
        {
            Debug.Log("Interstitial Ad not ready yet, loading again");
            LoadAd();
        }
    }

    public void ShowAd()
    {
        if (isReady)
        {
            //Papildināsim priekš banner ad

            Advertisement.Show(_adUnitId, this);
            isReady = false;
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready yet!");
        }
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load interstitial ad before Unity Ads was initialized!");
            return;
        }
        else
        {
            Debug.Log("Loading Interstitial Ad: " + _adUnitId);
            Advertisement.Load(_adUnitId, this);
        }
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Interstitial ad loaded successfully!");
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = true;
        isReady = true;
        OnInterstitialAdLoaded?.Invoke();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load interstitial ad: {error.ToString()} - {message}");
        StartCoroutine(WaitAndLoadAd(10f));
    }

    private IEnumerator WaitAndLoadAd(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Failed to show interstitial ad: {error.ToString()} - {message}");
        LoadAd();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Interstitial ad started showing!");
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("Interstitial ad clicked!");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Interstitial ad completed successfully!");
            StartCoroutine(SlowDownTimeTemporarily(30f));
            LoadAd();
        }
        else
        {
            Debug.LogWarning($"Interstitial ad was not completed: {showCompletionState.ToString()}");
            Time.timeScale = 1f;
            LoadAd();
        }
    }

    // Balva par noskatītu reklāmu - 30 sekundes laiks iet lēnāk
    private IEnumerator SlowDownTimeTemporarily(float duration)
    {
        Time.timeScale = 0.3f;
        Debug.Log("Time slowed down for " + duration + " seconds.");
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        Debug.Log("Time restored to normal.");
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnInterstitialButtonClicked);
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = false;
    }
}

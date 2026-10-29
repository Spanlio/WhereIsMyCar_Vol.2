using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "BP_Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;

    // Each scene decides what the reward is: CityScene clears the flying
    // objects, towerScene halves the timer.
    public event Action OnRewardEarned;
    public bool IsReady { get; private set; }


    private void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("tried to load rewarded ad before Unity Ads was initialized");
            return;
        }

        Debug.Log("Loading Rewareded Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Rewared ad loaded: " + placementId);
        if(placementId.Equals(_adUnitId))
        {
            IsReady = true;
            if (_rewardedAdButton != null)
                _rewardedAdButton.interactable = true;
        }

    }

    public void ShowAd()
    {
        if (!IsReady)
        {
            Debug.Log("Rewarded ad is not ready yet");
            return;
        }

        IsReady = false;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;
        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading rewarded ad: {placementId} : {error.ToString()} - {message}");
        StartCoroutine(WaitAndLoadAd(10f));
    }
    private IEnumerator WaitAndLoadAd(float delay)
    {
        // Realtime, because ads and slow motion change Time.timeScale.
        yield return new WaitForSecondsRealtime(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing rewarded ad: {placementId} : {error.ToString()} - {message}");
        Time.timeScale = 1f;
        StartCoroutine(WaitAndLoadAd(10f));
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad: " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId,
    UnityAdsShowCompletionState showCompletionState)
    {
        Time.timeScale = 1f;

        if (!placementId.Equals(_adUnitId))
            return;

        if (showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Rewared ad completed");
            OnRewardEarned?.Invoke();
            StartCoroutine(WaitAndLoadAd(15f));
        }
        else
        {
            // A skipped ad gives no reward, but it's used up: load the next one.
            LoadAd();
        }
    }

    public void SetButton(Button button)
    {
        if(button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = IsReady;
    }
}

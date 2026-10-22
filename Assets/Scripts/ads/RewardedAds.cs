using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "BP_Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flyingObjectManager;


    private void Awake()
    {
        _adUnitId = _androidAdUnitId;
        if(flyingObjectManager == null)
        {
            flyingObjectManager = 
            FindFirstObjectByType<FlyingObjectManager>();
        }
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
            _rewardedAdButton.interactable = true;
        }

    }

    public void ShowAd()
    {
        //velak jaslepj baner ad
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
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing rewarded ad: {placementId} : {error.ToString()} - {message}");
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
        if (placementId.Equals(_adUnitId) &&
            showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Rewared ad completed");
            flyingObjectManager.DestroyAllFlyingObjects();
            _rewardedAdButton.interactable = false;
            StartCoroutine(WaitAndLoadAd(15f));
        }
        Time.timeScale = 1f;
    }

    public void SetButton(Button button)
    {
        if(button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = false;
    }
}

using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] private string _androidGameId = "800388717";
    [SerializeField] bool _testMode = true;
    private string _gameId;
    public event Action OnAdsInitialized;

    // Start (not Awake), so AdManager has subscribed to OnAdsInitialized first.
    void Start()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        _gameId = _androidGameId;
#endif
        if (Advertisement.isInitialized)
        {
            OnAdsInitialized?.Invoke();
        }
        else if (Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        OnAdsInitialized?.Invoke();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

}

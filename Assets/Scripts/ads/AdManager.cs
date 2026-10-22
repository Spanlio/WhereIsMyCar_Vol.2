using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAds interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    private bool firstAdShown = false;

    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;

    public static AdManager Instance { get; private set; }

    private bool firstSceneLoaded = false;

    void Awake()
    {
        if (adsInitializer == null)
        {
            adsInitializer = FindFirstObjectByType<AdsInitializer>();
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAds)
        {
            // Noņemam, lai pēc scēnas pārlādes nepiesakās divreiz
            interstitialAd.OnInterstitialAdLoaded -= HandleInterstitialReady;
            interstitialAd.OnInterstitialAdLoaded += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds)
        {
            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAds)
        {
            bannerAd.LoadBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing interstitial ad for the first time!");
            interstitialAd.ShowAd();
            firstAdShown = true;
        }
        else
        {
            Debug.Log("Interstitial ad is ready to be shown again!");
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAds>();

        Button interstitialButton = GameObject.FindGameObjectWithTag("InterstitialAdButton").GetComponent<Button>();
        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
        }

        if(rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        Button rewardedAdButton = 
        GameObject.FindGameObjectWithTag("RewardedAdButton").GetComponent<Button>();
        if(rewardedAds != null && rewardedAdButton != null)
        {
            rewardedAds.SetButton(rewardedAdButton);
        }

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        GameObject bannerButtonObject = GameObject.FindGameObjectWithTag("BannerAdButton");
        if (bannerAd != null && bannerButtonObject != null)
        {
            bannerAd.SetButton(bannerButtonObject.GetComponent<Button>());
        }

        if (!firstSceneLoaded)
        {
            firstSceneLoaded = true;
            Debug.Log("First scene loaded!");
            return;
        }

        Debug.Log("Scene reloaded!");
        HandleAdsInitialized();
    }
}

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

        // StartScene has no ad buttons, so every tag lookup can return null.
        GameObject interstitialButtonObject = GameObject.FindGameObjectWithTag("InterstitialAdButton");
        if (interstitialAd != null && interstitialButtonObject != null)
        {
            interstitialAd.SetButton(interstitialButtonObject.GetComponent<Button>());
        }

        if(rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        GameObject rewardedButtonObject = GameObject.FindGameObjectWithTag("RewardedAdButton");
        if(rewardedAds != null && rewardedButtonObject != null)
        {
            rewardedAds.SetButton(rewardedButtonObject.GetComponent<Button>());
        }

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        GameObject bannerButtonObject = GameObject.FindGameObjectWithTag("BannerAdButton");
        if (bannerAd != null && bannerButtonObject != null)
        {
            bannerAd.SetButton(bannerButtonObject.GetComponent<Button>());
        }
        else if (bannerAd != null)
        {
            // No banner button here (main menu): don't leave the banner on screen.
            bannerAd.HideBannerAd();
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

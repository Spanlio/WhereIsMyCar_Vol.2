using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Lives for the whole app. It's created automatically before the first scene
// loads, so every scene (menu included) gets ads without any setup in the scene.
public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAds interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;

    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;

    public static AdManager Instance { get; private set; }

    // Set on every scene load, the interstitial shows as soon as it's ready.
    private bool interstitialPending = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateAdManager()
    {
        if (Instance != null)
            return;

        GameObject adsObject = new GameObject("AdManager");
        adsObject.AddComponent<AdsInitializer>();
        adsObject.AddComponent<InterstitialAds>();
        adsObject.AddComponent<RewardedAds>();
        adsObject.AddComponent<BannerAd>();
        adsObject.AddComponent<AdManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        adsInitializer = GetComponent<AdsInitializer>();
        interstitialAd = GetComponent<InterstitialAds>();
        rewardedAds = GetComponent<RewardedAds>();
        bannerAd = GetComponent<BannerAd>();

        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
        interstitialAd.OnInterstitialAdLoaded += TryShowInterstitial;
    }

    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAds)
            interstitialAd.LoadAd();

        if (!turnOffRewardedAds)
            rewardedAds.LoadAd();

        if (!turnOffBannerAds)
            bannerAd.LoadBanner();
    }

    // Shows the interstitial if a scene load is waiting for one and it's loaded.
    private void TryShowInterstitial()
    {
        if (!interstitialPending || !interstitialAd.isReady)
            return;

        Debug.Log("Showing interstitial ad for the scene load");
        interstitialPending = false;
        interstitialAd.ShowAd();
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
        // Slow motion from an interstitial in CityScene must not follow the player.
        interstitialAd.StopSlowMotion();

        // Manual ad buttons are optional: only CityScene has them.
        GameObject interstitialButtonObject = GameObject.FindGameObjectWithTag("InterstitialAdButton");
        if (interstitialButtonObject != null)
            interstitialAd.SetButton(interstitialButtonObject.GetComponent<Button>());

        GameObject rewardedButtonObject = GameObject.FindGameObjectWithTag("RewardedAdButton");
        if (rewardedButtonObject != null)
            rewardedAds.SetButton(rewardedButtonObject.GetComponent<Button>());

        // The banner is always on; this only re-shows it if something hid it.
        if (!turnOffBannerAds)
            bannerAd.ShowBannerAd();

        // "No ads" from the shop only turns off this automatic one.
        if (!turnOffInterstitialAds && !StarWallet.Owns(StarWallet.NO_ADS))
        {
            interstitialPending = true;
            TryShowInterstitial();
        }
    }
}

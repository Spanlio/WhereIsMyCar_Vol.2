using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

// The banner is always on: it shows as soon as it loads and has no hide button.
public class BannerAd : MonoBehaviour
{

    [SerializeField] string _androidAdUnitId = "BP_Banner_Android";
    string _adUnitId;
    public bool isBannerLoaded = false;
    public bool isBannerVisible = false;
    [SerializeField] BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;
    void Awake()
    {
        _adUnitId = _androidAdUnitId;
        isBannerLoaded = false;
        isBannerVisible = false;
        Advertisement.Banner.SetPosition(bannerPosition);
    }

    public void LoadBanner()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load banner ad before Unity ads was initialized");
            return;
        }

        BannerLoadOptions bannerOptions = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };
        Advertisement.Banner.Load(_adUnitId, bannerOptions);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded!");
        isBannerLoaded = true;
        ShowBannerAd();
    }

    void OnBannerError(string message)
    {
        Debug.Log($"Banner error: {message}");
        StartCoroutine(WaitAndLoadBanner(10f));
    }

    private IEnumerator WaitAndLoadBanner(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        LoadBanner();
    }

    public void ShowBannerAd()
    {
        if (!isBannerLoaded || isBannerVisible)
            return;

        BannerOptions bannerOptions = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };
        Advertisement.Banner.Show(_adUnitId, bannerOptions);
    }

    void OnBannerClicked()
    {
        Debug.Log("User clicked on the banner ad!");
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner ad hidden!" + _adUnitId);
        isBannerVisible = false;
    }

    void OnBannerShown()
    {
        Debug.Log("Banner ad visible!" + _adUnitId);
        isBannerVisible = true;
    }
}

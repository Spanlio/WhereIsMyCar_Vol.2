using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{

    [SerializeField] string _androidAdUnitId = "BP_Banner_Android";
    string _adUnitId;
    [SerializeField] Button _bannerAdButton;
    public bool isBannerVisible = false;
    [SerializeField] BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;
    void Awake()
    {
        _adUnitId = _androidAdUnitId;
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
        if (_bannerAdButton != null)
            _bannerAdButton.interactable = true;
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
        if (isBannerVisible)
        {
            HideBannerAd();
        }
        else
        {
            BannerOptions bannerOptions = new BannerOptions
            {
                clickCallback = OnBannerClicked,
                hideCallback = OnBannerHidden,
                showCallback = OnBannerShown
            };
            Advertisement.Banner.Show(_adUnitId, bannerOptions);
        }
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

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
        isBannerVisible = false;
    }

    public void SetButton(Button button)
    {
        if(button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowBannerAd);
        _bannerAdButton = button;
        _bannerAdButton.interactable = false;
    }
}

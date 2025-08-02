using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AdmobAdsScript : MonoBehaviour
{

    public static AdmobAdsScript instance;

    private BannerView _bannerView;
    private InterstitialAd interstitial;

    [Header("AdMob Ad Unit IDs")]
    [SerializeField] private string androidBannerAdUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";
    [SerializeField] private string androidInterstitialAdUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";
    [SerializeField] private string androidRewardedAdUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";

    [SerializeField] private string iosBannerAdUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";
    [SerializeField] private string iosInterstitialAdUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";
    [SerializeField] private string iosRewardedAdUnitId = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx";

    private string bannerAdUnitId;
    private string interstitialAdUnitId;
    private string rewardedAdUnitId;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
#if UNITY_ANDROID
        bannerAdUnitId = androidBannerAdUnitId;
        interstitialAdUnitId = androidInterstitialAdUnitId;
        rewardedAdUnitId = androidRewardedAdUnitId;
#elif UNITY_IPHONE
        bannerAdUnitId = iosBannerAdUnitId;
        interstitialAdUnitId = iosInterstitialAdUnitId;
        rewardedAdUnitId = iosRewardedAdUnitId;
#else
        bannerAdUnitId = "unexpected_platform";
        interstitialAdUnitId = "unexpected_platform";
        rewardedAdUnitId = "unexpected_platform";
#endif
    }


    private void Start()
    {
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }

            Debug.Log("Google Mobile Ads initialization complete.");
        });
        this.RequestBanner();
        this.RequestInterstitial();
        this.RequestRewarded();

    }
    public void RequestBanner()
    {
        Debug.Log("Creating banner view");
        if (_bannerView != null)
        {
            DestroyAd();
        }
        _bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Top);
    }

    public void DestroyAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner view");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    public void LoadAd()
    {
        if (_bannerView == null)
        {
            Debug.Log("Creating banner view");

            if (_bannerView != null)
            {
                DestroyAd();
            }
            _bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Top);
        }
        var adRequest = new AdRequest();
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }

    public void RequestInterstitial()
    {
        // Creates a 320x50 banner view at top of the screen.
        Debug.Log("Creating interstitial view");
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
        Debug.Log("Loading the interstitial ad.");
        // create our request used to load the ad.
        var adRequest = new AdRequest();
        // send the request to load the ad.
        InterstitialAd.Load(interstitialAdUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                Debug.LogError("interstitial ad failed to load an ad " +
                    "with error: " + error);
                return;
            }
            Debug.Log("Interstitial ad loaded with response : "
                + ad.GetResponseInfo());
            _interstitialAd = ad;
        });
    }

    private InterstitialAd _interstitialAd;

    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }

    public void RequestRewarded()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
        Debug.Log("Loading the rewarded ad.");
        // create our request used to load the ad.
        var adRequest = new AdRequest();
        // send the request to load the ad.
        RewardedAd.Load(interstitialAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                            + ad.GetResponseInfo());

                _rewardedAd = ad;
            });
    }

    RewardedAd _rewardedAd;

    public void LoadRewardedAd()
    {
        if(_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
        Debug.Log("Loading the reward ad");
        var adRequest = new AdRequest();
        RewardedAd.Load(rewardedAdUnitId, adRequest, 
            (RewardedAd ad, LoadAdError error) =>
            {
                // If the operation failed, an error is returned.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad "+
                        "with error : " + error);
                    return;
                }

                // If the operation completed successfully, no error is returned.
                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                _rewardedAd = ad;

            });
    }

    public void ShowRewardedAd()
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
            {
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
            });
        }
    }
}
using GoogleMobileAds.Api;
using System.Collections;
using UnityEngine;

public class AdsController : MonoBehaviour
{
    public enum BannerPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }

    [SerializeField] private bool loadBannerOnloadScene;
    [SerializeField] private bool loadInterstitialOnLoadScene;

    [Header("Banner Position")]
    [SerializeField] private BannerPosition bannerPosition = BannerPosition.Bottom;

    void Start()
    {
        if (loadBannerOnloadScene)
        {
            if (AdmobAdsScript.instance != null)
            {
                // Converte nossa enumeração para AdPosition do Google Ads
                AdPosition position = ConvertBannerPosition(bannerPosition);
                AdmobAdsScript.instance.RequestBanner(position);
                StartCoroutine(BannerLoad(position));
            }
        }

        if (loadInterstitialOnLoadScene)
        {
            if (AdmobAdsScript.instance != null)
            {
                AdmobAdsScript.instance.RequestInterstitial();
                StartCoroutine(InsterstitialLoad());
            }
        }
    }

    private AdPosition ConvertBannerPosition(BannerPosition position)
    {
        switch (position)
        {
            case BannerPosition.Top: return AdPosition.Top;
            case BannerPosition.Bottom: return AdPosition.Bottom;
            case BannerPosition.TopLeft: return AdPosition.TopLeft;
            case BannerPosition.TopRight: return AdPosition.TopRight;
            case BannerPosition.BottomLeft: return AdPosition.BottomLeft;
            case BannerPosition.BottomRight: return AdPosition.BottomRight;
            case BannerPosition.Center: return AdPosition.Center;
            default: return AdPosition.Bottom;
        }
    }

    IEnumerator BannerLoad(AdPosition position)
    {
        yield return new WaitForSeconds(2);
        AdmobAdsScript.instance.LoadAd(position);
    }

    IEnumerator InsterstitialLoad()
    {
        yield return new WaitForSeconds(2);
        AdmobAdsScript.instance.LoadInterstitialAd();
    }
}
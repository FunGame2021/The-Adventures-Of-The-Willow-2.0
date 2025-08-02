using System.Collections;
using UnityEngine;

public class AdsController : MonoBehaviour
{
    [SerializeField] private bool loadBannerOnloadScene;
    void Start()
    {
        if (loadBannerOnloadScene)
        {
            if(AdmobAdsScript.instance != null)
            {
                AdmobAdsScript.instance.RequestBanner();
                StartCoroutine(BannerLoad());
            }
        }
    }
    IEnumerator BannerLoad()
    {
        yield return new WaitForSeconds(5);
        AdmobAdsScript.instance.LoadAd();
    }
    void Update()
    {
        
    }
}

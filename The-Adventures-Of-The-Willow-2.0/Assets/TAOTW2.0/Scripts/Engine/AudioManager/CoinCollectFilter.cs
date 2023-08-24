using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class CoinCollectFilter : MonoBehaviour
{
    public static CoinCollectFilter coinCollectFilterInstance;
    public static float coinPitch = 0;

    [SerializeField] private float minTimeBetweenCoins = 1.0f;
    [SerializeField] private float timeSinceLastCoin;
    public EventInstance eventInstance;

    void Start()
    {
        if (coinCollectFilterInstance == null)
        {
            coinCollectFilterInstance = this;
        }

        coinPitch = 0;

        timeSinceLastCoin = minTimeBetweenCoins;
        eventInstance = RuntimeManager.CreateInstance(FMODEvents.instance.coinCollected);
    }

    public void SequenceCoinCollected()
    {
        coinPitch = Mathf.Clamp(coinPitch + 0.2f, 0f, 1f);
        timeSinceLastCoin = 0.0f;
    }

    void Update()
    {
        eventInstance.setParameterByName("CoinPitch", coinPitch);
        timeSinceLastCoin += Time.deltaTime;
        if (timeSinceLastCoin >= minTimeBetweenCoins)
        {
            coinPitch = 0;
        }
        if(coinPitch >= 1)
        {
            coinPitch = 0;
        }
    }
}



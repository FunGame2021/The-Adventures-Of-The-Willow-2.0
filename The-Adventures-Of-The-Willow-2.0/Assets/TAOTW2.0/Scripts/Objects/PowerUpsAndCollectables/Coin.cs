using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Coin : MonoBehaviour
{
    private bool collected = false;

    public int coinValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !collected)
        {
            
            CoinCollectFilter.coinCollectFilterInstance.eventInstance.start();
            CoinCollectFilter.coinCollectFilterInstance.eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            CoinCollectFilter.coinCollectFilterInstance.SequenceCoinCollected();
            CollectCoin();
            Destroy(gameObject);
        }
    }

    private void CollectCoin()
    {
        collected = true;
        CoinCollect.instance.ChangeCoin(coinValue);
    }
}

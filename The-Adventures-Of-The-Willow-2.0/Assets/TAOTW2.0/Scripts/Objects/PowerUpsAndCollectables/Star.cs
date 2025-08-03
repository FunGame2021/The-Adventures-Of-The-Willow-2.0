using FMODUnity;
using UnityEngine;

public class Star : MonoBehaviour
{
    private bool collected = false;

    public int starValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !collected)
        {

            CoinCollectFilter.coinCollectFilterInstance.eventInstance.start();
            CoinCollectFilter.coinCollectFilterInstance.eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            CoinCollectFilter.coinCollectFilterInstance.SequenceCoinCollected();
            CollectStar();
            Destroy(gameObject);
        }
    }

    private void CollectStar()
    {
        collected = true;
        StarsFunction.instance.ChangeStar(starValue);
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.IncrementStarsCollected(starValue);
        }
    }
}

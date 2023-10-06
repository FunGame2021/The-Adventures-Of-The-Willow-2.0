using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigFlower : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PowerUpCollect, this.transform.position);
            PlayerStates.instance.SetBigState(true);
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public static CheckPoint checkPointInstance;
    private Animator Anim;
    private bool AudioPlayed;
    private float timeToPlayAgain = 5f;
    private Vector3 thisPosition;

    private void Start()
    {
        Anim = GetComponent<Animator>();
        thisPosition = transform.position;
        AudioPlayed = false;
        if (checkPointInstance == null)
        {
            checkPointInstance = this;
        }
    }

    private void OnTriggerEnter2D(Collider2D checkpoint)
    {
        if (checkpoint.gameObject.CompareTag("Player"))
        {
            PlayerHealth.instance.playerPosCheck = thisPosition;
            
            if (!AudioPlayed)
            {
                Anim.SetTrigger("appear");
                AudioManager.instance.PlayOneShot(FMODEvents.instance.CheckPoint, this.transform.position);
                AudioPlayed = true;
                StartCoroutine(ToPlayAgain());
            }
        }
    }
    IEnumerator ToPlayAgain()
    {
        yield return new WaitForSeconds(timeToPlayAgain);
        AudioPlayed = false;
    }
}

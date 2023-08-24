using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [SerializeField] private float toDie = 1f;
    [HideInInspector] public Vector3 playerPosCheck;
    private Vector3 startPlayerPos;
    [SerializeField] private PlayerAnimatorController playerAnimatiorController;
    private bool isDeadNow;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        startPlayerPos = this.transform.position;
    }


    public void TakeDamage()
    {
        if (!isDeadNow)
        {
            if (CoinCollect.instance.coin >= 25)
            {
                isDeadNow = true;
                CoinCollect.instance.ChangeMinusCoin(25);
                PlayerController.instance.stopPlayer = true;
                CheckDie();
            }
            else
            {
                isDeadNow = true;
                PlayerController.instance.stopPlayer = true;
                Die();
            }
        }
    }

    //Die and restart to checkpoint pos if have coins >= 25 and remove 25 coins, if 0 restart to initial pos

    void CheckDie()
    {
        playerAnimatiorController.PlayerDie();
        StartCoroutine(RestartCheckpointDie());
    }
    IEnumerator RestartCheckpointDie()
    {
        yield return new WaitForSeconds(toDie);
        this.transform.position = playerPosCheck;
        isDeadNow = false;
        PlayerController.instance.stopPlayer = false;
    }

    //Die and restart to initial pos
    void Die()
    {
        if (CoinCollect.instance.coin <= 0)
        {
            //AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
            playerAnimatiorController.PlayerDie();
            //PlayerController.instance.isDead = true;
            StartCoroutine(RestartDie());
        }
    }
    void Restart()
    {
        //playerAnimatiorController.PlayerStart();
        //PlayerController.instance.isDead = false;
        this.transform.position = startPlayerPos;
        isDeadNow = false;
        PlayerController.instance.stopPlayer = false;
    }


    IEnumerator RestartDie()
    {
        yield return new WaitForSeconds(toDie);
        Restart();
    }

    #region colliders and triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("immediate_death") && !isDeadNow)
        {
            TakeDamage();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("immediate_death") && !isDeadNow)
        {
            TakeDamage();
        }
    }

    #endregion
}

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
    public bool isDead;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        startPlayerPos = this.transform.position;
    }
    private void Update()
    {
        if(GameStates.Instance.isNormalGame)
        {
            if (LevelTimeManager.instance.remainingDuration <= 0 && playLevel.instance.StartedLevel)
            {
                TakeDamage();
            }
        }
        else
        {
            if (LevelTimeManager.instance.remainingDuration <= 0 && LoadPlayLevel.instance.StartedLevel)
            {
                TakeDamage();
            }
        }
    }

    public void TakeDamage()
    {
        if (!isDeadNow)
        {
            if (PlayerManager.instance != null)
            {
                PlayerManager.instance.IncrementDeaths();
            }
            if (CoinCollect.instance.coin >= 25 && playerPosCheck != null)
            {
                isDead = true;
                isDeadNow = true;
                CoinCollect.instance.ChangeMinusCoin(25);
                PlayerController.instance.stopPlayer = true;
                CheckDie();
            }
            else
            {
                isDead = true;
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
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
        StartCoroutine(RestartCheckpointDie());
    }
    IEnumerator RestartCheckpointDie()
    {
        if (playerPosCheck != null)
        {
            yield return new WaitForSeconds(toDie);
            this.transform.position = playerPosCheck;
            isDeadNow = false;
            PlayerController.instance.stopPlayer = false;
            LevelTimeManager.instance.RestartTimer();
            isDead = false;
        }
        else
        {
            Debug.LogWarning("Checkpoint position is not set!");
        }
    }


    //Die and restart to initial pos
    void Die()
    {
        playerAnimatiorController.PlayerDie();
        //PlayerController.instance.isDead = true;
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Dead, this.transform.position);
        StartCoroutine(RestartDie());
    }
    void Restart()
    {
        //playerAnimatiorController.PlayerStart();
        //PlayerController.instance.isDead = false;
        this.transform.position = startPlayerPos;
        isDeadNow = false;
        PlayerController.instance.stopPlayer = false;
        LevelTimeManager.instance.RestartTimer();
        isDead = false;
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

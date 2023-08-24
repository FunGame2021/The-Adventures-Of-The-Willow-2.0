using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    public static FinishPoint instance;


    private bool isStarted = false;
    [SerializeField] private float toStop = 15f;
    [SerializeField] private float toStopWalking = 15f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float elapsedTime;
    public bool isFinished;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void FinishSequence()
    {
        if (!isStarted)
        {
            elapsedTime = 0;
            PlayerController.instance.RB.velocity = new Vector2(PlayerController.instance.RB.velocity.x, jumpForce);
            // Iniciar a Coroutine para movimento automático
            StartCoroutine(ToStopFireworks());
            isStarted = true;
            isFinished = true;
        }
    }

    private void FixedUpdate()
    {
        if (elapsedTime < toStopWalking)
        {
            isFinished = true;
            elapsedTime += Time.fixedDeltaTime;
        }
        if (elapsedTime >= toStopWalking)
        {
            isFinished = false;
            elapsedTime = toStopWalking + 2;
            PlayerController.instance.stopPlayer = false;
        }

    }

    IEnumerator ToStopFireworks()
    {
        yield return new WaitForSeconds(toStop);
        FinishPole.instance.StopFireworksEffect();
        isStarted = false;
    }
}

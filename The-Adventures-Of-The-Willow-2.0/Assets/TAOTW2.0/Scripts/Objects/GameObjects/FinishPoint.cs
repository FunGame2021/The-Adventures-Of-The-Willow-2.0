using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            PlayerController.instance.RB.linearVelocity = new Vector2(PlayerController.instance.RB.linearVelocity.x, jumpForce);
            // Iniciar a Coroutine para movimento autom�tico
            StartCoroutine(ToStopFireworks());
            isStarted = true;
            isFinished = true;
        }
    }

    private void FixedUpdate()
    {
        if (isFinished)
        {
            if (elapsedTime < toStopWalking)
            {
                CameraZoom.instance.ZoomOutFinish();
                isFinished = true;
                elapsedTime += Time.fixedDeltaTime;
            }
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

        CameraZoom.instance.zoomfinish = false;
        FinishPole.instance.StopFireworksEffect();
        isStarted = false;
        //Load back
        GameObject objectwithscript = GameObject.Find("SceneManager");
        LoadScenes loadscenesScript = objectwithscript.GetComponent<LoadScenes>();
        loadscenesScript.loadSceneEscapeButton();
    }
}

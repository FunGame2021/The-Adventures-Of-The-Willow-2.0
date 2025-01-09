using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }

    private GameObject CameraObj;
    private CinemachineCamera cinemachineVirtualCamera;
    private float shakeTimer;
    private float shakeTimerTotal;
    public float startingIntensity;

    void Awake()
    {
        Instance = this;
    }
	private void Start()
	{
		CameraObj = GameObject.FindGameObjectWithTag("CinemachineCamera");
		cinemachineVirtualCamera = CameraObj.GetComponent<CinemachineCamera>();
	}

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
        cinemachineVirtualCamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;

        startingIntensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }
   
    private void Update()
    {
        if(shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                cinemachineVirtualCamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

            cinemachineBasicMultiChannelPerlin.AmplitudeGain = 
                Mathf.Lerp(startingIntensity, 0f, shakeTimer / shakeTimerTotal);
            
        }
    }
}
